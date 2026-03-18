using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Sentra.Api.Controllers;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Contains unit tests for <see cref="SystemController"/>.
/// </summary>
public sealed class SystemControllerTests
{
    /// <summary>
    /// Verifies that <see cref="SystemController.GetRoot"/> returns a success response.
    /// </summary>
    [Fact]
    public void GetRoot_Should_Return_Ok_With_Status_Message()
    {
        SystemController? controller = new SystemController();

        IActionResult? result = controller.GetRoot();

        OkObjectResult? okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be("Sentra API is running.");
    }
}