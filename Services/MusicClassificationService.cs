using MusicClassificationApi.Models;

namespace MusicClassificationApi.Services;

/// <summary>
/// Interface for music classification service
/// </summary>
public interface IMusicClassificationService
{
    /// <summary>
    /// Analyze music from audio data
    /// </summary>
    Task<MusicAnalysisResponse> AnalyzeMusicAsync(MusicAnalysisRequest request);
    
    /// <summary>
    /// Check if the service is healthy and model is loaded
    /// </summary>
    Task<bool> IsHealthyAsync();
}

/// <summary>
/// Service for communicating with Python music classification model
/// </summary>
public class PythonMusicClassificationService : IMusicClassificationService
{
    private readonly PythonModelConfiguration _modelConfig;
    private readonly PreprocessingConfiguration _preprocessingConfig;
    private readonly ILogger<PythonMusicClassificationService> _logger;

    public PythonMusicClassificationService(
        PythonModelConfiguration modelConfig,
        PreprocessingConfiguration preprocessingConfig,
        ILogger<PythonMusicClassificationService> logger)
    {
        _modelConfig = modelConfig;
        _preprocessingConfig = preprocessingConfig;
        _logger = logger;
    }

    public async Task<MusicAnalysisResponse> AnalyzeMusicAsync(MusicAnalysisRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting music analysis for file: {FileName}", request.FileName);
            
            // Create response object
            var response = new MusicAnalysisResponse
            {
                FileName = request.FileName ?? "unknown"
            };

            // Save audio data to temporary file if provided
            string? tempAudioFile = null;
            if (!string.IsNullOrEmpty(request.AudioData))
            {
                tempAudioFile = await SaveAudioToTempFileAsync(request);
            }

            try
            {
                // Call Python preprocessing and model inference
                var predictions = await CallPythonModelAsync(tempAudioFile, request);
                response.Predictions = predictions;
                
                _logger.LogInformation("Music analysis completed successfully for: {FileName}", request.FileName);
            }
            finally
            {
                // Clean up temporary file
                if (tempAudioFile != null && File.Exists(tempAudioFile))
                {
                    File.Delete(tempAudioFile);
                }
            }

            stopwatch.Stop();
            response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during music analysis for file: {FileName}", request.FileName);
            stopwatch.Stop();
            
            return new MusicAnalysisResponse
            {
                FileName = request.FileName ?? "unknown",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Warnings = new List<string> { $"Analysis failed: {ex.Message}" }
            };
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            _logger.LogDebug("Checking service health");
            
            // Check if Python executable exists
            var pythonCheckProcess = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _modelConfig.PythonExecutablePath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(pythonCheckProcess);
            if (process == null) return false;
            
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return false;
        }
    }

    private async Task<string> SaveAudioToTempFileAsync(MusicAnalysisRequest request)
    {
        if (string.IsNullOrEmpty(request.AudioData))
            throw new ArgumentException("Audio data is required");

        // Ensure temp directory exists
        Directory.CreateDirectory(_preprocessingConfig.TempDirectory);
        
        // Generate unique temp file name
        var extension = !string.IsNullOrEmpty(request.Format) ? request.Format : "wav";
        var tempFileName = $"{Guid.NewGuid()}.{extension}";
        var tempFilePath = Path.Combine(_preprocessingConfig.TempDirectory, tempFileName);
        
        // Decode and save audio data
        var audioBytes = Convert.FromBase64String(request.AudioData);
        await File.WriteAllBytesAsync(tempFilePath, audioBytes);
        
        _logger.LogDebug("Saved audio to temporary file: {TempFile}", tempFilePath);
        return tempFilePath;
    }

    private async Task<MusicPredictions> CallPythonModelAsync(string? audioFilePath, MusicAnalysisRequest request)
    {
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = _modelConfig.PythonExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _modelConfig.WorkingDirectory
        };

        // Build arguments for Python script
        var arguments = new List<string> { _modelConfig.ModelScriptPath };
        
        if (!string.IsNullOrEmpty(audioFilePath))
        {
            arguments.Add("--audio-file");
            arguments.Add($"\"{audioFilePath}\"");
        }
        
        if (!string.IsNullOrEmpty(request.FeaturesPath))
        {
            arguments.Add("--features-file");
            arguments.Add($"\"{request.FeaturesPath}\"");
        }
        
        if (!string.IsNullOrEmpty(request.SpectrogramPath))
        {
            arguments.Add("--spectrogram-file");
            arguments.Add($"\"{request.SpectrogramPath}\"");
        }
        
        arguments.Add("--model-path");
        arguments.Add($"\"{_modelConfig.ModelFilePath}\"");
        arguments.Add("--output-format");
        arguments.Add("json");

        processInfo.Arguments = string.Join(" ", arguments);
        
        _logger.LogDebug("Calling Python model with arguments: {Arguments}", processInfo.Arguments);

        using var process = System.Diagnostics.Process.Start(processInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start Python process");

        // Wait for process to complete with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_modelConfig.TimeoutSeconds));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill();
            throw new TimeoutException($"Python model process timed out after {_modelConfig.TimeoutSeconds} seconds");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("Python process failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
            throw new InvalidOperationException($"Python model failed: {error}");
        }

        if (string.IsNullOrEmpty(output))
        {
            throw new InvalidOperationException("No output received from Python model");
        }

        // Parse JSON response from Python
        return ParsePythonModelOutput(output);
    }

    private MusicPredictions ParsePythonModelOutput(string jsonOutput)
    {
        try
        {
            // Parse the JSON response from Python model
            // Expected format based on the README examples
            var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonOutput);
            
            if (jsonObject?.predictions == null)
                throw new InvalidOperationException("Invalid response format from Python model");

            var predictions = jsonObject.predictions;
            
            return new MusicPredictions
            {
                Genre = new ClassificationResult
                {
                    Label = predictions.genre?.label?.ToString() ?? "unknown",
                    Confidence = predictions.genre?.confidence ?? 0.0
                },
                Mood = new ClassificationResult
                {
                    Label = predictions.mood?.label?.ToString() ?? "unknown",
                    Confidence = predictions.mood?.confidence ?? 0.0
                },
                Bpm = new BpmPrediction
                {
                    Value = predictions.bpm?.value ?? 0.0,
                    Category = GetBpmCategory(predictions.bpm?.value ?? 0.0),
                    Confidence = predictions.bpm?.confidence ?? 0.0
                },
                Key = new ClassificationResult
                {
                    Label = predictions.key?.label?.ToString() ?? "unknown",
                    Confidence = predictions.key?.confidence ?? 0.0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Python model output: {Output}", jsonOutput);
            throw new InvalidOperationException($"Failed to parse model output: {ex.Message}");
        }
    }

    private static string GetBpmCategory(double bpm)
    {
        return bpm switch
        {
            < 60 => "very_slow",
            < 90 => "slow",
            < 120 => "moderate",
            < 140 => "fast",
            _ => "very_fast"
        };
    }
}
