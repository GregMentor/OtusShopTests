# 🛒 Shop — демо-репозиторий вебинара по тестированию

Полностью рабочий код для двух практик вебинара.

## 📁 Структура

```
Shop.sln
├── src/
│   ├── Shop.Domain/                  ← бизнес-логика (практика 1)
│   │   ├── ICustomerService.cs       ← внешняя зависимость (мокается)
│   │   ├── DiscountService.cs        ← тестируемый класс
│   │   └── ShippingCostCalculator.cs ← мини-практика по TDD
│   └── Shop.Api/                     ← Web API (практика 2)
│       ├── Program.cs                ← ⚠️ внимание на partial class Program!
│       ├── Controllers/OrdersController.cs
│       ├── Services/LoyaltyTierCustomerService.cs  ← реализация ICustomerService
│       ├── Models/Order.cs
│       └── Data/AppDbContext.cs
└── tests/
    ├── Shop.Tests/                   ← юнит-тесты (xUnit + Moq + FluentAssertions)
    │   ├── DiscountServiceTests.cs
    │   ├── LoyaltyTierCustomerServiceTests.cs
    │   ├── TextValidationTests.cs    ← Theory + InlineData (null/""/" ")
    │   └── TddPractice/ShippingCostCalculatorTests.cs
    └── Shop.IntegrationTests/        ← интеграционные тесты (WebApplicationFactory + InMemory EF)
        ├── Infrastructure/ApiFactory.cs   ← подменяет SQL Server на in-memory
        ├── Infrastructure/TestFixture.cs  ← shared-контекст
        ├── HttpClients/OrdersApiClient.cs
        ├── Services/TestCustomerService.cs
        ├── Services/StubNotificationService.cs
        └── OrdersApiTests.cs
```

## Связка `ICustomerService` — от контракта до ответа API

Цепочка для демонстрации **двух** методов внешней зависимости: **`IsVipCustomer`** (лояльность) и **`IsAccountSuspended`** (блокировка аккаунта для заказов).

```
Shop.Domain.ICustomerService
        │
        ├─ IsVipCustomer(customerId)
        └─ IsAccountSuspended(customerId)
                │
                ▼
       Shop.Domain.DiscountService
       • при orderAmount < 0 → ArgumentException (сумма)
       • при IsAccountSuspended == true → InvalidOperationException
       • иначе расчёт % скидки + удвоение для VIP с cap 30%
                │
                ▼
       Shop.Api.Controllers.OrdersController.Create
       • после проверки Amount > 0 вызывает CalculateDiscount
       • InvalidOperationException (заблокированный аккаунт)
         → HTTP 403 Forbidden и тело из текста исключения
                │
                ▼
       Тесты
       • Shop.Tests: юниты DiscountService и LoyaltyTierCustomerService (Moq / реальный сервис)
       • Shop.IntegrationTests: TestCustomerService задаёт множества
         VipCustomerIds и SuspendedCustomerIds под сценарий
```

**Реализации `ICustomerService`:**

| Где | Поведение |
|-----|-----------|
| **`LoyaltyTierCustomerService`** (прод в API) | VIP: положительный id, кратный 100. Блокировка: id ≤ 0 или положительный id кратен **523**. |
| **`TestCustomerService`** (интеграционные тесты) | VIP и «заблокированные» id задаются явно через `HashSet` в тесте. |

**Ключевые тесты по цепочке:**

1. `LoyaltyTierCustomerServiceTests` — правила `IsVipCustomer` / `IsAccountSuspended` без Moq.
2. `DiscountServiceTests.CalculateDiscount_SuspendedCustomer_*` — мок `IsAccountSuspended` → `InvalidOperationException`, VIP не вызывается.
3. `DiscountServiceTests.CalculateDiscount_Always_AsksSuspendAndVipStatus` — проверка, что при расчёте вызываются оба метода контракта.
4. `OrdersApiTests.CreateOrder_SuspendedCustomer_Returns403` — полный путь HTTP: `SuspendedCustomerIds` в стабе → **403**.

## 🚀 Запуск

```bash
# Из корня репозитория
dotnet restore
dotnet build

# Все тесты
dotnet test

# Только юнит-тесты
dotnet test Shop.Tests

# Только интеграционные
dotnet test Shop.IntegrationTests

# С подробным выводом
dotnet test --logger "console;verbosity=detailed"
```

## 🔢 Сквозные токены тестов (GUID)

В интеграционных тестах включён **сквозной токен** для удобной отладки: перед каждым тестом генерируется **GUID**, который маркирует все HTTP-запросы этого теста и логи API по ним.

Токен автоматически прокидывается:

- **из теста → в HTTP**: заголовок `X-Test-Token`
- **в ответ API**: тот же заголовок `X-Test-Token`
- **в логи API**: поле `test_token` (удобно фильтровать в Kibana)

## 📦 Kibana/Elasticsearch для логов (Docker)

Поднять Elasticsearch + Kibana:

```powershell
docker compose -f .\docker-compose.observability.yml up -d
```

Проверки:

- Elasticsearch: `http://localhost:9200`
- Kibana: `http://localhost:5601`

