namespace Shop.IntegrationTests.Infrastructure.TestTokens;

public static class TestTokenContext
{
    private static readonly AsyncLocal<string?> _token = new();

    public static string? CurrentToken => _token.Value;

    internal static void Set(string? token) => _token.Value = token;
}

