using API_Pedidos.DTOs;
using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace API_Pedidos.Services
{
    public class FundingRequestService : IFundingRequestService
    {
        private readonly FundingRequestContext _context;
        private readonly IFundingRequestAuditService _auditService;
        private readonly UserManager<IdentityUser> _userManager;

        public FundingRequestService(FundingRequestContext context, IFundingRequestAuditService auditService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
        }

        public async Task<FundingRequestResponseDto> AddFundingRequestAsync(FundingRequestCreateDto newFundingRequest, string userId)
        {
            var entity = FundingRequestMapper.ToEntity(newFundingRequest);
            entity.UserId = userId;
            
            _context.Add(entity);
            await _context.SaveChangesAsync();
            
            // Auditar creación
            var user = await _userManager.FindByIdAsync(userId);
            await _auditService.LogCreateAsync(entity.Id, userId, user?.Email ?? "Unknown");
            
            return FundingRequestMapper.ToResponseDto(entity);
        }

        public async Task<IEnumerable<FundingRequestResponseDto>> GetUserFundingRequestsAsync(string userId)
        {
            var entities = await _context.Requests
                .Where(r => r.UserId == userId)
                .ToListAsync();
                
            return FundingRequestMapper.ToResponseDtoList(entities);
        }

        public async Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllActiveFundingRequestsAsync()
        {
            var entities = await _context.Requests
                .Where(fr => fr.IsActive)
                .OrderByDescending(fr => fr.ReceivedAt)
                .ToListAsync();
                
            return FundingRequestMapper.ToAdminResponseDtoList(entities);
        }

        public async Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllInactiveFundingRequestsAsync()
        {
            var entities = await _context.Requests
                .Where(fr => !fr.IsActive)
                .OrderByDescending(fr => fr.ReceivedAt)
                .ToListAsync();
                
            return FundingRequestMapper.ToAdminResponseDtoList(entities);
        }

        public async Task<FundingRequestAdminResponseDto?> UpdatePartialPaymentAsync(long id, double newPartialPayment, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            var oldPayment = fundingRequest.PartialPayment;
            fundingRequest.PartialPayment = newPartialPayment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            // Auditar cambio de pago
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            await _auditService.LogPaymentUpdateAsync(id, currentUserId, currentUser?.Email ?? "Unknown", oldPayment, newPartialPayment);

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
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

            // Auditar cambio de estado
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var action = fundingRequest.IsActive ? "ACTIVATE" : "DEACTIVATE";
            var description = fundingRequest.IsActive ? "Solicitud activada" : "Solicitud desactivada";
            await _auditService.LogStatusChangeAsync(id, currentUserId, currentUser?.Email ?? "Unknown", action, description);

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<FundingRequestAdminResponseDto?> ChangeOnWorkAsync(long id, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.OnWork = !fundingRequest.OnWork;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            // Auditar cambio de estado "en trabajo"
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var action = "CHANGE_WORK_STATUS";
            var description = fundingRequest.OnWork ? "Solicitud marcada como 'en trabajo'" : "Solicitud desmarcada de 'en trabajo'";
            await _auditService.LogStatusChangeAsync(id, currentUserId, currentUser?.Email ?? "Unknown", action, description);

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<FundingRequestAdminResponseDto?> AddCommentAsync(long id, string comment, string currentUserId)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.CommentsFromTeso = comment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            // Auditar comentario
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            await _auditService.LogCommentAsync(id, currentUserId, currentUser?.Email ?? "Unknown", comment);

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<bool> UpdateFundingRequestAsync(FundingRequestUpdateDto dto, string userId)
        {
            var request = await _context.Requests.FindAsync(dto.Id);
            if (request == null)
                return false;

            if (request.UserId != userId)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            var userEmail = user?.Email ?? "Unknown";

            // Auditar cambios campo por campo
            if (request.RequestNumber != dto.RequestNumber)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "RequestNumber", request.RequestNumber.ToString(), dto.RequestNumber.ToString());
            
            if (request.FiscalYear != dto.FiscalYear)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "FiscalYear", request.FiscalYear.ToString(), dto.FiscalYear.ToString());
            
            if (request.PaymentOrderNumber != dto.PaymentOrderNumber)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "PaymentOrderNumber", request.PaymentOrderNumber, dto.PaymentOrderNumber);
            
            if (request.Concept != dto.Concept)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Concept", request.Concept, dto.Concept);
            
            if (request.Amount != dto.Amount)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Amount", request.Amount.ToString("C"), dto.Amount.ToString("C"));
            
            if (request.FundingSource != dto.FundingSource)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "FundingSource", request.FundingSource, dto.FundingSource);
            
            if (request.CheckingAccount != dto.CheckingAccount)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "CheckingAccount", request.CheckingAccount, dto.CheckingAccount);
            
            if (request.DueDate != dto.DueDate)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "DueDate", request.DueDate, dto.DueDate);
            
            if (request.Comments != dto.Comments)
                await _auditService.LogUpdateAsync(dto.Id, userId, userEmail, "Comments", request.Comments ?? "", dto.Comments ?? "");

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

            return true;
        }
    }
}