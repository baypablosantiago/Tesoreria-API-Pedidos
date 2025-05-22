namespace API_Pedidos.Models;

public class FundingRequest
{
    public long Id { get; set; }

    public int DA { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public int FiscalYear { get; set; }

    public string PaymentOrderNumber { get; set; } = string.Empty;

    public string Concept { get; set; } = string.Empty;

    public string DueDate { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string FundingSource { get; set; } = string.Empty;

    public string CheckingAccount { get; set; } = string.Empty;

    public string? Comments { get; set; }
}