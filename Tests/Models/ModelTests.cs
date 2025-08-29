using Xunit;
using FluentAssertions;
using MusicClassificationApi.Models;
using Tests.TestData;

namespace Tests.Models;

/// <summary>
/// Unit tests for model classes
/// </summary>
public class ModelTests
{
    [Fact]
    public void MusicAnalysisRequest_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new MusicAnalysisRequest();

        // Assert
        request.AudioData.Should().BeNull();
        request.FileName.Should().BeNull();
        request.Format.Should().BeNull();
        request.FeaturesPath.Should().BeNull();
        request.SpectrogramPath.Should().BeNull();
    }

    [Fact]
    public void MusicAnalysisRequest_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expectedAudioData = "base64data";
        var expectedFileName = "test.wav";
        var expectedFormat = "wav";

        // Act
        var request = new MusicAnalysisRequest
        {
            AudioData = expectedAudioData,
            FileName = expectedFileName,
            Format = expectedFormat
        };

        // Assert
        request.AudioData.Should().Be(expectedAudioData);
        request.FileName.Should().Be(expectedFileName);
        request.Format.Should().Be(expectedFormat);
    }

    [Fact]
    public void MusicAnalysisResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new MusicAnalysisResponse();

        // Assert
        response.FileName.Should().Be(string.Empty);
        response.Predictions.Should().NotBeNull();
        response.ProcessingTimeMs.Should().Be(0);
        response.Metadata.Should().NotBeNull();
        response.Warnings.Should().NotBeNull();
        response.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void MusicPredictions_ShouldInitializeWithDefaultValues()
    {
        // Act
        var predictions = new MusicPredictions();

        // Assert
        predictions.Genre.Should().NotBeNull();
        predictions.Mood.Should().NotBeNull();
        predictions.Bpm.Should().NotBeNull();
        predictions.Key.Should().NotBeNull();
    }

    [Fact]
    public void ClassificationResult_ShouldInitializeWithDefaultValues()
    {
        // Act
        var result = new ClassificationResult();

        // Assert
        result.Label.Should().Be(string.Empty);
        result.Confidence.Should().Be(0.0);
        result.AllProbabilities.Should().BeNull();
    }

    [Fact]
    public void BpmPrediction_ShouldInitializeWithDefaultValues()
    {
        // Act
        var bpm = new BpmPrediction();

        // Assert
        bpm.Value.Should().Be(0.0);
        bpm.Category.Should().Be(string.Empty);
        bpm.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void AudioMetadata_ShouldInitializeWithDefaultValues()
    {
        // Act
        var metadata = new AudioMetadata();

        // Assert
        metadata.Duration.Should().Be(0.0);
        metadata.SampleRate.Should().Be(0);
        metadata.FileSizeBytes.Should().Be(0);
    }

    [Fact]
    public void PythonModelConfiguration_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var config = TestDataFactory.CreateTestPythonModelConfig();

        // Assert
        config.PythonExecutablePath.Should().Be("python");
        config.ModelScriptPath.Should().Be("examples/mock_inference.py");
        config.ModelFilePath.Should().Be("examples/mock_model.pth");
        config.TimeoutSeconds.Should().Be(30);
        config.WorkingDirectory.Should().Be(".");
    }

    [Fact]
    public void PreprocessingConfiguration_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var config = TestDataFactory.CreateTestPreprocessingConfig();

        // Assert
        config.PreprocessingScriptPath.Should().Be("examples/mock_preprocessing.py");
        config.TempDirectory.Should().Be("temp");
        config.MaxFileSizeMB.Should().Be(50);
        config.SupportedFormats.Should().Contain("mp3");
        config.SupportedFormats.Should().Contain("wav");
        config.SupportedFormats.Should().Contain("flac");
        config.SupportedFormats.Should().Contain("m4a");
    }

    [Theory]
    [InlineData(50, "very_slow")]
    [InlineData(75, "slow")]
    [InlineData(110, "moderate")]
    [InlineData(130, "fast")]
    [InlineData(160, "very_fast")]
    public void BpmPrediction_ShouldCategorizeBpmCorrectly(double bpmValue, string expectedCategory)
    {
        // Act
        var bpm = new BpmPrediction
        {
            Value = bpmValue,
            Category = GetBpmCategory(bpmValue)
        };

        // Assert
        bpm.Category.Should().Be(expectedCategory);
    }

    [Fact]
    public void MusicAnalysisResponse_ShouldAcceptComplexData()
    {
        // Arrange
        var response = TestDataFactory.CreateSampleResponse();

        // Assert
        response.FileName.Should().Be("test_audio.wav");
        response.ProcessingTimeMs.Should().Be(1500);
        response.Predictions.Genre.Label.Should().Be("rock");
        response.Predictions.Genre.Confidence.Should().Be(0.85);
        response.Predictions.Mood.Label.Should().Be("energetic");
        response.Predictions.Bpm.Value.Should().Be(120.5);
        response.Predictions.Key.Label.Should().Be("C");
        response.Metadata.Duration.Should().Be(180.5);
        response.Metadata.SampleRate.Should().Be(44100);
        response.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ClassificationResult_ShouldHandleAllProbabilities()
    {
        // Arrange
        var probabilities = new Dictionary<string, double>
        {
            { "rock", 0.85 },
            { "pop", 0.10 },
            { "jazz", 0.05 }
        };

        // Act
        var result = new ClassificationResult
        {
            Label = "rock",
            Confidence = 0.85,
            AllProbabilities = probabilities
        };

        // Assert
        result.AllProbabilities.Should().NotBeNull();
        result.AllProbabilities.Should().HaveCount(3);
        result.AllProbabilities!["rock"].Should().Be(0.85);
        result.AllProbabilities["pop"].Should().Be(0.10);
        result.AllProbabilities["jazz"].Should().Be(0.05);
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
