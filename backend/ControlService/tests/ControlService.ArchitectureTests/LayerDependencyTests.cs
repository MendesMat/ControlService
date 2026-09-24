using System.Reflection;
using NetArchTest.Rules;

namespace ControlService.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string _applicationNamespace = "ControlService.Application";
    private const string _infrastructureNamespace = "ControlService.Infrastructure";
    private const string _apiNamespace = "ControlService.API";
    private const string _entityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string _aspNetCore = "Microsoft.AspNetCore";

    private static readonly Assembly _domainAssembly = typeof(Domain.AssemblyReference).Assembly;
    private static readonly Assembly _applicationAssembly = typeof(Application.AssemblyReference).Assembly;
    private static readonly Assembly _infrastructureAssembly = typeof(Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_other_layers_or_frameworks() =>
        AssertNoDependency(_domainAssembly,
            _applicationNamespace, _infrastructureNamespace, _apiNamespace, _entityFrameworkCore, _aspNetCore);

    [Fact]
    public void Application_does_not_depend_on_infrastructure_api_or_frameworks() =>
        AssertNoDependency(_applicationAssembly,
            _infrastructureNamespace, _apiNamespace, _entityFrameworkCore, _aspNetCore);

    [Fact]
    public void Infrastructure_does_not_depend_on_the_api() =>
        AssertNoDependency(_infrastructureAssembly, _apiNamespace);

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
