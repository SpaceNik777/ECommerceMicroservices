using Microsoft.EntityFrameworkCore;
using OrderService.API.Endpoints;
using OrderService.Application.Services;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;
using Confluent.Kafka;
using OrderService.Domain;
using OrderService.Application;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string for PostgreSQL not found in environment variables. Ensure Vault mutating webhook injects it.");
}
builder.Services.AddDbContext<OrderService.Infrastructure.Persistence.OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Kafka
builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var config = new Confluent.Kafka.ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    };
    var logger = sp.GetRequiredService<ILogger<KafkaEventPublisher>>();
    return new KafkaEventPublisher(builder.Configuration, logger);
});

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register HTTP clients
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UserService"] ?? "http://user-service");
});
builder.Services.AddTransient<IUserService>(provider =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("UserService");
    var baseUrl = builder.Configuration["Services:UserService"] ?? "http://user-service";
    return new UserServiceClient(client, baseUrl);
});

builder.Services.AddHttpClient("InventoryService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryService"] ?? "http://inventory-service");
});
builder.Services.AddTransient<IInventoryService>(provider =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("InventoryService");
    var baseUrl = builder.Configuration["Services:InventoryService"] ?? "http://inventory-service";
    return new InventoryServiceClient(client, baseUrl);
});

// Register application services
builder.Services.AddScoped<OrderService.Domain.Interfaces.IOrderService, OrderService.Application.Services.OrderService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderService.Infrastructure.Persistence.OrderDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderService.Infrastructure.Persistence.OrderDbContext>();
    dbContext.Database.EnsureCreated();
}

// Map endpoints
app.MapOrderEndpoints();

app.Run();


