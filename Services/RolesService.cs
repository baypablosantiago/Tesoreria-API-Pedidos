using Microsoft.AspNetCore.Identity;

namespace API_Pedidos.Services
{
    public class RolesService : IRolesService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RolesService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string?> GetUserRoleAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }
    }
}