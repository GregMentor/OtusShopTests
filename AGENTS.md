# Shop demo repository — контекст для Cursor

Этот репозиторий — демо-код для вебинара по юнит- и интеграционным тестам на **.NET 8**.

## Структура решения

- `src/Shop.Domain/`: бизнес-логика (практика 1), класс `DiscountService`, контракт `ICustomerService` (мокается), и мини-практика `ShippingCostCalculator`.
- `tests/Shop.Tests/`: юнит-тесты (**xUnit** + **Moq** + **FluentAssertions**; Bogus — для Arrange).
- `src/Shop.Api/`: минимальный ASP.NET Core Web API + EF Core.
- `tests/Shop.IntegrationTests/`: интеграционные тесты (**WebApplicationFactory** + InMemory EF + typed `OrdersApiClient`).

## Роли инструментов (коротко)

- **xUnit**: тестовый фреймворк (Fact/Theory, fixtures, коллекции).
- **Moq**: библиотека моков (подмена внешних зависимостей, `Setup`/`Verify`).
- **FluentAssertions**: читаемые утверждения (`result.Should().Be(...)`, `act.Should().Throw<...>()`).
- **WebApplicationFactory / TestServer**: интеграционные тесты API в памяти (без сети и Kestrel).
- **Bogus**: генерация тестовых данных для Arrange (когда важна “правдоподобность”, а не конкретное значение).

## Быстрый старт (PowerShell)

Из корня репозитория:

```powershell
dotnet restore
dotnet build
dotnet test
```

Только отдельные тестовые проекты:

```powershell
dotnet test .\tests\Shop.Tests
dotnet test .\tests\Shop.IntegrationTests
```

Подробный вывод:

```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Как запускать API локально

```powershell
dotnet run --project .\src\Shop.Api
```

По умолчанию `Shop.Api` использует SQL Server через connection string `Default` (если не задан — fallback на LocalDB).

## Нюансы интеграционных тестов

- `Shop.IntegrationTests` подменяет реальную БД на **InMemory** через `WebApplicationFactory`.
- В `src/Shop.Api/Program.cs` обязательно должно быть `public partial class Program { }`, иначе `WebApplicationFactory<Program>` не соберётся (в .NET 6+ `Program` — internal).

## Что важно сохранять при правках

- Не ломать контракт контроллеров/DTO без обновления интеграционных тестов.
- Изменения, касающиеся БД/`AppDbContext`, должны учитываться в `ApiFactory` (подмена на InMemory).
- Предпочитать небольшие, проверяемые изменения с прогоном `dotnet test`.

