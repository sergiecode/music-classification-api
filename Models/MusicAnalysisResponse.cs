namespace MusicClassificationApi.Models;

/// <summary>
/// Response model for music analysis predictions
/// </summary>
public class MusicAnalysisResponse
{
    /// <summary>
    /// Original filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// All predictions from the model
    /// </summary>
    public MusicPredictions Predictions { get; set; } = new();
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public double ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Audio metadata
    /// </summary>
    public AudioMetadata Metadata { get; set; } = new();
    
    /// <summary>
    /// Any errors or warnings during processing
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// All predictions from the music classification model
/// </summary>
public class MusicPredictions
{
    /// <summary>
    /// Genre prediction
    /// </summary>
    public ClassificationResult Genre { get; set; } = new();
    
    /// <summary>
    /// Mood prediction
    /// </summary>
    public ClassificationResult Mood { get; set; } = new();
    
    /// <summary>
    /// BPM (tempo) prediction
    /// </summary>
    public BpmPrediction Bpm { get; set; } = new();
    
    /// <summary>
    /// Musical key prediction
    /// </summary>
    public ClassificationResult Key { get; set; } = new();
}

/// <summary>
/// Classification result with confidence score
/// </summary>
public class ClassificationResult
{
    /// <summary>
    /// Predicted label
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence score (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Optional: All class probabilities
    /// </summary>
    public Dictionary<string, double>? AllProbabilities { get; set; }
}

/// <summary>
/// BPM prediction with value and categorization
/// </summary>
public class BpmPrediction
{
    /// <summary>
    /// Predicted BPM value
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// BPM category (very_slow, slow, moderate, fast, very_fast)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence in the prediction
    /// </summary>
    public double Confidence { get; set; }
}

/// <summary>
/// Audio file metadata
/// </summary>
public class AudioMetadata
{
    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double Duration { get; set; }
    
    /// <summary>
    /// Sample rate
    /// </summary>
    public int SampleRate { get; set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
}
