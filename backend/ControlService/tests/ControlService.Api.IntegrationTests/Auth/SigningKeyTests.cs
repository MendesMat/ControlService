using ControlService.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class SigningKeyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData(null)] // the user secret was never set
    [InlineData("")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==")] // base64 of 16 bytes: too short for HS256
    [InlineData("not base64 at all!")]
    public void Api_does_not_start_without_a_valid_signing_key(string? invalidKey) // ADR-0019, T5
    {
        using var invalid = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:SigningKey"] = invalidKey,
            })));

        var exception = Should.Throw<Exception>(() => invalid.Services);

        exception.Message.ShouldContain("Auth:SigningKey");
        if (!string.IsNullOrEmpty(invalidKey))
        {
            exception.Message.ShouldNotContain(invalidKey);
        }
    }

    [Fact]
    public void Api_starts_with_a_signing_key_longer_than_64_bytes() // ADR-0019, T5: "at least 32 bytes"
    {
        var longKey = Convert.ToBase64String(new byte[96].Select(_ => (byte)9).ToArray());
        using var valid = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:SigningKey"] = longKey,
            })));

        Should.NotThrow(() => valid.Services);
    }
}
