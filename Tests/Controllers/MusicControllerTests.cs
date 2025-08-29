using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using MusicClassificationApi.Controllers;
using MusicClassificationApi.Models;
using MusicClassificationApi.Services;
using Tests.TestData;

namespace Tests.Controllers;

/// <summary>
/// Unit tests for MusicController
/// </summary>
public class MusicControllerTests
{
    private readonly Mock<IMusicClassificationService> _mockMusicService;
    private readonly Mock<ILogger<MusicController>> _mockLogger;
    private readonly MusicController _controller;

    public MusicControllerTests()
    {
        _mockMusicService = new Mock<IMusicClassificationService>();
        _mockLogger = new Mock<ILogger<MusicController>>();
        _controller = new MusicController(_mockMusicService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeMusic_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();
        var expectedResponse = TestDataFactory.CreateSampleResponse();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AnalyzeMusic(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task AnalyzeMusic_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new MusicAnalysisRequest(); // Empty request

        // Act
        var result = await _controller.AnalyzeMusic(invalidRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AnalyzeMusic_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.AnalyzeMusic(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task AnalyzeMusicUpload_WithValidFile_ShouldReturnOkResult()
    {
        // Arrange
        var audioData = TestDataFactory.CreateTestAudioData();
        var mockFile = CreateMockFormFile(audioData, "test.wav");
        var expectedResponse = TestDataFactory.CreateSampleResponse();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AnalyzeMusicUpload(mockFile);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as MusicAnalysisResponse;
        response!.Metadata.FileSizeBytes.Should().Be(audioData.Length);
    }

    [Fact]
    public async Task AnalyzeMusicUpload_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.AnalyzeMusicUpload(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AnalyzeMusicUpload_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = CreateMockFormFile(Array.Empty<byte>(), "empty.wav");

        // Act
        var result = await _controller.AnalyzeMusicUpload(mockFile);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AnalyzePreprocessedData_WithValidPaths_ShouldReturnOkResult()
    {
        // Arrange
        var featuresPath = "test_features.json";
        var spectrogramPath = "test_spectrogram.npy";
        var fileName = "test_song.wav";
        var expectedResponse = TestDataFactory.CreateSampleResponse();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AnalyzePreprocessedData(featuresPath, spectrogramPath, fileName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AnalyzePreprocessedData_WithMissingPaths_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.AnalyzePreprocessedData("", "", null);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("test.mp3")]
    [InlineData("test.wav")]
    [InlineData("test.flac")]
    [InlineData("test.m4a")]
    public async Task AnalyzeMusicUpload_WithDifferentFileTypes_ShouldProcess(string fileName)
    {
        // Arrange
        var audioData = TestDataFactory.CreateTestAudioData();
        var mockFile = CreateMockFormFile(audioData, fileName);
        var expectedResponse = TestDataFactory.CreateSampleResponse();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AnalyzeMusicUpload(mockFile);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AnalyzeMusic_ShouldCallServiceOnce()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();
        var expectedResponse = TestDataFactory.CreateSampleResponse();

        _mockMusicService
            .Setup(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.AnalyzeMusic(request);

        // Assert
        _mockMusicService.Verify(s => s.AnalyzeMusicAsync(It.IsAny<MusicAnalysisRequest>()), Times.Once);
    }

    private static IFormFile CreateMockFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        var mockFile = new Mock<IFormFile>();
        
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
               .Returns(Task.CompletedTask);

        return mockFile.Object;
    }
}
