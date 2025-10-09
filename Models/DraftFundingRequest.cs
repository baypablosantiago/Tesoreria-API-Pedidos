using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pedidos.Models;

public class DraftFundingRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "jsonb")]
    public string DraftData { get; set; } = string.Empty;

    [Required]
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
}
