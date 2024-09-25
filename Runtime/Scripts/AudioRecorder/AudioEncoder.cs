using UnityEngine;
using System;
using System.Text;

public class AudioEncoder
{
    public string EncodeToBase64(byte[] audioData)
    {
        return Convert.ToBase64String(audioData);
    }

    public byte[] DecodeFromBase64(string encodedData)
    {
        return Convert.FromBase64String(encodedData);
    }

    public AudioClip DecodeBase64ToAudioClip(string base64Audio)
    {
        try
        {
            byte[] audioBytes = Convert.FromBase64String(base64Audio);
            return WavUtility.ToAudioClip(audioBytes);
        }
        catch (Exception e)
        {
            Debug.LogError("Error decoding Base64 to AudioClip: " + e.Message);
            return null;
        }
    }
}
