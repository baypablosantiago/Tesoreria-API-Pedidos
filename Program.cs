using Microsoft.EntityFrameworkCore;
using API_Pedidos.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<FundingRequestContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("XAMMP"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("XAMMP"))
    ));

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

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    FundingRequestContext context = scope.ServiceProvider.GetRequiredService<FundingRequestContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();