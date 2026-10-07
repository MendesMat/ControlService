using System.Text.Json;
using ControlService.API.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Common;

// Unit tests of the Error -> Problem Details mapping itself. The full JSON shape, traceId
// and the generic 500 are checked through the real HTTP pipeline in ErrorResponseTests.
public sealed class ErrorResultsTests
{
    // Every row of the error code table in docs/api/conventions.md#error-codes. account_inactive
    // defaults to 401 (during a session); the sign-in endpoint returns its 403 explicitly.
    [Theory]
    [InlineData("validation_failed", 400, "https://tools.ietf.org/html/rfc9110#section-15.5.1", "Bad Request")]
    [InlineData("session_expired", 401, "https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized")]
    [InlineData("invalid_credentials", 401, "https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized")]
    [InlineData("account_inactive", 401, "https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized")]
    [InlineData("password_change_required", 401, "https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized")]
    [InlineData("forbidden", 403, "https://tools.ietf.org/html/rfc9110#section-15.5.4", "Forbidden")]
    [InlineData("not_found", 404, "https://tools.ietf.org/html/rfc9110#section-15.5.5", "Not Found")]
    [InlineData("concurrency_conflict", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("profile_in_use", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("system_record", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("self_deactivation", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("not_pending", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("not_inactive", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("email_missing", 409, "https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict")]
    [InlineData("link_invalid", 410, "https://tools.ietf.org/html/rfc9110#section-15.5.11", "Gone")]
    [InlineData("locked_out", 429, "https://tools.ietf.org/html/rfc6585#section-4", "Too Many Requests")]
    [InlineData("unexpected_error", 500, "https://tools.ietf.org/html/rfc9110#section-15.6.1", "Internal Server Error")]
    public async Task Every_documented_error_code_maps_to_its_status(string code, int status, string type, string title) // API-14
    {
        var body = await ExecuteAsync(new Error(code, "Mensagem de teste."));

        body.GetProperty("status").GetInt32().ShouldBe(status);
        body.GetProperty("type").GetString().ShouldBe(type);
        body.GetProperty("title").GetString().ShouldBe(title);
        body.GetProperty("code").GetString().ShouldBe(code);
    }

    [Fact]
    public async Task Details_of_an_error_are_returned_under_details()
    {
        var error = new Error(
            "concurrency_conflict",
            "Este cadastro foi alterado por Bruno Lima enquanto você editava.",
            Details: new Dictionary<string, object?> { ["updatedByName"] = "Bruno Lima" });

        var body = await ExecuteAsync(error);

        body.GetProperty("details").GetProperty("updatedByName").GetString().ShouldBe("Bruno Lima");
    }

    [Fact]
    public void An_error_becomes_a_typed_problem_result() // Typed results feed the OpenAPI document
    {
        ProblemHttpResult result = new Error("not_found", "Registro não encontrado.").ToProblem();

        result.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void Unmapped_error_code_is_an_unexpected_failure() // API-12
    {
        var error = new Error("not_in_table", "Erro sem mapeamento.");

        var exception = Should.Throw<InvalidOperationException>(() => error.ToProblem());

        exception.Message.ShouldContain("not_in_table");
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

        await error.ToProblem().ExecuteAsync(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<JsonElement>(httpContext.Response.Body);
    }
}
