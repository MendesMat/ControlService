using System.Reflection;
using NetArchTest.Rules;

namespace ControlService.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "ControlService.Application";
    private const string InfrastructureNamespace = "ControlService.Infrastructure";
    private const string ApiNamespace = "ControlService.API";
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";

    private static readonly Assembly DomainAssembly = typeof(Domain.AssemblyReference).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.AssemblyReference).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_other_layers_or_frameworks() =>
        AssertNoDependency(DomainAssembly,
            ApplicationNamespace, InfrastructureNamespace, ApiNamespace, EntityFrameworkCore, AspNetCore);

    [Fact]
    public void Application_does_not_depend_on_infrastructure_api_or_frameworks() =>
        AssertNoDependency(ApplicationAssembly,
            InfrastructureNamespace, ApiNamespace, EntityFrameworkCore, AspNetCore);

    [Fact]
    public void Infrastructure_does_not_depend_on_the_api() =>
        AssertNoDependency(InfrastructureAssembly, ApiNamespace);

    private static void AssertNoDependency(Assembly assembly, params string[] forbiddenNamespaces)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        var offenders = result.FailingTypeNames ?? [];
        result.IsSuccessful.ShouldBeTrue($"Forbidden dependencies found in: {string.Join(", ", offenders)}");
    }
}
