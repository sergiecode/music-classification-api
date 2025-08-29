using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using MusicClassificationApi.Controllers;
using MusicClassificationApi.Services;

namespace Tests.Controllers;

/// <summary>
/// Unit tests for HealthController
/// </summary>
public class HealthControllerTests
{
    private readonly Mock<IMusicClassificationService> _mockMusicService;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockMusicService = new Mock<IMusicClassificationService>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockMusicService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHealthStatus_WhenServiceIsHealthy_ShouldReturnOkWithHealthyStatus()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetHealthStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        
        response.Should().NotBeNull();
        // Check that the response contains status and it's "healthy"
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        statusProperty!.GetValue(response).Should().Be("healthy");
    }

    [Fact]
    public async Task GetHealthStatus_WhenServiceIsUnhealthy_ShouldReturnServiceUnavailable()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetHealthStatus();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
        
        var response = objectResult.Value;
        response.Should().NotBeNull();
        
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        statusProperty!.GetValue(response).Should().Be("unhealthy");
    }

    [Fact]
    public async Task GetHealthStatus_WhenServiceThrowsException_ShouldReturnServiceUnavailable()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetHealthStatus();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
        
        var response = objectResult.Value;
        response.Should().NotBeNull();
        
        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        statusProperty!.GetValue(response).Should().Be("unhealthy");
    }

    [Fact]
    public void GetApiInfo_ShouldReturnOkWithApiInformation()
    {
        // Act
        var result = _controller.GetApiInfo();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        
        response.Should().NotBeNull();
        
        // Check that the response contains expected properties
        var responseType = response!.GetType();
        var nameProperty = responseType.GetProperty("name");
        nameProperty!.GetValue(response).Should().Be("Music Classification API");
        
        var versionProperty = responseType.GetProperty("version");
        versionProperty!.GetValue(response).Should().Be("1.0.0");
        
        var authorProperty = responseType.GetProperty("author");
        authorProperty!.GetValue(response).Should().Be("Sergie Code");
    }

    [Fact]
    public async Task GetHealthStatus_ShouldCallServiceHealthCheckOnce()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ReturnsAsync(true);

        // Act
        await _controller.GetHealthStatus();

        // Assert
        _mockMusicService.Verify(s => s.IsHealthyAsync(), Times.Once);
    }

    [Fact]
    public async Task GetHealthStatus_ResponseShouldContainTimestamp()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ReturnsAsync(true);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _controller.GetHealthStatus();

        // Assert
        var afterCall = DateTime.UtcNow;
        
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        
        var responseType = response!.GetType();
        var timestampProperty = responseType.GetProperty("timestamp");
        var timestamp = (DateTime)timestampProperty!.GetValue(response)!;
        
        timestamp.Should().BeOnOrAfter(beforeCall);
        timestamp.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public async Task GetHealthStatus_ResponseShouldContainServicesStatus()
    {
        // Arrange
        _mockMusicService
            .Setup(s => s.IsHealthyAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetHealthStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        
        var responseType = response!.GetType();
        var servicesProperty = responseType.GetProperty("services");
        servicesProperty.Should().NotBeNull();
        
        var services = servicesProperty!.GetValue(response);
        services.Should().NotBeNull();
    }
}
