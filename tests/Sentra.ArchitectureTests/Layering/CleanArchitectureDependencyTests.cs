using FluentAssertions;
using NetArchTest.Rules;
using Sentra.Application;
using Sentra.Domain;
using Sentra.Infrastructure;
using Sentra.SharedKernel;
using Xunit;

namespace Sentra.ArchitectureTests.Layering;

/// <summary>
/// Verifies clean architecture dependency boundaries.
/// </summary>
public sealed class CleanArchitectureDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        // Act
        TestResult result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(InfrastructureAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        // Act
        TestResult result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(ApplicationAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void SharedKernel_ShouldNotDependOnInfrastructure()
    {
        // Act
        TestResult result = Types.InAssembly(typeof(SharedKernelAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(InfrastructureAssemblyMarker).Assembly.GetName().Name!)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}