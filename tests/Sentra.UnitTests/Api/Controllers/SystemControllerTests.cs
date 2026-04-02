using Microsoft.AspNetCore.Mvc;
using Xunit;

using Sentra.Api.Controllers;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Contains tests for <see cref="SystemController"/>.
/// </summary>
public sealed class SystemControllerTests
{
    [Fact]
    public void GetRoot_ShouldReturnOk_WithExpectedMessage()
    {
        SystemController controller = new();

        IActionResult actionResult = controller.GetRoot();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(actionResult);
        string message = Assert.IsType<string>(ok.Value);

        Assert.Equal("Sentra API is running.", message);
    }
}