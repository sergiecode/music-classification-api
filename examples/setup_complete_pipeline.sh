#!/usr/bin/env bash
# Complete Integration Setup Script
# This script demonstrates how to set up all three repositories to work together

echo "🎵 Music Classification Pipeline Setup"
echo "======================================"

# Create main directory
echo "📁 Creating main directory structure..."
mkdir -p AI-Music-Tools
cd AI-Music-Tools

# Repository URLs (replace with actual URLs)
PREPROCESSING_REPO="https://github.com/sergiecode/music-classification-preprocessing.git"
MODEL_REPO="https://github.com/sergiecode/music-classification-model.git"
API_REPO="https://github.com/sergiecode/music-classification-api.git"

# Clone repositories
echo "📥 Cloning repositories..."
git clone $PREPROCESSING_REPO music-classification-preprocessing
git clone $MODEL_REPO music-classification-model
git clone $API_REPO music-classification-api

# Setup preprocessing
echo "🔧 Setting up preprocessing environment..."
cd music-classification-preprocessing
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
cd ..

# Setup model training
echo "🤖 Setting up model training environment..."
cd music-classification-model
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt

# Download or train model
echo "📊 Training model (or download pre-trained)..."
# python train.py --data ../music-classification-preprocessing/data/manifest.json --epochs 50
echo "Note: Add your model training command here"

# Export model for API
echo "📤 Exporting model for API..."
# python export_model.py --model models/best_model.pth --output models/api_model.pth
echo "Note: Add your model export command here"
cd ..

# Setup API
echo "🌐 Setting up .NET API..."
cd music-classification-api
dotnet restore
dotnet build

# Update configuration
echo "⚙️ Updating API configuration..."
cat > appsettings.Production.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "PythonModel": {
    "PythonExecutablePath": "python",
    "ModelScriptPath": "../music-classification-model/inference.py",
    "ModelFilePath": "../music-classification-model/models/api_model.pth",
    "TimeoutSeconds": 30,
    "WorkingDirectory": "../music-classification-model"
  },
  "Preprocessing": {
    "PreprocessingScriptPath": "../music-classification-preprocessing/src/cli.py",
    "TempDirectory": "temp",
    "MaxFileSizeMB": 50,
    "SupportedFormats": ["mp3", "wav", "flac", "m4a"]
  }
}
EOF

echo "✅ Setup complete!"
echo ""
echo "🚀 To start the API:"
echo "cd music-classification-api"
echo "dotnet run"
echo ""
echo "🧪 To test the API:"
echo "curl http://localhost:5000/api/health"
echo ""
echo "📖 Check README.md files in each repository for detailed usage instructions."
