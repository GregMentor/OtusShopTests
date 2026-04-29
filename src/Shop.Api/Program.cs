using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Shop.Api.Data;
using Shop.Api.Observability;
using Shop.Api.Services;
using Shop.Domain;

SelfLog.Enable(msg => Console.Error.WriteLine(msg));

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, cfg) =>
{
    var elasticUrl = ctx.Configuration["ELASTICSEARCH_URL"];
    Console.Error.WriteLine($"[serilog-bootstrap] ELASTICSEARCH_URL={elasticUrl ?? "<null>"}");

    cfg.MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service", "shop-api")
        .WriteTo.Console();

    if (!string.IsNullOrWhiteSpace(elasticUrl))
    {
        cfg.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUrl))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "shop-api-logs-{0:yyyy.MM.dd}",
            EmitEventFailure = Serilog.Sinks.Elasticsearch.EmitEventFailureHandling.WriteToSelfLog
        });
    }
});

builder.Services.AddControllers();

// В реальном приложении — SQL Server.
// В интеграционных тестах подменим на InMemory через WebApplicationFactory.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
        ?? "Server=(localdb)\\mssqllocaldb;Database=Shop;Trusted_Connection=True;"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Прикладные сервисы (считаем скидку, обращаемся к внешним зависимостям).
// В интеграционных тестах ICustomerService будет подменяться на тестовую реализацию.
builder.Services.AddScoped<ICustomerService, LoyaltyTierCustomerService>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Важно для интеграционных тестов: хост часто создаётся/уничтожается быстро.
// Регистрация гарантирует, что батчи Serilog будут сброшены при остановке приложения.
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.UseMiddleware<TestTokenMiddleware>();

app.MapControllers();
app.Run();

// ⚠️ КРИТИЧЕСКИ ВАЖНАЯ СТРОЧКА ДЛЯ WebApplicationFactory!
// С .NET 6+ Program — internal sealed.
// Без этого partial-объявления тесты не увидят класс Program
// и WebApplicationFactory<Program> не скомпилируется.
public partial class Program { }
