namespace API_Pedidos.Services
{
    public interface IUserDAService
    {
        Task<IEnumerable<int>> GetUserDANumbersAsync(string userId);
    }
}