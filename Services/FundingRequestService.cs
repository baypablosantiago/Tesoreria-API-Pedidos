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

        public async Task<FundingRequest> AddFundingRequestAsync(FundingRequest newFundingRequest, string userId)
        {
            newFundingRequest.UserId = userId;
            _context.Add(newFundingRequest);
            await _context.SaveChangesAsync();
            return newFundingRequest;
        }

        public async Task<IEnumerable<FundingRequest>> GetUserFundingRequestsAsync(string userId)
        {
            return await _context.Requests
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FundingRequest>> GetAllActiveFundingRequestsAsync()
        {
            return await _context.Requests
                .Where(fr => fr.IsActive)
                .OrderByDescending(fr => fr.ReceivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FundingRequest>> GetAllInactiveFundingRequestsAsync()
        {
            return await _context.Requests
                .Where(fr => !fr.IsActive)
                .OrderByDescending(fr => fr.ReceivedAt)
                .ToListAsync();
        }

        public async Task<FundingRequest?> UpdatePartialPaymentAsync(long id, double newPartialPayment)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.PartialPayment = newPartialPayment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return fundingRequest;
        }

        public async Task<FundingRequest?> ChangeIsActiveAsync(long id)
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

            return fundingRequest;
        }

        public async Task<FundingRequest?> ChangeOnWorkAsync(long id)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.OnWork = !fundingRequest.OnWork;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return fundingRequest;
        }

        public async Task<FundingRequest?> AddCommentAsync(long id, string comment)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
                return null;

            fundingRequest.CommentsFromTeso = comment;
            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return fundingRequest;
        }

        public async Task<bool> UpdateFundingRequestAsync(FundingRequestUpdateDto dto)
        {
            var request = await _context.Requests.FindAsync(dto.Id);
            if (request == null)
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