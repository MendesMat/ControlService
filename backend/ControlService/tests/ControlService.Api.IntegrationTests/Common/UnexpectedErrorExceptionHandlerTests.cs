using System.Text.Json;
using ControlService.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Common;

// Unit test of the exception -> Problem Details fallback (API-12: 500 with a generic Portuguese
// message, no exception detail in the response). Registration of this handler with UseExceptionHandler()
// is DI wiring, covered by ApiStartupTests.
public class UnexpectedErrorExceptionHandlerTests
{
    [Fact]
    public async Task Unhandled_exception_becomes_500_with_the_generic_message() // API-12
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() },
        };
        var sut = new UnexpectedErrorExceptionHandler();

        var handled = await sut.TryHandleAsync(httpContext, new InvalidOperationException("Boom."), TestContext.Current.CancellationToken);

        handled.ShouldBeTrue();

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(
            httpContext.Response.Body, cancellationToken: TestContext.Current.CancellationToken);

        body.GetProperty("status").GetInt32().ShouldBe(500);
        body.GetProperty("code").GetString().ShouldBe("unexpected_error");
        body.GetProperty("message").GetString()
            .ShouldBe("Não foi possível concluir a operação. Tente de novo em alguns minutos.");
        body.GetRawText().ShouldNotContain("Boom.");
    }
}
