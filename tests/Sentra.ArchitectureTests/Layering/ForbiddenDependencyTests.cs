using NetArchTest.Rules;

using Xunit;

using Sentra.Api;
using Sentra.Application;
using Sentra.Domain;
using Sentra.Infrastructure;

namespace Sentra.ArchitectureTests.Layering;

/// <summary>
/// Verifies forbidden dependencies between project layers.
/// </summary>
public sealed class ForbiddenDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        TestResult result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(InfrastructureAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_ShouldNotDependOnApi()
    {
        TestResult result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(ApiAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOnApi()
    {
        TestResult result = Types.InAssembly(typeof(ApplicationAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(ApiAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }
}