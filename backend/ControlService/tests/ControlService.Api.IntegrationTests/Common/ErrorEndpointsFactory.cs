using ControlService.API.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Common;

/// <summary>
/// The real API plus test-only endpoints that fail on purpose, so error responses can be checked
/// through the whole HTTP pipeline before any feature endpoint exists (API-12, API-13, API-14).
/// </summary>
public sealed class ErrorEndpointsFactory : WebApplicationFactory<Program>
{
    public const string ValidationFailure = "/test-only/validation-failure";
    public const string UnmappedErrorCode = "/test-only/unmapped-error-code";
    public const string UnhandledException = "/test-only/unhandled-exception";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, TestOnlyEndpoints>());

    // A startup filter receives a plain ApplicationBuilder, not an IEndpointRouteBuilder, so the
    // endpoints need their own UseRouting/UseEndpoints. They run inside the app's pipeline, after
    // UseExceptionHandler.
    private sealed class TestOnlyEndpoints : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(ValidationFailure, () => new Error(
                        "validation_failed",
                        "Alguns campos precisam ser corrigidos.",
                        Fields: new Dictionary<string, string[]>
                        {
                            ["login"] = ["Já existe um usuário com o login “ana.souza”. Escolha outro."],
                            ["address.cep"] = ["O CEP precisa ter 8 números."],
                        })
                    .ToProblem());
                endpoints.MapGet(UnmappedErrorCode, () => new Error("not_in_table", "Erro sem mapeamento.").ToProblem());
                endpoints.MapGet(UnhandledException, string () => throw new InvalidOperationException("Boom."));
            });
        };
    }
}
