using NetArchTest.Rules;

using Xunit;

using Sentra.Application;
using Sentra.Infrastructure;

namespace Sentra.ArchitectureTests.Naming;

/// <summary>
/// Verifies naming conventions for common service types.
/// </summary>
public sealed class ClassSuffixTests
{
    [Fact]
    public void Application_Handlers_Should_EndWithHandler()
    {
        TestResult result = Types.InAssembly(typeof(ApplicationAssemblyMarker).Assembly)
            .That()
            .ResideInNamespaceMatching(".*Handlers.*")
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_Repositories_Should_EndWithRepository()
    {
        TestResult result = Types.InAssembly(typeof(InfrastructureAssemblyMarker).Assembly)
            .That()
            .ResideInNamespaceMatching(".*Repositories.*")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }
}