using API_Pedidos.DTOs;
using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using API_Pedidos.Hubs;

namespace API_Pedidos.Services
{
    public class FundingRequestService : IFundingRequestService
    {
        private readonly FundingRequestContext _context;
        private readonly IFundingRequestAuditService _auditService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPartialPaymentService _partialPaymentService;
        private readonly IHubContext<FundingRequestHub> _hubContext;
        private readonly IAdminNotificationService _notificationService;

        public FundingRequestService(FundingRequestContext context, IFundingRequestAuditService auditService, UserManager<IdentityUser> userManager, IPartialPaymentService partialPaymentService, IHubContext<FundingRequestHub> hubContext, IAdminNotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
            _partialPaymentService = partialPaymentService;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        private async Task<bool> IsUserAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "admin");
        }

        public async Task<FundingRequestResponseDto> AddFundingRequestAsync(FundingRequestCreateDto newFundingRequest, string userId)
        {
            var entity = FundingRequestMapper.ToEntity(newFundingRequest);
            entity.UserId = userId;

            var existingRequest = await _context.Requests
                .Where(fr => fr.UserId == userId &&
                            fr.DA == entity.DA &&
                            fr.RequestNumber == entity.RequestNumber &&
                            fr.FiscalYear == entity.FiscalYear &&
                            fr.PaymentOrderNumber == entity.PaymentOrderNumber &&
                            fr.Concept == entity.Concept &&
                            fr.DueDate == entity.DueDate &&
                            fr.Amount == entity.Amount &&
                            fr.FundingSource == entity.FundingSource &&
                            fr.CheckingAccount == entity.CheckingAccount)
                .FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                throw new InvalidOperationException("Ya existe una solicitud idéntica con los mismos datos.");
            }

            _context.Add(entity);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            await _auditService.LogCreateAsync(entity.Id, userId, user?.Email ?? "Unknown");

            var result = FundingRequestMapper.ToAdminResponseDto(entity);

            // Enviar actualización del dashboard
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // Crear notificación detallada
            var notification = new FundingRequestChangeNotificationDto
            {
                RequestId = entity.Id,
                RequestNumber = entity.RequestNumber,
                DA = entity.DA,
                ChangeType = "CREATE",
                ChangeDate = DateTime.UtcNow,
                UserEmail = user?.Email ?? "Unknown",
                FullRequest = result
            };

            // Guardar notificación en DB para todos los admins
            await _notificationService.CreateNotificationForAllAdminsAsync(notification);

            // Enviar por SignalR
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestNotification", notification);

