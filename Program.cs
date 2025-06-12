using Microsoft.EntityFrameworkCore;
using API_Pedidos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "API-Pedidos";
    config.Version = "v2";
    
    // Agrega la definición de seguridad JWT
    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Ingrese 'Bearer {su_token_jwt}'"
    });

    // Aplica la definición a todos los endpoints
    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

builder.Services.AddOpenApi();

builder.Services.AddDbContext<FundingRequestContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("XAMMP")));

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<FundingRequestContext>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapIdentityApi<IdentityUser>();

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    FundingRequestContext context = scope.ServiceProvider.GetRequiredService<FundingRequestContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}    

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();