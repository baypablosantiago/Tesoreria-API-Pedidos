using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pedidos.Models;

public class FundingRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }


    [Required]
    public int DA { get; set; }


    [Required]
    public int RequestNumber { get; set; }


    [Required]
    public int FiscalYear { get; set; }


    [Required]
    public int PaymentOrderNumber { get; set; }


    [Required]
    [MaxLength(500)]
    public string Concept { get; set; } = string.Empty;


    [Required]
    [MaxLength(20)]
    public string DueDate { get; set; } = string.Empty;


    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }


    [Required]
    [MaxLength(20)]
    public string FundingSource { get; set; } = string.Empty;


    [Required]
    [MaxLength(50)]
    public string CheckingAccount { get; set; } = string.Empty;


    [MaxLength(500)]
    public string? Comments { get; set; }


    public bool IsActive { get; set; } = true;


    public string UserId { get; set; } = string.Empty;
}