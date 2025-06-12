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

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// HTTPS y CORS
app.UseHttpsRedirection();
app.UseCors();

// Autorización (auth ya incluida en MapIdentityApi)
app.UseAuthorization();

// Mapas de rutas
app.MapIdentityApi<IdentityUser>();
app.MapControllers();

// ------------------------------------------------------
// Inicialización de la base de datos (temporal)
// ------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FundingRequestContext>();
    context.Database.EnsureCreated(); // ⚠️ Migrar esto luego a Migrations
}

app.Run();