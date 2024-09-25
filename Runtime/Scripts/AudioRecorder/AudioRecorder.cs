using UnityEngine;
using System.IO;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource MicAudioSource;
    private AudioClip audioClip;
    private const int sampleRate = 44100;
    private const int maxRecordingDuration = 10; // 10 seconds
    private void Start()
    {
        Debug.Log("Microphone Name: " + Microphone.devices[0]);
    }
    public void StartRecording()
    {
        audioClip = Microphone.Start(null, true, maxRecordingDuration, sampleRate);
    }
    public byte[] StopRecordingAndGetWav()
    {
        Microphone.End(null);
        return WavUtility.FromAudioClip(audioClip);
    }
}
