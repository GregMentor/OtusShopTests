using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.Api.Data;
using Shop.Api.Services;
using Shop.Domain;
using Shop.IntegrationTests.Services;

namespace Shop.IntegrationTests.Infrastructure;

/// <summary>
/// Фабрика, поднимающая Shop.Api в памяти для интеграционных тестов.
/// Подменяет реальный SQL Server на in-memory базу с уникальным именем,
/// чтобы тесты не мешали друг другу.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();
    private readonly TestCustomerService _customerService = new();
    private readonly StubNotificationService _notifications = new();

    public TestCustomerService CustomerService => _customerService;
    public StubNotificationService Notifications => _notifications;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Убираем зарегистрированный в Program.cs DbContext (на SQL Server)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // 2. Регистрируем InMemory с уникальным именем — изоляция тестов
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));

            // 2.1 Подменяем внешние зависимости на тестовые реализации
            var customerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICustomerService));
            if (customerDescriptor != null)
                services.Remove(customerDescriptor);
            services.AddSingleton<ICustomerService>(_customerService);

            var notificationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(INotificationService));
            if (notificationDescriptor != null)
                services.Remove(notificationDescriptor);
            services.AddSingleton<INotificationService>(_notifications);

            // 3. Создаём схему БД
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var lf = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _customerService.SetLogger(lf.CreateLogger<TestCustomerService>());
            _notifications.SetLogger(lf.CreateLogger<StubNotificationService>());
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}