            return FundingRequestMapper.ToResponseDto(entity);
        }

        public async Task<IEnumerable<FundingRequestResponseDto>> GetUserFundingRequestsAsync(string userId)
        {
            var entities = await _context.Requests
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.RequestNumber)
                .ToListAsync();

            return FundingRequestMapper.ToResponseDtoList(entities);
        }

        public async Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllActiveFundingRequestsAsync()
        {
            var entities = await _context.Requests
                .Where(fr => fr.IsActive)
                .OrderBy(fr => fr.RequestNumber)
                .ToListAsync();
                
            return FundingRequestMapper.ToAdminResponseDtoList(entities);
        }

        public async Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllInactiveFundingRequestsAsync()
        {
            var entities = await _context.Requests
                .Where(fr => !fr.IsActive)
                .OrderBy(fr => fr.RequestNumber)
                .ToListAsync();
                
            return FundingRequestMapper.ToAdminResponseDtoList(entities);
        }

        public async Task<FundingRequestAdminResponseDto?> UpdatePartialPaymentAsync(long id, double newPartialPayment, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var userEmail = currentUser?.Email ?? "Unknown";

            // Crear nuevo registro en PartialPayments
            await _partialPaymentService.CreatePartialPaymentAsync(id, (decimal)newPartialPayment, currentUserId, userEmail);

            var oldPayment = fundingRequest.PartialPayment;

            // Recalcular el total de pagos parciales
            var totalPartialPayment = await _partialPaymentService.GetTotalPartialPaymentAsync(id);
            fundingRequest.PartialPayment = (double)totalPartialPayment;

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            await _auditService.LogPaymentUpdateAsync(id, currentUserId, userEmail, oldPayment, (double)totalPartialPayment);

            var result = FundingRequestMapper.ToAdminResponseDto(fundingRequest);

            // Siempre enviar actualización del dashboard
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // NO enviar notificación (acción de admin)

            return result;
        }

        public async Task<FundingRequestAdminResponseDto?> ChangeIsActiveAsync(long id, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            var wasActive = fundingRequest.IsActive;
            fundingRequest.IsActive = !fundingRequest.IsActive;
            
            if (!fundingRequest.IsActive)
            {
                fundingRequest.OnWork = false;
            }

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var action = fundingRequest.IsActive ? "ACTIVATE" : "DEACTIVATE";
            var description = fundingRequest.IsActive ? "Solicitud activada" : "Solicitud desactivada";
            await _auditService.LogStatusChangeAsync(id, currentUserId, currentUser?.Email ?? "Unknown", action, description);

            var result = FundingRequestMapper.ToAdminResponseDto(fundingRequest);

            // Siempre enviar actualización del dashboard
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // NO enviar notificación (acción de admin)

            return result;
        }

        public async Task<FundingRequestAdminResponseDto?> ChangeOnWorkAsync(long id, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            var wasOnWork = fundingRequest.OnWork;
            fundingRequest.OnWork = !fundingRequest.OnWork;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var action = "CHANGE_WORK_STATUS";
            var description = fundingRequest.OnWork ? "Solicitud marcada como 'en trabajo'" : "Solicitud desmarcada de 'en trabajo'";
            await _auditService.LogStatusChangeAsync(id, currentUserId, currentUser?.Email ?? "Unknown", action, description);

            var result = FundingRequestMapper.ToAdminResponseDto(fundingRequest);

            // Siempre enviar actualización del dashboard
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // NO enviar notificación (acción de admin)

            return result;
        }

        public async Task<FundingRequestAdminResponseDto?> AddCommentAsync(long id, string comment, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            var oldComment = fundingRequest.CommentsFromTeso;
            fundingRequest.CommentsFromTeso = comment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            await _auditService.LogCommentAsync(id, currentUserId, currentUser?.Email ?? "Unknown", comment);

            var result = FundingRequestMapper.ToAdminResponseDto(fundingRequest);

            // Siempre enviar actualización del dashboard
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // NO enviar notificación (acción de admin)

            return result;
        }

        public async Task<FundingRequestResponseDto?> UpdateFundingRequestAsync(FundingRequestUpdateDto dto, string userId)
        {
            var request = await _context.Requests.FindAsync(dto.Id);
            if (request == null)
                return null;

            if (request.UserId != userId)
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            var userEmail = user?.Email ?? "Unknown";

            // Recolectar cambios para notificación
            var changes = new List<(string field, string oldValue, string newValue)>();

            // Auditar cambios campo por campo
            if (request.RequestNumber != dto.RequestNumber)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "RequestNumber", request.RequestNumber.ToString(), dto.RequestNumber.ToString());
                changes.Add(("N° de Solicitud", request.RequestNumber.ToString(), dto.RequestNumber.ToString()));
            }

            if (request.FiscalYear != dto.FiscalYear)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "FiscalYear", request.FiscalYear.ToString(), dto.FiscalYear.ToString());
                changes.Add(("Ejercicio", request.FiscalYear.ToString(), dto.FiscalYear.ToString()));
            }

            if (request.PaymentOrderNumber != dto.PaymentOrderNumber)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "PaymentOrderNumber", request.PaymentOrderNumber, dto.PaymentOrderNumber);
                changes.Add(("N° Orden de Pago", request.PaymentOrderNumber, dto.PaymentOrderNumber));
            }

            if (request.Concept != dto.Concept)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Concept", request.Concept, dto.Concept);
                changes.Add(("Concepto", request.Concept, dto.Concept));
            }

            if (request.Amount != dto.Amount)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Amount", request.Amount.ToString("C"), dto.Amount.ToString("C"));
                changes.Add(("Monto", request.Amount.ToString("C"), dto.Amount.ToString("C")));
            }

            if (request.FundingSource != dto.FundingSource)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "FundingSource", request.FundingSource, dto.FundingSource);
                changes.Add(("Fuente de Financiamiento", request.FundingSource, dto.FundingSource));
            }

            if (request.CheckingAccount != dto.CheckingAccount)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "CheckingAccount", request.CheckingAccount, dto.CheckingAccount);
                changes.Add(("Cuenta Corriente", request.CheckingAccount, dto.CheckingAccount));
            }

            if (request.DueDate != dto.DueDate)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "DueDate", request.DueDate, dto.DueDate);
                changes.Add(("Vencimiento", request.DueDate, dto.DueDate));
            }

            if (request.Comments != dto.Comments)
            {
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Comments", request.Comments ?? "", dto.Comments ?? "");
                changes.Add(("Comentarios", request.Comments ?? "(vacío)", dto.Comments ?? "(vacío)"));
            }

            // Aplicar cambios
            request.RequestNumber = dto.RequestNumber;
            request.FiscalYear = dto.FiscalYear;
            request.PaymentOrderNumber = dto.PaymentOrderNumber;
            request.Concept = dto.Concept;
            request.Amount = dto.Amount;
            request.FundingSource = dto.FundingSource;
            request.CheckingAccount = dto.CheckingAccount;
            request.DueDate = dto.DueDate;
            request.Comments = dto.Comments;

            _context.Requests.Update(request);
            await _context.SaveChangesAsync();

            var result = FundingRequestMapper.ToAdminResponseDto(request);

            // Enviar actualización del dashboard (una sola vez)
            await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", result);

            // Enviar notificaciones detalladas para cada cambio
            foreach (var change in changes)
            {
                var notification = new FundingRequestChangeNotificationDto
                {
                    RequestId = request.Id,
                    RequestNumber = request.RequestNumber,
                    DA = request.DA,
                    ChangeType = "UPDATE",
                    FieldChanged = change.field,
                    OldValue = change.oldValue,
                    NewValue = change.newValue,
                    ChangeDate = DateTime.UtcNow,
                    UserEmail = userEmail,
                    FullRequest = result
                };

                // Guardar en DB para todos los admins
                await _notificationService.CreateNotificationForAllAdminsAsync(notification);

                // Enviar por SignalR
                await _hubContext.Clients.Group("admins").SendAsync("FundingRequestNotification", notification);
            }

            return FundingRequestMapper.ToResponseDto(request);
        }
    }
}