using MusicClassificationApi.Models;

namespace Tests.TestData;

/// <summary>
/// Test data factory for creating sample data for tests
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a valid music analysis request
    /// </summary>
    public static MusicAnalysisRequest CreateValidMusicAnalysisRequest()
    {
        // Create a minimal valid WAV header as base64
        var wavHeader = new byte[]
        {
            0x52, 0x49, 0x46, 0x46, // "RIFF"
            0x24, 0x00, 0x00, 0x00, // File size - 8
            0x57, 0x41, 0x56, 0x45, // "WAVE"
            0x66, 0x6D, 0x74, 0x20, // "fmt "
            0x10, 0x00, 0x00, 0x00, // Subchunk1Size
            0x01, 0x00,             // AudioFormat (PCM)
            0x01, 0x00,             // NumChannels (1)
            0x44, 0xAC, 0x00, 0x00, // SampleRate (44100)
            0x88, 0x58, 0x01, 0x00, // ByteRate
            0x02, 0x00,             // BlockAlign
            0x10, 0x00,             // BitsPerSample (16)
            0x64, 0x61, 0x74, 0x61, // "data"
            0x00, 0x00, 0x00, 0x00  // Subchunk2Size (0 for empty)
        };

        return new MusicAnalysisRequest
        {
            AudioData = Convert.ToBase64String(wavHeader),
            FileName = "test_audio.wav",
            Format = "wav"
        };
    }

    /// <summary>
    /// Creates a request with preprocessed file paths
    /// </summary>
    public static MusicAnalysisRequest CreatePreprocessedRequest()
    {
        return new MusicAnalysisRequest
        {
            FeaturesPath = "Tests/TestData/test_features.json",
            SpectrogramPath = "Tests/TestData/test_spectrogram.npy",
            FileName = "test_song.wav"
        };
    }

    /// <summary>
    /// Creates a sample music analysis response
    /// </summary>
    public static MusicAnalysisResponse CreateSampleResponse()
    {
        return new MusicAnalysisResponse
        {
            FileName = "test_audio.wav",
            ProcessingTimeMs = 1500,
            Predictions = new MusicPredictions
            {
                Genre = new ClassificationResult
                {
                    Label = "rock",
                    Confidence = 0.85
                },
                Mood = new ClassificationResult
                {
                    Label = "energetic",
                    Confidence = 0.78
                },
                Bpm = new BpmPrediction
                {
                    Value = 120.5,
                    Category = "moderate",
                    Confidence = 0.82
                },
                Key = new ClassificationResult
                {
                    Label = "C",
                    Confidence = 0.71
                }
            },
            Metadata = new AudioMetadata
            {
                Duration = 180.5,
                SampleRate = 44100,
                FileSizeBytes = 1024
            },
            Warnings = new List<string>()
        };
    }

    /// <summary>
    /// Creates Python model configuration for testing
    /// </summary>
    public static PythonModelConfiguration CreateTestPythonModelConfig()
    {
        return new PythonModelConfiguration
        {
            PythonExecutablePath = "python",
            ModelScriptPath = "examples/mock_inference.py",
            ModelFilePath = "examples/mock_model.pth",
            TimeoutSeconds = 30,
            WorkingDirectory = "."
        };
    }

    /// <summary>
    /// Creates preprocessing configuration for testing
    /// </summary>
    public static PreprocessingConfiguration CreateTestPreprocessingConfig()
    {
        return new PreprocessingConfiguration
        {
            PreprocessingScriptPath = "examples/mock_preprocessing.py",
            TempDirectory = "temp",
            MaxFileSizeMB = 50,
            SupportedFormats = new List<string> { "mp3", "wav", "flac", "m4a" }
        };
    }

    /// <summary>
    /// Creates test audio file data
    /// </summary>
    public static byte[] CreateTestAudioData()
    {
        // Create a minimal valid WAV file
        var wavHeader = new byte[]
        {
            0x52, 0x49, 0x46, 0x46, // "RIFF"
            0x2C, 0x00, 0x00, 0x00, // File size - 8 (44 bytes total)
            0x57, 0x41, 0x56, 0x45, // "WAVE"
            0x66, 0x6D, 0x74, 0x20, // "fmt "
            0x10, 0x00, 0x00, 0x00, // Subchunk1Size (16)
            0x01, 0x00,             // AudioFormat (PCM)
            0x01, 0x00,             // NumChannels (1)
            0x44, 0xAC, 0x00, 0x00, // SampleRate (44100)
            0x88, 0x58, 0x01, 0x00, // ByteRate
            0x02, 0x00,             // BlockAlign
            0x10, 0x00,             // BitsPerSample (16)
            0x64, 0x61, 0x74, 0x61, // "data"
            0x08, 0x00, 0x00, 0x00, // Subchunk2Size (8 bytes of data)
            0x00, 0x00, 0x00, 0x00, // Sample data
            0x00, 0x00, 0x00, 0x00  // Sample data
        };

        return wavHeader;
    }

    /// <summary>
    /// Creates JSON response from Python model
    /// </summary>
    public static string CreateMockPythonResponse()
    {
        return @"{
            ""predictions"": {
                ""genre"": {
                    ""label"": ""rock"",
                    ""confidence"": 0.85
                },
                ""mood"": {
                    ""label"": ""energetic"",
                    ""confidence"": 0.78
                },
                ""bpm"": {
                    ""value"": 120.5,
                    ""confidence"": 0.82
                },
                ""key"": {
                    ""label"": ""C"",
                    ""confidence"": 0.71
                }
            },
            ""metadata"": {
                ""model_version"": ""1.0.0"",
                ""processing_time_seconds"": 1.5,
                ""audio_duration_seconds"": 180.5
            }
        }";
    }
}
