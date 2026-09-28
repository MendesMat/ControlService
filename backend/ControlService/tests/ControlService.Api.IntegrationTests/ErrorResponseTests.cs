using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ControlService.Api.IntegrationTests.Common;

namespace ControlService.Api.IntegrationTests;

public sealed class ErrorResponseTests(ErrorEndpointsFactory factory) : IClassFixture<ErrorEndpointsFactory>
{
    private const string GenericMessage = "Não foi possível concluir a operação. Tente de novo em alguns minutos.";

    [Fact]
    public async Task Validation_failure_returns_the_documented_json() // API-12, API-13, docs/api/conventions.md#errors
    {
        var (status, body) = await GetAsync(ErrorEndpointsFactory.ValidationFailure);

        status.ShouldBe(HttpStatusCode.BadRequest);
        body.EnumerateObject().Select(property => property.Name)
            .ShouldBe(["type", "title", "status", "code", "message", "errors", "traceId"], ignoreOrder: true);
        body.GetProperty("type").GetString().ShouldBe("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        body.GetProperty("title").GetString().ShouldBe("Bad Request");
        body.GetProperty("status").GetInt32().ShouldBe(400);
        body.GetProperty("code").GetString().ShouldBe("validation_failed");
        body.GetProperty("message").GetString().ShouldBe("Alguns campos precisam ser corrigidos.");
        body.GetProperty("errors").GetProperty("login")[0].GetString()
            .ShouldBe("Já existe um usuário com o login “ana.souza”. Escolha outro.");
        body.GetProperty("errors").GetProperty("address.cep")[0].GetString().ShouldBe("O CEP precisa ter 8 números.");
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Unhandled_exception_returns_the_generic_500() // API-12, API-14
    {
        var (status, body) = await GetAsync(ErrorEndpointsFactory.UnhandledException);

        ShouldBeTheGeneric500(status, body);
        body.GetRawText().ShouldNotContain("Boom.");
    }

    [Fact]
    public async Task Unmapped_error_code_returns_the_generic_500() // API-12, API-14
    {
        var (status, body) = await GetAsync(ErrorEndpointsFactory.UnmappedErrorCode);

        ShouldBeTheGeneric500(status, body);
        body.GetRawText().ShouldNotContain("not_in_table");
    }

    private static void ShouldBeTheGeneric500(HttpStatusCode status, JsonElement body)
    {
        status.ShouldBe(HttpStatusCode.InternalServerError);
        body.GetProperty("code").GetString().ShouldBe("unexpected_error");
        body.GetProperty("message").GetString().ShouldBe(GenericMessage);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(string route)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(route, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        return (response.StatusCode, body);
    }
}
