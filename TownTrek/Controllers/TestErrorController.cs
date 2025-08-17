using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers;

/// <summary>
/// Test controller for verifying error handling (remove in production)
/// </summary>
[Route("test-error")]
public class TestErrorController : Controller
{
    private readonly ILogger<TestErrorController> _logger;

    public TestErrorController(ILogger<TestErrorController> logger)
    {
        _logger = logger;
    }

    [HttpGet("exception")]
    public IActionResult TestException()
    {
        _logger.LogInformation("Testing exception handling");
        throw new InvalidOperationException("This is a test exception to verify error handling");
    }

    [HttpGet("not-found")]
    public IActionResult TestNotFound()
    {
        _logger.LogInformation("Testing not found error");
        throw new KeyNotFoundException("This is a test not found exception");
    }

    [HttpGet("unauthorized")]
    public IActionResult TestUnauthorized()
    {
        _logger.LogInformation("Testing unauthorized error");
        throw new UnauthorizedAccessException("This is a test unauthorized exception");
    }

    [HttpGet("argument")]
    public IActionResult TestArgument()
    {
        _logger.LogInformation("Testing argument error");
        throw new ArgumentException("This is a test argument exception");
    }

    [HttpGet("api/exception")]
    public IActionResult TestApiException()
    {
        _logger.LogInformation("Testing API exception handling");
        throw new InvalidOperationException("This is a test API exception");
    }
}