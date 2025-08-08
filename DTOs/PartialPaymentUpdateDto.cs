using System.ComponentModel.DataAnnotations;

namespace API_Pedidos.DTOs
{
    public class PartialPaymentUpdateDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "El pago parcial debe ser un valor positivo")]
        public double PartialPayment { get; set; }
    }
}