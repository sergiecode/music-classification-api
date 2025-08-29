using MusicClassificationApi.Models;
using MusicClassificationApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Music Classification API",
        Version = "v1",
        Description = "ASP.NET Core API for music classification using AI models",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Sergie Code",
            Url = new Uri("https://github.com/sergiecode")
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure model settings from appsettings.json
builder.Services.Configure<PythonModelConfiguration>(
    builder.Configuration.GetSection("PythonModel"));
builder.Services.Configure<PreprocessingConfiguration>(
    builder.Configuration.GetSection("Preprocessing"));

// Register services
builder.Services.AddSingleton(provider =>
    provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PythonModelConfiguration>>().Value);
builder.Services.AddSingleton(provider =>
    provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PreprocessingConfiguration>>().Value);

builder.Services.AddScoped<IMusicClassificationService, PythonMusicClassificationService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Music Classification API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
