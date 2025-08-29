#!/usr/bin/env python3
"""
Mock Python inference script for music classification API testing.
This simulates the actual inference script from music-classification-model repository.

Usage:
python mock_inference.py --audio-file path/to/audio.mp3 --model-path path/to/model.pth --output-format json
"""

import argparse
import json
import random
import time
import sys
from pathlib import Path

def mock_analyze_audio(audio_file_path, model_path):
    """
    Mock function that simulates music analysis.
    In the real implementation, this would load the PyTorch model and process the audio.
    """
    
    # Simulate processing time
    time.sleep(random.uniform(1.0, 3.0))
    
    # Mock genre predictions
    genres = ["rock", "pop", "jazz", "classical", "electronic", "hip_hop", "country", "blues"]
    genre = random.choice(genres)
    
    # Mock mood predictions  
    moods = ["happy", "sad", "energetic", "calm", "aggressive"]
    mood = random.choice(moods)
    
    # Mock BPM prediction
    bpm = random.uniform(60, 180)
    
    # Mock key prediction
    keys = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"]
    key = random.choice(keys)
    
    # Create mock response in the format expected by the API
    response = {
        "predictions": {
            "genre": {
                "label": genre,
                "confidence": random.uniform(0.6, 0.95)
            },
            "mood": {
                "label": mood,
                "confidence": random.uniform(0.6, 0.95)
            },
            "bpm": {
                "value": round(bpm, 1),
                "confidence": random.uniform(0.7, 0.95)
            },
            "key": {
                "label": key,
                "confidence": random.uniform(0.5, 0.9)
            }
        },
        "metadata": {
            "model_version": "1.0.0",
            "processing_time_seconds": random.uniform(1.0, 3.0),
            "audio_duration_seconds": random.uniform(30, 300)
        }
    }
    
    return response

def main():
    parser = argparse.ArgumentParser(description="Mock Music Classification Inference Script")
    parser.add_argument("--audio-file", type=str, help="Path to audio file")
    parser.add_argument("--features-file", type=str, help="Path to features JSON file")
    parser.add_argument("--spectrogram-file", type=str, help="Path to spectrogram NPY file")
    parser.add_argument("--model-path", type=str, required=True, help="Path to model file")
    parser.add_argument("--output-format", type=str, default="json", choices=["json"], help="Output format")
    
    args = parser.parse_args()
    
    try:
        # Validate inputs
        if not args.audio_file and not (args.features_file and args.spectrogram_file):
            raise ValueError("Either --audio-file or both --features-file and --spectrogram-file must be provided")
        
        if args.audio_file and not Path(args.audio_file).exists():
            raise FileNotFoundError(f"Audio file not found: {args.audio_file}")
        
        if not Path(args.model_path).exists():
            # For mock purposes, we'll create a dummy model file if it doesn't exist
            Path(args.model_path).parent.mkdir(parents=True, exist_ok=True)
            Path(args.model_path).touch()
        
        # Perform mock analysis
        result = mock_analyze_audio(args.audio_file or args.features_file, args.model_path)
        
        # Output result as JSON
        if args.output_format == "json":
            print(json.dumps(result, indent=None, separators=(',', ':')))
        
        sys.exit(0)
        
    except Exception as e:
        error_response = {
            "error": str(e),
            "type": type(e).__name__
        }
        print(json.dumps(error_response), file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
