namespace MusicClassificationApi.Models;

/// <summary>
/// Request model for music analysis
/// </summary>
public class MusicAnalysisRequest
{
    /// <summary>
    /// Base64 encoded audio file data
    /// </summary>
    public string? AudioData { get; set; }
    
    /// <summary>
    /// Audio file name
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// Audio file format (mp3, wav, flac, etc.)
    /// </summary>
    public string? Format { get; set; }
    
    /// <summary>
    /// Optional: Direct path to preprocessed features file
    /// </summary>
    public string? FeaturesPath { get; set; }
    
    /// <summary>
    /// Optional: Direct path to preprocessed spectrogram file
    /// </summary>
    public string? SpectrogramPath { get; set; }
}
