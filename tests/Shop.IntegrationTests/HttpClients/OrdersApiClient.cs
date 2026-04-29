using System.Net.Http.Json;
using Shop.Api.Models;
using Shop.Api.Models.DTO;
using Shop.IntegrationTests.Infrastructure.TestTokens;

namespace Shop.IntegrationTests.HttpClients;

/// <summary>
/// Небольшая typed-обёртка над HttpClient для интеграционных тестов.
/// Делает тесты "сценарными": тест описывает шаги, а не детали HTTP/JSON.
/// </summary>
public sealed class OrdersApiClient
{
    private readonly HttpClient _http;
    private readonly Func<string?> _tokenProvider;

    public OrdersApiClient(HttpClient http, Func<string?>? tokenProvider = null)
    {
        _http = http;
        _tokenProvider = tokenProvider ?? (() => null);
    }

    public async Task<HttpResponseMessage> CreateAsync(CreateOrderDto dto)
        => await SendAsync(() => _http.PostAsJsonAsync("/api/orders", dto));

    public async Task<HttpResponseMessage> GetByIdAsync(int id)
        => await SendAsync(() => _http.GetAsync($"/api/orders/{id}"));

    public async Task<Order?> ReadOrderAsync(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<Order>();

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        var token = _tokenProvider();
        if (!string.IsNullOrWhiteSpace(token))
        {
            if (_http.DefaultRequestHeaders.Contains(UseTestTokenAttribute.HeaderName))
                _http.DefaultRequestHeaders.Remove(UseTestTokenAttribute.HeaderName);
            _http.DefaultRequestHeaders.Add(UseTestTokenAttribute.HeaderName, token);
        }

        var response = await send();

        if (!string.IsNullOrWhiteSpace(token) &&
            _http.DefaultRequestHeaders.Contains(UseTestTokenAttribute.HeaderName))
        {
            _http.DefaultRequestHeaders.Remove(UseTestTokenAttribute.HeaderName);
        }

        return response;
    }
}

