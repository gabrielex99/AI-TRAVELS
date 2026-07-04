using Microsoft.AspNetCore.Mvc;

namespace AiTravels.Api.Controllers;

/// <summary>
/// Health check endpoint for monitoring and orchestration.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the health status of the backend API service.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "ai-travels-backend-api",
            timestamp = DateTime.UtcNow.ToString("o"),
            version = "1.0.0"
        });
    }
}
