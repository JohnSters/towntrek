using Microsoft.AspNetCore.Mvc;
using TownTrek.Models.ViewModels;
using System.Diagnostics;

namespace TownTrek.Controllers;

/// <summary>
/// Handles error pages and error responses
/// </summary>
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Default error page
    /// </summary>
    [Route("/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var errorViewModel = HttpContext.Items["ErrorViewModel"] as ErrorViewModel;
        
        if (errorViewModel == null)
        {
            errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = 500,
                Title = "Error",
                Message = "An error occurred while processing your request."
            };
        }

        return View("Error", errorViewModel);
    }

    /// <summary>
    /// Status code specific error pages
    /// </summary>
    [Route("/Error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HandleStatusCode(int statusCode)
    {
        var errorViewModel = HttpContext.Items["ErrorViewModel"] as ErrorViewModel;
        
        if (errorViewModel == null)
        {
            errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };

            // Set appropriate title and message based on status code
            switch (statusCode)
            {
                case 400:
                    errorViewModel.Title = "Bad Request";
                    errorViewModel.Message = "The request could not be understood by the server.";
                    break;
                case 401:
                    errorViewModel.Title = "Unauthorized";
                    errorViewModel.Message = "You are not authorized to access this resource.";
                    break;
                case 403:
                    errorViewModel.Title = "Forbidden";
                    errorViewModel.Message = "You don't have permission to access this resource.";
                    break;
                case 404:
                    errorViewModel.Title = "Page Not Found";
                    errorViewModel.Message = "The page you are looking for could not be found.";
                    break;
                case 500:
                    errorViewModel.Title = "Internal Server Error";
                    errorViewModel.Message = "An unexpected error occurred. Please try again later.";
                    break;
                case 502:
                    errorViewModel.Title = "Bad Gateway";
                    errorViewModel.Message = "The server received an invalid response from an upstream server.";
                    break;
                case 503:
                    errorViewModel.Title = "Service Unavailable";
                    errorViewModel.Message = "The service is temporarily unavailable. Please try again later.";
                    break;
                default:
                    errorViewModel.Title = $"Error {statusCode}";
                    errorViewModel.Message = "An error occurred while processing your request.";
                    break;
            }
        }

        _logger.LogWarning("Status code error page requested: {StatusCode}, RequestId: {RequestId}, Path: {Path}",
            statusCode, errorViewModel.RequestId, HttpContext.Request.Path);

        // Return appropriate view based on status code
        var viewName = statusCode switch
        {
            404 => "NotFound",
            401 => "Unauthorized",
            403 => "Forbidden",
            _ => "Error"
        };

        return View(viewName, errorViewModel);
    }
}