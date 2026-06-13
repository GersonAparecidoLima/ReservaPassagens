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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CONFIGURAÇÃO DO CORS: Permite que o frontend React acesse a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



// Configuração do Entity Framework (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração ÚNICA do MassTransit (Agora configurado com RabbitMQ)
// Configuração ÚNICA do MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    // Compatibilidade com MassTransit Community Edition
    var licensingType = Type.GetType("MassTransit.Licensing.LicenseAssignment, MassTransit");
    if (licensingType != null)
    {
        var setLicenseMethod = licensingType.GetMethod("SetLicense", new[] { typeof(string) });
        setLicenseMethod?.Invoke(null, new object[] { "MT-Community" });
    }

    // Registra o Consumer
    x.AddConsumer<BookingCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.ReceiveEndpoint("booking-created", e =>
        {
            e.ConfigureConsumer<BookingCreatedConsumer>(context);
        });
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// MIDDLEWARE DE CORS: Deve vir antes da Autorização e dos Controllers
app.UseCors("ViteFrontend");

// Middleware de autorização
app.UseAuthorization();


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