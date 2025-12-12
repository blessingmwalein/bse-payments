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

// Seed data and ensure database created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.Initialize(context);
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
