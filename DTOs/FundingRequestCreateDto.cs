using System.ComponentModel.DataAnnotations;

namespace API_Pedidos.DTOs
{
    public class FundingRequestCreateDto
    {
        [Required]
        public int DA { get; set; }

        [Required]
        public int RequestNumber { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        [Required]
        public string PaymentOrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Concept { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string DueDate { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string FundingSource { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CheckingAccount { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comments { get; set; }
    }
}