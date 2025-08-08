using API_Pedidos.DTOs;
using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public static class FundingRequestMapper
    {
        public static FundingRequest ToEntity(FundingRequestCreateDto dto)
        {
            return new FundingRequest
            {
                DA = dto.DA,
                RequestNumber = dto.RequestNumber,
                FiscalYear = dto.FiscalYear,
                PaymentOrderNumber = dto.PaymentOrderNumber,
                Concept = dto.Concept,
                DueDate = dto.DueDate,
                Amount = dto.Amount,
                FundingSource = dto.FundingSource,
                CheckingAccount = dto.CheckingAccount,
                Comments = dto.Comments,
                ReceivedAt = DateTime.UtcNow,
                IsActive = true,
                OnWork = false,
                PartialPayment = 0
            };
        }

        public static FundingRequestResponseDto ToResponseDto(FundingRequest entity)
        {
            return new FundingRequestResponseDto
            {
                Id = entity.Id,
                ReceivedAt = entity.ReceivedAt,
                DA = entity.DA,
                RequestNumber = entity.RequestNumber,
                FiscalYear = entity.FiscalYear,
                PaymentOrderNumber = entity.PaymentOrderNumber,
                Concept = entity.Concept,
                DueDate = entity.DueDate,
                Amount = entity.Amount,
                FundingSource = entity.FundingSource,
                CheckingAccount = entity.CheckingAccount,
                Comments = entity.Comments,
                CommentsFromTeso = entity.CommentsFromTeso,
                PartialPayment = entity.PartialPayment,
                IsActive = entity.IsActive,
                OnWork = entity.OnWork
            };
        }

        public static FundingRequestAdminResponseDto ToAdminResponseDto(FundingRequest entity)
        {
            return new FundingRequestAdminResponseDto
            {
                Id = entity.Id,
                ReceivedAt = entity.ReceivedAt,
                DA = entity.DA,
                RequestNumber = entity.RequestNumber,
                FiscalYear = entity.FiscalYear,
                PaymentOrderNumber = entity.PaymentOrderNumber,
                Concept = entity.Concept,
                DueDate = entity.DueDate,
                Amount = entity.Amount,
                FundingSource = entity.FundingSource,
                CheckingAccount = entity.CheckingAccount,
                Comments = entity.Comments,
                CommentsFromTeso = entity.CommentsFromTeso,
                PartialPayment = entity.PartialPayment,
                IsActive = entity.IsActive,
                OnWork = entity.OnWork,
                UserId = entity.UserId
            };
        }

        public static IEnumerable<FundingRequestResponseDto> ToResponseDtoList(IEnumerable<FundingRequest> entities)
        {
            return entities.Select(ToResponseDto);
        }

        public static IEnumerable<FundingRequestAdminResponseDto> ToAdminResponseDtoList(IEnumerable<FundingRequest> entities)
        {
            return entities.Select(ToAdminResponseDto);
        }
    }
}