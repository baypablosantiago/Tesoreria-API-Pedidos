namespace API_Pedidos.Models;

public class LoginAudit
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime LoginDate { get; set; }
    public string IpAddress { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}