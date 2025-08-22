namespace API_Pedidos.Models;

public class FundingRequestAudit
{
    public int Id { get; set; }
    public long FundingRequestId { get; set; }
    public string UserId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public DateTime ChangeDate { get; set; }
    public string Action { get; set; } = null!; // CREATE, UPDATE, CHANGE_STATUS, ADD_COMMENT, UPDATE_PAYMENT
    public string? FieldChanged { get; set; } // Nombre del campo que cambió
    public string? OldValue { get; set; } // Valor anterior
    public string? NewValue { get; set; } // Valor nuevo
    public string? Description { get; set; } // Descripción legible del cambio
}