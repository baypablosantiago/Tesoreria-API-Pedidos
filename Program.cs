using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API_Pedidos.Models;
using API_Pedidos.Services;
using API_Pedidos.Data;
using API_Pedidos.Middleware;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.WebHost.UseUrls("http://0.0.0.0:5000");

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
builder.Services.AddScoped<ILoginAuditService, LoginAuditService>();
builder.Services.AddScoped<IFundingRequestAuditService, FundingRequestAuditService>();
builder.Services.AddScoped<IUserDAService, UserDAService>();
builder.Services.AddScoped<IPartialPaymentService, PartialPaymentService>();

// Headers para reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FundingRequestContext>();

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
{
    options.BearerTokenExpiration = TimeSpan.FromHours(6);
    options.RefreshTokenExpiration = TimeSpan.FromMinutes(6); 
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200");
        }
        else
        {
            policy.WithOrigins("https://solicitudesdefondos.entrerios.gov.ar");
        }
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// NO UseHttpsRedirection porque lo maneja el proxy
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders(); 
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoginAuditMiddleware>();

app.MapIdentityApi<IdentityUser>();
app.MapHub<API_Pedidos.Hubs.FundingRequestHub>("/hubs/funding-requests");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FundingRequestContext>();
    
    context.Database.EnsureCreated();
    
    await DatabaseSeeder.SeedAsync(services);
}

app.Run();
