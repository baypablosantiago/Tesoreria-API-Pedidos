namespace API_Pedidos.Services
{
    public interface IRolesService
    {
        Task<string?> GetUserRoleAsync(string userName);
    }
}