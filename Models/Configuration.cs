namespace MusicClassificationApi.Models;

/// <summary>
/// Configuration for Python ML model integration
/// </summary>
public class PythonModelConfiguration
{
    /// <summary>
    /// Path to Python executable
    /// </summary>
    public string PythonExecutablePath { get; set; } = "python";
    
    /// <summary>
    /// Path to the music classification model script
    /// </summary>
    public string ModelScriptPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to the trained model file
    /// </summary>
    public string ModelFilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Timeout for model inference in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Working directory for Python processes
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}

/// <summary>
/// Configuration for audio preprocessing
/// </summary>
public class PreprocessingConfiguration
{
    /// <summary>
    /// Path to preprocessing script
    /// </summary>
    public string PreprocessingScriptPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Temporary directory for audio processing
    /// </summary>
    public string TempDirectory { get; set; } = "temp";
    
    /// <summary>
    /// Maximum file size in MB
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 50;
    
    /// <summary>
    /// Supported audio formats
    /// </summary>
    public List<string> SupportedFormats { get; set; } = new() { "mp3", "wav", "flac", "m4a" };
}
