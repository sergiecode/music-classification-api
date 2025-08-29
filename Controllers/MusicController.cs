using Microsoft.AspNetCore.Mvc;
using MusicClassificationApi.Models;
using MusicClassificationApi.Services;

namespace MusicClassificationApi.Controllers;

/// <summary>
/// Controller for music classification endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MusicController : ControllerBase
{
    private readonly IMusicClassificationService _musicService;
    private readonly ILogger<MusicController> _logger;

    public MusicController(
        IMusicClassificationService musicService,
        ILogger<MusicController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze music file and return predictions for genre, mood, BPM, and key
    /// </summary>
    /// <param name="request">Music analysis request containing audio data or file paths</param>
    /// <returns>Music analysis response with predictions</returns>
    /// <response code="200">Returns the music analysis results</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an error processing the audio</response>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(MusicAnalysisResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<MusicAnalysisResponse>> AnalyzeMusic([FromBody] MusicAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received music analysis request for file: {FileName}", request.FileName);

            // Validate request
            if (string.IsNullOrEmpty(request.AudioData) && 
                string.IsNullOrEmpty(request.FeaturesPath) && 
                string.IsNullOrEmpty(request.SpectrogramPath))
            {
                return BadRequest("Either audio data or preprocessed file paths must be provided");
            }

            var response = await _musicService.AnalyzeMusicAsync(request);
            
            _logger.LogInformation("Music analysis completed for file: {FileName} in {ProcessingTime}ms", 
                request.FileName, response.ProcessingTimeMs);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid request: {Error}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing music analysis request");
            return StatusCode(500, "An error occurred while processing the audio file");
        }
    }

    /// <summary>
    /// Upload and analyze a music file
    /// </summary>
    /// <param name="file">Audio file to analyze</param>
    /// <returns>Music analysis response with predictions</returns>
    /// <response code="200">Returns the music analysis results</response>
    /// <response code="400">If the file is invalid</response>
    /// <response code="500">If there was an error processing the audio</response>
    [HttpPost("analyze/upload")]
    [ProducesResponseType(typeof(MusicAnalysisResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<MusicAnalysisResponse>> AnalyzeMusicUpload(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            _logger.LogInformation("Received file upload for analysis: {FileName} ({FileSize} bytes)", 
                file.FileName, file.Length);

            // Convert file to base64 for processing
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var audioData = Convert.ToBase64String(memoryStream.ToArray());

            var request = new MusicAnalysisRequest
            {
                AudioData = audioData,
                FileName = file.FileName,
                Format = Path.GetExtension(file.FileName)?.TrimStart('.').ToLowerInvariant()
            };

            var response = await _musicService.AnalyzeMusicAsync(request);
            
            // Add file metadata
            response.Metadata.FileSizeBytes = file.Length;
            
            _logger.LogInformation("File analysis completed for: {FileName} in {ProcessingTime}ms", 
                file.FileName, response.ProcessingTimeMs);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded file");
            return StatusCode(500, "An error occurred while processing the uploaded file");
        }
    }

    /// <summary>
    /// Analyze music using preprocessed data files
    /// </summary>
    /// <param name="featuresPath">Path to the features JSON file</param>
    /// <param name="spectrogramPath">Path to the spectrogram NPY file</param>
    /// <param name="fileName">Original audio file name</param>
    /// <returns>Music analysis response with predictions</returns>
    /// <response code="200">Returns the music analysis results</response>
    /// <response code="400">If the file paths are invalid</response>
    /// <response code="500">If there was an error processing the data</response>
    [HttpPost("analyze/preprocessed")]
    [ProducesResponseType(typeof(MusicAnalysisResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<MusicAnalysisResponse>> AnalyzePreprocessedData(
        [FromQuery] string featuresPath,
        [FromQuery] string spectrogramPath,
        [FromQuery] string? fileName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(featuresPath) || string.IsNullOrEmpty(spectrogramPath))
            {
                return BadRequest("Both features path and spectrogram path are required");
            }

            _logger.LogInformation("Received preprocessed data analysis request for features: {FeaturesPath}, spectrogram: {SpectrogramPath}", 
                featuresPath, spectrogramPath);

            var request = new MusicAnalysisRequest
            {
                FeaturesPath = featuresPath,
                SpectrogramPath = spectrogramPath,
                FileName = fileName ?? Path.GetFileNameWithoutExtension(featuresPath)
            };

            var response = await _musicService.AnalyzeMusicAsync(request);
            
            _logger.LogInformation("Preprocessed data analysis completed for: {FileName} in {ProcessingTime}ms", 
                request.FileName, response.ProcessingTimeMs);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing preprocessed data");
            return StatusCode(500, "An error occurred while processing the preprocessed data");
        }
    }
}
