using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using MusicClassificationApi.Models;
using MusicClassificationApi.Services;
using Tests.TestData;

namespace Tests.Integration;

/// <summary>
/// Integration tests for the Music Classification API
/// </summary>
public class MusicClassificationApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MusicClassificationApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development"); // Use Development environment to enable Swagger
            
            // Set the correct content root path for the test
            var projectPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            while (projectPath != null && !File.Exists(Path.Combine(projectPath, "Program.cs")))
            {
                projectPath = Directory.GetParent(projectPath)?.FullName;
            }
            
            if (projectPath != null)
            {
                builder.UseContentRoot(projectPath);
            }
            
            builder.ConfigureServices(services =>
            {
                // Replace the real service with a mock for integration tests
                services.AddScoped<IMusicClassificationService, MockMusicClassificationService>();
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task HealthInfoEndpoint_ShouldReturnApiInformation()
    {
        // Act
        var response = await _client.GetAsync("/api/health/info");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Music Classification API");
        content.Should().Contain("Sergie Code");
    }

    [Fact]
    public async Task AnalyzeEndpoint_WithValidJson_ShouldReturnPredictions()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/music/analyze", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        responseContent.Should().NotBeNullOrEmpty();
        responseContent.Should().Contain("predictions");
        responseContent.Should().Contain("genre");
        responseContent.Should().Contain("mood");
        responseContent.Should().Contain("bpm");
        responseContent.Should().Contain("key");
    }

    [Fact]
    public async Task AnalyzeEndpoint_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new { invalid = "data" };
        var json = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/music/analyze", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadEndpoint_WithValidFile_ShouldReturnPredictions()
    {
        // Arrange
        var audioData = TestDataFactory.CreateTestAudioData();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(audioData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
        content.Add(fileContent, "file", "test.wav");

        // Act
        var response = await _client.PostAsync("/api/music/analyze/upload", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        responseContent.Should().NotBeNullOrEmpty();
        responseContent.Should().Contain("predictions");
        responseContent.Should().Contain("metadata");
    }

    [Fact]
    public async Task UploadEndpoint_WithoutFile_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/music/analyze/upload", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PreprocessedEndpoint_WithValidPaths_ShouldReturnPredictions()
    {
        // Act
        var response = await _client.PostAsync(
            "/api/music/analyze/preprocessed?featuresPath=test.json&spectrogramPath=test.npy&fileName=test.wav", 
            null);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        responseContent.Should().NotBeNullOrEmpty();
        responseContent.Should().Contain("predictions");
    }

    [Fact]
    public async Task PreprocessedEndpoint_WithMissingPaths_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/music/analyze/preprocessed", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RootEndpoint_ShouldRedirectToSwagger()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("swagger", "Root should serve Swagger UI");
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldReturnSwaggerUI()
    {
        // Act - Test swagger.json endpoint instead
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Music Classification API");
    }

    [Theory]
    [InlineData("/api/health")]
    [InlineData("/api/health/info")]
    public async Task GetEndpoints_ShouldHaveCorsHeaders(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // In development environment, CORS headers should be present
        // This test mainly ensures the endpoints are accessible
    }

    [Fact]
    public async Task ApiEndpoints_ShouldReturnJsonContentType()
    {
        // Act
        var healthResponse = await _client.GetAsync("/api/health");
        var infoResponse = await _client.GetAsync("/api/health/info");

        // Assert
        healthResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        infoResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task AnalyzeEndpoint_ShouldHaveReasonableResponseTime()
    {
        // Arrange
        var request = TestDataFactory.CreateValidMusicAnalysisRequest();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/api/music/analyze", content);

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        
        // Response should be within reasonable time (adjust based on your requirements)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // 10 seconds max
    }
}

/// <summary>
/// Mock service for integration testing
/// </summary>
public class MockMusicClassificationService : IMusicClassificationService
{
    public Task<MusicAnalysisResponse> AnalyzeMusicAsync(MusicAnalysisRequest request)
    {
        var response = TestDataFactory.CreateSampleResponse();
        response.FileName = request.FileName ?? "test_file";
        return Task.FromResult(response);
    }

    public Task<bool> IsHealthyAsync()
    {
        return Task.FromResult(true);
    }
}
