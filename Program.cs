using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API_Pedidos.Models;
using API_Pedidos.Services;
using API_Pedidos.Data;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
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
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LOCALHOST");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string not found. Make sure ConnectionStrings__LOCALHOST is set in your .env file.");

builder.Services.AddDbContext<FundingRequestContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<IFundingRequestService, FundingRequestService>();
builder.Services.AddScoped<IRolesService, RolesService>();

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
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
// Inicialización de la base de datos y seeding
// ------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FundingRequestContext>();
    
    // Asegurar que la base de datos existe
    context.Database.EnsureCreated();
    
    // Ejecutar seeding de forma segura
    await DatabaseSeeder.SeedAsync(services);
}

app.Run();