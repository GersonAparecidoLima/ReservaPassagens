using Microsoft.EntityFrameworkCore;
using TicketsBooking.Infrastructure.Data.Context;

// 1. Cria o builder apenas UMA vez
var builder = WebApplication.CreateBuilder(args);

// 2. Adiciona os serviços ao container (DbContext, OpenAPI, etc.)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

// 3. Constrói a aplicação após registrar todos os serviços
var app = builder.Build();

// 4. Configura o pipeline de requisições HTTP (Middlewares)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Dados de exemplo para o WeatherForecast
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Endpoints da API
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// 5. Roda a aplicação
app.Run();

// Registro do Record (pode ficar no final do arquivo sem problemas)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}