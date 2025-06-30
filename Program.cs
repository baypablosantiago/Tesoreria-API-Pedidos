using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API_Pedidos.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Servicios
// ------------------------------------------------------

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "API-Pedidos";
    config.Version = "v2";

    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Ingrese 'Bearer {su_token}'"
    });

    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

// Base de datos
var connectionString = builder.Configuration.GetConnectionString("XAMMP")
    ?? throw new InvalidOperationException("Connection string 'XAMMP' not found.");
builder.Services.AddDbContext<FundingRequestContext>(options =>
    options.UseMySQL(connectionString));

// Identity
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FundingRequestContext>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// ------------------------------------------------------
// Middleware
// ------------------------------------------------------

// Swagger para el en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// HTTPS y CORS
app.UseHttpsRedirection();
app.UseCors();

// Autorización (la authentication ya esta incluida en MapIdentityApi)
app.UseAuthorization();

// Mapas de rutas
app.MapIdentityApi<IdentityUser>();
app.MapControllers();

// ------------------------------------------------------
// Inicialización de la base de datos, roles y usuarios (TEMPORAL)
// ------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<FundingRequestContext>();
    context.Database.EnsureCreated();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetService<RoleManager<IdentityRole>>();
    if (roleManager == null)
        throw new Exception("No se pudo resolver RoleManager<IdentityRole>");

    string[] roles = new[] { "user", "admin" };

    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
        {
            var identityRole = new IdentityRole
            {
                Name = role,
                NormalizedName = role.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = roleManager.CreateAsync(identityRole).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception($"No se pudo crear el rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    var users = new[]
    {
        new { Email = "user@test.com", Password = "!User123", Role = "user" },
        new { Email = "admin@test.com", Password = "!Admin123", Role = "admin" }
    };

    foreach (var u in users)
    {
        var existingUser = userManager.FindByEmailAsync(u.Email).GetAwaiter().GetResult();

        if (existingUser == null)
        {
            var newUser = new IdentityUser
            {
                UserName = u.Email,
                Email = u.Email
            };

            var createResult = userManager.CreateAsync(newUser, u.Password).GetAwaiter().GetResult();
            if (!createResult.Succeeded)
            {
                throw new Exception($"Error al crear usuario '{u.Email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var addToRoleResult = userManager.AddToRoleAsync(newUser, u.Role).GetAwaiter().GetResult();
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception($"Error al asignar rol '{u.Role}' a usuario '{u.Email}': {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}

app.Run();