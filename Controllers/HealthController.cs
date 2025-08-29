using Microsoft.AspNetCore.Mvc;
using MusicClassificationApi.Services;

namespace MusicClassificationApi.Controllers;

/// <summary>
/// Controller for health check and system status endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IMusicClassificationService _musicService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IMusicClassificationService musicService,
        ILogger<HealthController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }

    /// <summary>
    /// Check API health status
    /// </summary>
    /// <returns>Health status information</returns>
    /// <response code="200">Service is healthy</response>
    /// <response code="503">Service is unhealthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 503)]
    public async Task<ActionResult> GetHealthStatus()
    {
        try
        {
            var isHealthy = await _musicService.IsHealthyAsync();
            
            var healthStatus = new
            {
                status = isHealthy ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow,
                services = new
                {
                    api = "healthy",
                    python_model = isHealthy ? "healthy" : "unhealthy",
                    preprocessing = "ready"
                },
                version = "1.0.0"
            };

            if (isHealthy)
            {
                _logger.LogDebug("Health check passed");
                return Ok(healthStatus);
            }
            else
            {
                _logger.LogWarning("Health check failed - Python model service unavailable");
                return StatusCode(503, healthStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            
            var errorStatus = new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message,
                services = new
                {
                    api = "degraded",
                    python_model = "unknown",
                    preprocessing = "unknown"
                },
                version = "1.0.0"
            };

            return StatusCode(503, errorStatus);
        }
    }

    /// <summary>
    /// Get API information and supported features
    /// </summary>
    /// <returns>API information</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult GetApiInfo()
    {
        var apiInfo = new
        {
            name = "Music Classification API",
            version = "1.0.0",
            description = "ASP.NET Core API for music classification using AI models",
            author = "Sergie Code",
            endpoints = new
            {
                analyze = "/api/music/analyze",
                upload = "/api/music/analyze/upload",
                preprocessed = "/api/music/analyze/preprocessed",
                health = "/api/health",
                info = "/api/health/info"
            },
            supported_formats = new[] { "mp3", "wav", "flac", "m4a" },
            features = new
            {
                genre_classification = true,
                mood_detection = true,
                bpm_estimation = true,
                key_detection = true,
                batch_processing = false,
                real_time_processing = true
            },
            integration = new
            {
                preprocessing_repo = "music-classification-preprocessing",
                model_repo = "music-classification-model",
                python_backend = true
            }
        };

        return Ok(apiInfo);
    }
}
