# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Install Python and audio processing dependencies
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-venv \
    libsndfile1 \
    ffmpeg \
    git \
    && rm -rf /var/lib/apt/lists/*

# Create symlink for python
RUN ln -s /usr/bin/python3 /usr/bin/python

# Use the SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["MusicClassificationApi.csproj", "./"]
RUN dotnet restore "MusicClassificationApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "MusicClassificationApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MusicClassificationApi.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Create directories for temporary files and models
RUN mkdir -p temp models

# Copy Python dependencies and models (these would be mounted or copied in real deployment)
# COPY ../music-classification-model /app/music-classification-model
# COPY ../music-classification-preprocessing /app/music-classification-preprocessing

# Set up Python environment
RUN python3 -m venv /app/venv
ENV PATH="/app/venv/bin:$PATH"

# Install Python packages that would be needed
RUN pip install --no-cache-dir \
    torch \
    torchaudio \
    librosa \
    numpy \
    scikit-learn

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/api/health || exit 1

ENTRYPOINT ["dotnet", "MusicClassificationApi.dll"]
