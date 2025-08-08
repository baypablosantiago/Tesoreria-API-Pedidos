using System.ComponentModel.DataAnnotations;

namespace API_Pedidos.DTOs
{
    public class CommentsFromTesoDto
    {
        [Required]
        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}