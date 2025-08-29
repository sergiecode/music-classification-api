#!/usr/bin/env python3
"""
Test script for the Music Classification API.
Tests all available endpoints.
"""

import requests
import json
import base64
import os
from pathlib import Path

# API base URL
API_BASE_URL = "http://localhost:5242"

def test_health_endpoint():
    """Test the health check endpoint."""
    print("🔍 Testing health endpoint...")
    
    try:
        response = requests.get(f"{API_BASE_URL}/api/health")
        print(f"Status Code: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Health check failed: {e}")
        return False

def test_info_endpoint():
    """Test the API info endpoint."""
    print("\n📋 Testing info endpoint...")
    
    try:
        response = requests.get(f"{API_BASE_URL}/api/health/info")
        print(f"Status Code: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Info endpoint failed: {e}")
        return False

def create_dummy_audio_file():
    """Create a small dummy audio file for testing."""
    # Create a simple WAV file header for testing
    # This creates a minimal valid WAV file
    wav_header = bytes([
        0x52, 0x49, 0x46, 0x46,  # "RIFF"
        0x24, 0x00, 0x00, 0x00,  # File size - 8
        0x57, 0x41, 0x56, 0x45,  # "WAVE"
        0x66, 0x6D, 0x74, 0x20,  # "fmt "
        0x10, 0x00, 0x00, 0x00,  # Subchunk1Size
        0x01, 0x00,              # AudioFormat (PCM)
        0x01, 0x00,              # NumChannels (1)
        0x44, 0xAC, 0x00, 0x00,  # SampleRate (44100)
        0x88, 0x58, 0x01, 0x00,  # ByteRate
        0x02, 0x00,              # BlockAlign
        0x10, 0x00,              # BitsPerSample (16)
        0x64, 0x61, 0x74, 0x61,  # "data"
        0x00, 0x00, 0x00, 0x00   # Subchunk2Size (0 for empty)
    ])
    
    # Ensure temp directory exists
    os.makedirs("temp", exist_ok=True)
    
    # Write dummy file
    test_file_path = "temp/test_audio.wav"
    with open(test_file_path, "wb") as f:
        f.write(wav_header)
    
    return test_file_path

def test_analyze_with_json():
    """Test analysis with JSON payload."""
    print("\n🎵 Testing analyze endpoint with JSON payload...")
    
    try:
        # Create dummy audio data
        test_file = create_dummy_audio_file()
        
        with open(test_file, "rb") as f:
            audio_bytes = f.read()
            audio_data = base64.b64encode(audio_bytes).decode('utf-8')
        
        payload = {
            "audioData": audio_data,
            "fileName": "test_audio.wav",
            "format": "wav"
        }
        
        response = requests.post(
            f"{API_BASE_URL}/api/music/analyze",
            json=payload,
            headers={"Content-Type": "application/json"}
        )
        
        print(f"Status Code: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
        
        # Clean up
        os.remove(test_file)
        
        return response.status_code == 200
    except Exception as e:
        print(f"❌ JSON analysis failed: {e}")
        return False

def test_upload_endpoint():
    """Test file upload endpoint."""
    print("\n📤 Testing upload endpoint...")
    
    try:
        # Create dummy audio file
        test_file = create_dummy_audio_file()
        
        with open(test_file, "rb") as f:
            files = {"file": ("test_audio.wav", f, "audio/wav")}
            response = requests.post(
                f"{API_BASE_URL}/api/music/analyze/upload",
                files=files
            )
        
        print(f"Status Code: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
        
        # Clean up
        os.remove(test_file)
        
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Upload analysis failed: {e}")
        return False

def test_preprocessed_endpoint():
    """Test preprocessed data endpoint."""
    print("\n🔬 Testing preprocessed data endpoint...")
    
    try:
        # Create dummy feature and spectrogram files
        features_path = "temp/dummy_features.json"
        spectrogram_path = "temp/dummy_spectrogram.npy"
        
        # Create dummy files
        os.makedirs("temp", exist_ok=True)
        
        with open(features_path, "w") as f:
            json.dump({"dummy": "features"}, f)
        
        with open(spectrogram_path, "wb") as f:
            f.write(b"dummy_spectrogram_data")
        
        params = {
            "featuresPath": features_path,
            "spectrogramPath": spectrogram_path,
            "fileName": "test_song.wav"
        }
        
        response = requests.post(
            f"{API_BASE_URL}/api/music/analyze/preprocessed",
            params=params
        )
        
        print(f"Status Code: {response.status_code}")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
        
        # Clean up
        os.remove(features_path)
        os.remove(spectrogram_path)
        
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Preprocessed analysis failed: {e}")
        return False

def main():
    """Run all tests."""
    print("🚀 Starting Music Classification API Tests")
    print("=" * 50)
    
    tests = [
        ("Health Check", test_health_endpoint),
        ("API Info", test_info_endpoint),
        ("JSON Analysis", test_analyze_with_json),
        ("File Upload", test_upload_endpoint),
        ("Preprocessed Data", test_preprocessed_endpoint),
    ]
    
    results = []
    for test_name, test_func in tests:
        try:
            result = test_func()
            results.append((test_name, result))
            status = "✅ PASSED" if result else "❌ FAILED"
            print(f"\n{status}: {test_name}")
        except Exception as e:
            results.append((test_name, False))
            print(f"\n❌ FAILED: {test_name} - Exception: {e}")
    
    print("\n" + "=" * 50)
    print("📊 Test Results Summary:")
    print("=" * 50)
    
    passed = sum(1 for _, result in results if result)
    total = len(results)
    
    for test_name, result in results:
        status = "✅ PASSED" if result else "❌ FAILED"
        print(f"{status}: {test_name}")
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 All tests passed! API is working correctly.")
    else:
        print("⚠️  Some tests failed. Check the output above for details.")

if __name__ == "__main__":
    main()
