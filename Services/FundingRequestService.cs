using API_Pedidos.DTOs;
using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Services
{
    public class FundingRequestService : IFundingRequestService
    {
        private readonly FundingRequestContext _context;

        public FundingRequestService(FundingRequestContext context)
        {
            _context = context;
        }

        public async Task<FundingRequestResponseDto> AddFundingRequestAsync(FundingRequestCreateDto newFundingRequest, string userId)
        {
            var entity = FundingRequestMapper.ToEntity(newFundingRequest);
            entity.UserId = userId;
            
            _context.Add(entity);
            await _context.SaveChangesAsync();
            
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

        public async Task<FundingRequestAdminResponseDto?> UpdatePartialPaymentAsync(long id, double newPartialPayment)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.PartialPayment = newPartialPayment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<FundingRequestAdminResponseDto?> ChangeIsActiveAsync(long id)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.IsActive = !fundingRequest.IsActive;
            
            if (!fundingRequest.IsActive)
            {
                fundingRequest.OnWork = false;
            }

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<FundingRequestAdminResponseDto?> ChangeOnWorkAsync(long id)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.OnWork = !fundingRequest.OnWork;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<FundingRequestAdminResponseDto?> AddCommentAsync(long id, string comment)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.CommentsFromTeso = comment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return FundingRequestMapper.ToAdminResponseDto(fundingRequest);
        }

        public async Task<bool> UpdateFundingRequestAsync(FundingRequestUpdateDto dto, string userId)
        {
            var request = await _context.Requests.FindAsync(dto.Id);
            if (request == null)
                return false;

            if (request.UserId != userId)
                return false;

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