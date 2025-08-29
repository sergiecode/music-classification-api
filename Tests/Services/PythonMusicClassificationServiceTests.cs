using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using MusicClassificationApi.Services;
using MusicClassificationApi.Models;
using Tests.TestData;

namespace Tests.Services;

/// <summary>
/// Unit tests for PythonMusicClassificationService
/// </summary>
public class PythonMusicClassificationServiceTests
{
    private readonly Mock<ILogger<PythonMusicClassificationService>> _mockLogger;
    private readonly PythonModelConfiguration _modelConfig;
    private readonly PreprocessingConfiguration _preprocessingConfig;
    private readonly PythonMusicClassificationService _service;

    public PythonMusicClassificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<PythonMusicClassificationService>>();
        _modelConfig = TestDataFactory.CreateTestPythonModelConfig();
        _preprocessingConfig = TestDataFactory.CreateTestPreprocessingConfig();
        _service = new PythonMusicClassificationService(_modelConfig, _preprocessingConfig, _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeMusicAsync_WithValidAudioData_ShouldReturnValidResponse()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();

        // Act
        var result = await _service.AnalyzeMusicAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test_audio.wav");
        result.ProcessingTimeMs.Should().BeGreaterThan(0);
        result.Predictions.Should().NotBeNull();
    }

    [Fact]
    public async Task AnalyzeMusicAsync_WithNullRequest_ShouldHandleGracefully()
    {
        // Arrange
        var request = new MusicAnalysisRequest();

        // Act
        var result = await _service.AnalyzeMusicAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeMusicAsync_WithPreprocessedPaths_ShouldReturnValidResponse()
    {
        // Arrange
        var request = TestDataFactory.CreatePreprocessedRequest();
        
        // Create test files
        Directory.CreateDirectory("Tests/TestData");
        await File.WriteAllTextAsync("Tests/TestData/test_features.json", @"{""tempo"": 120}");
        await File.WriteAllBytesAsync("Tests/TestData/test_spectrogram.npy", new byte[] { 1, 2, 3, 4 });

        try
        {
            // Act
            var result = await _service.AnalyzeMusicAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.FileName.Should().Be("test_song.wav");
        }
        finally
        {
            // Cleanup
            if (File.Exists("Tests/TestData/test_features.json"))
                File.Delete("Tests/TestData/test_features.json");
            if (File.Exists("Tests/TestData/test_spectrogram.npy"))
                File.Delete("Tests/TestData/test_spectrogram.npy");
        }
    }

    [Fact]
    public async Task IsHealthyAsync_WithValidPythonPath_ShouldReturnTrue()
    {
        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHealthyAsync_WithInvalidPythonPath_ShouldReturnFalse()
    {
        // Arrange
        var invalidConfig = new PythonModelConfiguration
        {
            PythonExecutablePath = "invalid_python_path",
            ModelScriptPath = "invalid_script.py",
            ModelFilePath = "invalid_model.pth",
            TimeoutSeconds = 5,
            WorkingDirectory = "."
        };

        var service = new PythonMusicClassificationService(invalidConfig, _preprocessingConfig, _mockLogger.Object);

        // Act
        var result = await service.IsHealthyAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("mp3")]
    [InlineData("wav")]
    [InlineData("flac")]
    [InlineData("m4a")]
    public async Task AnalyzeMusicAsync_WithDifferentFormats_ShouldProcess(string format)
    {
        // Arrange
        var request = new MusicAnalysisRequest
        {
            AudioData = Convert.ToBase64String(TestDataFactory.CreateTestAudioData()),
            FileName = $"test_audio.{format}",
            Format = format
        };

        // Act
        var result = await _service.AnalyzeMusicAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be($"test_audio.{format}");
    }

    [Fact]
    public async Task AnalyzeMusicAsync_WithLargeFile_ShouldProcessSuccessfully()
    {
        // Arrange
        var largeAudioData = new byte[1024 * 1024]; // 1MB
        var request = new MusicAnalysisRequest
        {
            AudioData = Convert.ToBase64String(largeAudioData),
            FileName = "large_audio.wav",
            Format = "wav"
        };

        // Act
        var result = await _service.AnalyzeMusicAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ProcessingTimeMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_WithNullParameters_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PythonMusicClassificationService(null!, _preprocessingConfig, _mockLogger.Object));

        Assert.Throws<ArgumentNullException>(() =>
            new PythonMusicClassificationService(_modelConfig, null!, _mockLogger.Object));

        Assert.Throws<ArgumentNullException>(() =>
            new PythonMusicClassificationService(_modelConfig, _preprocessingConfig, null!));
    }
}
