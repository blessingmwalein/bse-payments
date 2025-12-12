using Microsoft.EntityFrameworkCore;
using bse_payments.Data;
using bse_payments.Data.Repositories;
using bse_payments.Services;
using bse_payments.Services.Adapters;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "BSE Payment Service API";
    config.Version = "v1";
    config.Description = "Payment gateway for Botswana Stock Exchange - Mobile Money Integration";
});

// Database - SQL Server on DESKTOP-9RRSD5S
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// BO Database for CashTrans
builder.Services.AddDbContext<BoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BoConnection")));

// Repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<CashTransService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<BtcAdapter>();

// HttpClient
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data and ensure database created (with error handling)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            logger.LogInformation("Attempting to connect to BSEPayments database...");
            // await context.Database.MigrateAsync();
            // await SeedData.Initialize(context);
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed. Check connection strings in appsettings.json");
            // Don't throw - allow app to start so we can see the error in logs
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR during startup: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

// Configure pipeline
// Enable Swagger in all environments for API documentation
app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/", () => new
{
    service = "BSE Payment Service",
    version = "1.0",
    status = "Running",
    endpoints = new[]
    {
        "/swagger - API Documentation",
        "/api/payments/deposit - Create deposit",
        "/api/payments/withdraw - Create withdrawal",
        "/api/payments/status - Check transaction status",
        "/api/clients/{cdsNumber}/transactions - Get client transactions",
        "/api/clients/{cdsNumber}/balance - Get client balance",
        "/api/transactions - Get all payment transactions"
    }
});

app.Run();
