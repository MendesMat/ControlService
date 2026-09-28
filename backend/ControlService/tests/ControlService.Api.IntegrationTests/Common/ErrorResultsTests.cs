using System.Text.Json;
using ControlService.API.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Common;

// Unit tests of the pure Error -> Problem Details mapping (ADR-0009, API-12, API-13).
// The real HTTP pipeline that calls it is covered by ApiStartupTests and RouteGroupTests.
public class ErrorResultsTests
{
    [Fact]
    public async Task Validation_failure_becomes_400_with_the_documented_json_shape() // API-12, API-13, ADR-0009
    {
        var error = new Error(
            "validation_failed",
            "Alguns campos precisam ser corrigidos.",
            Fields: new Dictionary<string, string[]>
            {
                ["login"] = ["Já existe um usuário com o login “ana.souza”. Escolha outro."]
            });

        var body = await ExecuteAsync(error);

        body.GetProperty("type").GetString().ShouldBe("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        body.GetProperty("title").GetString().ShouldBe("Bad Request");
        body.GetProperty("status").GetInt32().ShouldBe(400);
        body.GetProperty("code").GetString().ShouldBe("validation_failed");
        body.GetProperty("message").GetString().ShouldBe("Alguns campos precisam ser corrigidos.");
        body.GetProperty("errors").GetProperty("login")[0].GetString()
            .ShouldBe("Já existe um usuário com o login “ana.souza”. Escolha outro.");
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Not_found_failure_becomes_404() // ADR-0009
    {
        var error = new Error("not_found", "Registro não encontrado.");

        var body = await ExecuteAsync(error);

        body.GetProperty("status").GetInt32().ShouldBe(404);
        body.GetProperty("title").GetString().ShouldBe("Not Found");
        body.GetProperty("code").GetString().ShouldBe("not_found");
        body.GetProperty("message").GetString().ShouldBe("Registro não encontrado.");
    }

    [Fact]
    public async Task Conflict_failure_with_details_becomes_409_and_carries_the_details() // ADR-0009
    {
        var error = new Error(
            "concurrency_conflict",
            "Este cadastro foi alterado por Bruno Lima enquanto você editava.",
            Details: new Dictionary<string, object?> { ["updatedByName"] = "Bruno Lima" });

        var body = await ExecuteAsync(error);

        body.GetProperty("status").GetInt32().ShouldBe(409);
        body.GetProperty("title").GetString().ShouldBe("Conflict");
        body.GetProperty("code").GetString().ShouldBe("concurrency_conflict");
        body.GetProperty("details").GetProperty("updatedByName").GetString().ShouldBe("Bruno Lima");
    }

    [Fact]
    public async Task Unmapped_error_code_returns_500() // ADR-0009
    {
        var error = new Error("not_in_table", "Erro sem mapeamento.");

        var body = await ExecuteAsync(error);

        body.GetProperty("status").GetInt32().ShouldBe(500);
        body.GetProperty("title").GetString().ShouldBe("Internal Server Error");
        body.GetProperty("code").GetString().ShouldBe("not_in_table");
        body.GetProperty("message").GetString().ShouldBe("Erro sem mapeamento.");
    }

    [Theory]
    [InlineData("validation_failed")]
    [InlineData("not_found")]
    [InlineData("concurrency_conflict")]
    [InlineData("not_in_table")]
    public async Task Every_error_response_includes_the_traceId(string code) // ADR-0028
    {
        var body = await ExecuteAsync(new Error(code, "Mensagem de teste."));

        body.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    private static readonly IServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddProblemDetails()
        .BuildServiceProvider();

    private static async Task<JsonElement> ExecuteAsync(Error error)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = Services,
            Response = { Body = new MemoryStream() },
        };

        await error.ToProblem(httpContext).ExecuteAsync(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<JsonElement>(httpContext.Response.Body);
    }
}
