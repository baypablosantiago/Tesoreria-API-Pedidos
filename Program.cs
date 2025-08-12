using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API_Pedidos.Models;
using API_Pedidos.Services;
using API_Pedidos.Data;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddControllers();

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

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LOCALHOST");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string not found. Make sure ConnectionStrings__LOCALHOST is set in your .env file.");

builder.Services.AddDbContext<FundingRequestContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IFundingRequestService, FundingRequestService>();
builder.Services.AddScoped<IRolesService, RolesService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FundingRequestContext>();

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


if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapIdentityApi<IdentityUser>();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FundingRequestContext>();
    
    context.Database.EnsureCreated();
    
    await DatabaseSeeder.SeedAsync(services);
}

app.Run();