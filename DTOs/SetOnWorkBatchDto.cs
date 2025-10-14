namespace API_Pedidos.DTOs
{
    public class SetOnWorkBatchDto
    {
        public required List<long> RequestIds { get; set; }
        public required bool OnWork { get; set; }
    }
}
