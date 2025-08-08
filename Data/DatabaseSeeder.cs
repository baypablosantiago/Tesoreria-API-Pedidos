using Microsoft.AspNetCore.Identity;

namespace API_Pedidos.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await CreateRolesAsync(roleManager);

            await CreateUsersAsync(userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "user", "admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    var result = await roleManager.CreateAsync(identityRole);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"No se pudo crear el rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task CreateUsersAsync(UserManager<IdentityUser> userManager)
        {
            var adminEmail = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD");
            var userEmail = Environment.GetEnvironmentVariable("DEFAULT_USER_EMAIL");
            var userPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                await CreateUserIfNotExistsAsync(userManager, adminEmail, adminPassword, "admin");
            }

            if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userPassword))
            {
                await CreateUserIfNotExistsAsync(userManager, userEmail, userPassword, "user");
            }
        }

        private static async Task CreateUserIfNotExistsAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(newUser, password);
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Error al crear usuario '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addToRoleResult = await userManager.AddToRoleAsync(newUser, role);
                if (!addToRoleResult.Succeeded)
                {
                    throw new Exception($"Error al asignar rol '{role}' a usuario '{email}': {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}