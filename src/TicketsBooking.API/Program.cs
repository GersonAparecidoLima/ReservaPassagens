using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TicketsBooking.Application.Services;
using TicketsBooking.Core.Interfaces;
using TicketsBooking.Infrastructure.Cache;
using TicketsBooking.Infrastructure.Data.Context;
using TicketsBooking.Infrastructure.Messaging;

// 1. Cria o builder apenas UMA vez
var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 2. Adiciona os serviços ao container (DI)
// =================================================================

// Suporte para Controllers
builder.Services.AddControllers();

// Configuração do OpenAPI (.NET 9 nativo para documentação da API)
builder.Services.AddOpenApi();

// Configuração do Entity Framework (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração ÚNICA do MassTransit (Padrão In-Memory para o desafio)
builder.Services.AddMassTransit(x =>
{
    // Desativa a trava de licença da V9 dinamicamente (ignorado se for V8)
    var licensingType = Type.GetType("MassTransit.Licensing.LicenseAssignment, MassTransit");
    if (licensingType != null)
    {
        var setLicenseMethod = licensingType.GetMethod("SetLicense", new[] { typeof(string) });
        setLicenseMethod?.Invoke(null, new object[] { "MT-Community" });
    }

    // Registra o seu consumidor que vai processar a fila em background
    x.AddConsumer<BookingCreatedConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// Configuração do Redis (Lock distribuído para controle de assentos)
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Configuração dos Serviços de Aplicação
builder.Services.AddScoped<BookingApplicationService>();

// =================================================================
// 3. Constrói a aplicação após registrar TODOS os serviços
// =================================================================
var app = builder.Build();

// =================================================================
// 4. Configura o pipeline de requisições HTTP (Middlewares)
// =================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Middleware de autorização
app.UseAuthorization();

// Mapeia automaticamente todas as Controllers (incluindo a BookingsController)
app.MapControllers();

// --- Bloco adicionado para aplicar migrations automaticamente ao iniciar ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Cria o banco e aplica as tabelas se elas não existirem
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations no banco.");
    }
}
// ---------------------------------------------------------------------------

// 5. Roda a aplicação
app.Run();