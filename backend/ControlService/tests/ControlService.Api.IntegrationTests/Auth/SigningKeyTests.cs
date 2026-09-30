using ControlService.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class SigningKeyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==")] // base64 of 16 bytes: too short for HS256
    [InlineData("not base64 at all!")]
    public void Api_does_not_start_without_a_valid_signing_key(string invalidKey) // ADR-0019, T5
    {
        using var invalid = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:SigningKey"] = invalidKey,
            })));

        var exception = Should.Throw<Exception>(() => invalid.Services);

        exception.Message.ShouldContain("Auth:SigningKey");
        if (invalidKey.Length > 0)
        {
            exception.Message.ShouldNotContain(invalidKey);
        }
    }
}
