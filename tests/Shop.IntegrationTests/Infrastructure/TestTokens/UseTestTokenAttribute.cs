using System.Reflection;
using Xunit.Sdk;

namespace Shop.IntegrationTests.Infrastructure.TestTokens;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class UseTestTokenAttribute : BeforeAfterTestAttribute
{
    public const string HeaderName = "X-Test-Token";

    public override void Before(MethodInfo methodUnderTest)
    {
        TestTokenContext.Set(Guid.NewGuid().ToString("D"));
    }

    public override void After(MethodInfo methodUnderTest)
    {
        TestTokenContext.Set(null);
    }
}

