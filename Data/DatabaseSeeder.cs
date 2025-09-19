using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API_Pedidos.Models;

namespace API_Pedidos.Data
{
    public static class DatabaseSeeder
    {
        
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<FundingRequestContext>();

            await CreateRolesAsync(roleManager);
            await CreateUsersAsync(userManager);
            await CreateUserDAsAsync(context, userManager);
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
            foreach (var user in PredefinedUsers)
            {
                await CreateUserIfNotExistsAsync(userManager, user.Email, user.Password, user.Role);
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
            else
            {
                var passwordValid = await userManager.CheckPasswordAsync(existingUser, password);
                if (!passwordValid)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existingUser);
                    var result = await userManager.ResetPasswordAsync(existingUser, token, password);

                    if (!result.Succeeded)
                    {
                        throw new Exception($"Error al actualizar contraseña para '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task CreateUserDAsAsync(FundingRequestContext context, UserManager<IdentityUser> userManager)
        {
            foreach (var userDA in PredefinedUserDAs)
            {
                var user = await userManager.FindByEmailAsync(userDA.Email);
                if (user != null)
                {
                    foreach (var daNumber in userDA.DANumbers)
                    {
                        var existingDA = await context.UserDAs
                            .FirstOrDefaultAsync(d => d.UserId == user.Id && d.DANumber == daNumber);

                        if (existingDA == null)
                        {
                            context.UserDAs.Add(new UserDA
                            {
                                UserId = user.Id,
                                DANumber = daNumber,
                                Description = userDA.Description,
                                IsActive = true
                            });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private record UserSeed(string Email, string Password, string Role);
        private record UserDASeed(string Email, int[] DANumbers, string? Description);
    }
}