Запуск интеграционных тестов так, чтобы логи API улетали в Elasticsearch (Kibana покажет их автоматически):

```powershell
# 1) Поднять Elasticsearch+Kibana
docker compose -f .\docker-compose.observability.yml up -d

# 2) Запустить тесты, прокинув URL Elasticsearch в Shop.Api (Serilog sink)
$env:ELASTICSEARCH_URL="http://localhost:9200"
dotnet test .\tests\Shop.IntegrationTests\Shop.IntegrationTests.csproj
```

Запуск API вручную (если хотите генерировать логи без тестов):

```powershell
$env:ELASTICSEARCH_URL="http://localhost:9200"
dotnet run --project .\src\Shop.Api
```

### Как смотреть логи в Kibana

1. Откройте Kibana `http://localhost:5601`
2. Перейдите в **Discover**
3. Создайте **Data view** по индексу: `shop-api-logs-*`
4. Для фильтрации по конкретному тесту используйте поле:
   - `fields.test_token : "3d6bdb8a-4b6c-4a26-8b31-2b87f8c8d0b2"`

Примечание: токен попадает в Elasticsearch как вложенное поле `fields.test_token` (это лог-контекст Serilog), поэтому фильтровать нужно именно по нему.

Остановить окружение и очистить данные:

```powershell
docker compose -f .\docker-compose.observability.yml down -v
```

## ⚙️ Версии пакетов

- .NET 8.0
- xUnit 2.6.6
- **xUnit**: фреймворк тестов (Fact/Theory, fixtures)
- **Moq**: мокинг/стабы внешних зависимостей
- **FluentAssertions**: “читаемые” проверки (Should().Be/Throw/Contain…)
- **WebApplicationFactory** (`Microsoft.AspNetCore.Mvc.Testing`): интеграционные тесты API “в памяти”
- **Bogus** (рекомендация): генерация данных для Arrange

## 📚 Документация и примеры в коде

- **WebApplicationFactory**
  - Документация (RU): [Интеграционные тесты на платформе ASP.NET Core](https://learn.microsoft.com/ru-ru/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
  - Хабр: [Интеграционные тесты для ASP.NET Core](https://habr.com/ru/articles/860932/)
  - Примеры в репозитории:
    - [`tests/Shop.IntegrationTests/Infrastructure/ApiFactory.cs`](tests/Shop.IntegrationTests/Infrastructure/ApiFactory.cs)
    - [`tests/Shop.IntegrationTests/Infrastructure/TestFixture.cs`](tests/Shop.IntegrationTests/Infrastructure/TestFixture.cs)
    - [`tests/Shop.IntegrationTests/OrdersApiTests.cs`](tests/Shop.IntegrationTests/OrdersApiTests.cs)

- **Testcontainers**
  - Документация/гайд (RU): [Testcontainers: тестирование с реальными зависимостями](https://habr.com/ru/articles/700286/)
  - Хабр: [Еще раз про интеграционное тестирование ASP.NET Core c testserver и testcontainers](https://habr.com/ru/articles/720420/)
  - Статус в этом репозитории: сейчас не используется (интеграционные тесты работают через InMemory EF + `WebApplicationFactory`).

- **Bogus**
  - Документация/гайд (RU): [Генерация тестовых данных на C# (Bogus)](https://lsreg.ru/generaciya-testovyx-dannyx-na-c/)
  - Хабр: [Faker API для .NET — генерация случайных имен и других данных](https://habr.com/ru/articles/673674/)
  - Примеры в репозитории:
    - [`tests/Shop.Tests/DiscountServiceTests.cs`](tests/Shop.Tests/DiscountServiceTests.cs)
    - [`tests/Shop.Tests/TddPractice/ShippingCostCalculatorTests.cs`](tests/Shop.Tests/TddPractice/ShippingCostCalculatorTests.cs)

- **FluentAssertions**
  - Документация/гайд (RU): [Как писать тесты на FluentAssertions C#](https://radar4site.ru/blog/3180-kak-pisat-testy-na-fluentassertions-c.html)
  - Хабр: [Fluent Assertions — инструмент автоматизированного тестирования](https://habr.com/ru/companies/usetech/articles/691160/)
  - Примеры в репозитории:
    - [`tests/Shop.Tests/TextValidationTests.cs`](tests/Shop.Tests/TextValidationTests.cs)
    - [`tests/Shop.Tests/LoyaltyTierCustomerServiceTests.cs`](tests/Shop.Tests/LoyaltyTierCustomerServiceTests.cs)
    - [`tests/Shop.IntegrationTests/OrdersApiTests.cs`](tests/Shop.IntegrationTests/OrdersApiTests.cs)

## ❗ Типичные ошибки и подводные камни

| Проблема | Решение |
|----------|---------|
| `WebApplicationFactory<Program>` не компилируется | Добавьте `public partial class Program { }` в конец `Program.cs` |
| Тесты "видят" друг друга через БД | Используйте `Guid.NewGuid()` в имени in-memory БД |
| `dotnet test` молчит без подробностей | Добавьте `--logger "console;verbosity=detailed"` |
| Moq Setup не срабатывает | Проверьте матчер: `It.IsAny<int>()` vs конкретное значение |
