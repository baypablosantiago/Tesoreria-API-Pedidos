using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Pedidos.Models;

public class UserDA
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int DANumber { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}