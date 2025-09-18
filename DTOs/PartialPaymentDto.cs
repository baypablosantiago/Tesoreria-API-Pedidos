namespace API_Pedidos.DTOs
{
    public class PartialPaymentDto
    {
        public int Id { get; set; }
        public long FundingRequestId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}