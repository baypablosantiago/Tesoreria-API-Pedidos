using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pedidos.Models;

public class PartialPayment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public long FundingRequestId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string CreatedByUserEmail { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey(nameof(FundingRequestId))]
    public FundingRequest? FundingRequest { get; set; }
}