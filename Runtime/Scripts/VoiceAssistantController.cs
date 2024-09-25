using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceAssistantController : MonoBehaviour
{
    public static VoiceAssistantController Instance;
    // Start is called before the first frame updateprivate AudioRecorder audioRecorder;
    public AudioSource GuideAudioSource;
    public WebSocketClient WebSocketClient;
    public AudioRecorder AudioRecorder;
    private AudioEncoder audioEncoder;
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>(); // Queue to store audio clips
    private bool isPlaying = false; // Flag to check if audio is currently playing

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        // audioRecorder = gameObject.AddComponent<AudioRecorder>();
        audioEncoder = new AudioEncoder();
        // webSocketClient = gameObject.AddComponent<WebSocketClient>();
    }

    [ContextMenu("Start Recording")]
    public void RecordAudio()
    {
        AudioRecorder.StartRecording();
    }

    [ContextMenu("Stop Recording")]
    public void StopAndSendAudio()
    {
        byte[] audioData = AudioRecorder.StopRecordingAndGetWav();
        string encodedAudio = audioEncoder.EncodeToBase64(audioData);
        Debug.Log("Ecoded Audio" + encodedAudio);
        WebSocketClient.SendMessage("REQUEST_STREAM", encodedAudio);
    }

    private IEnumerator PlayAudioQueue()
    {
        isPlaying = true;

        while (audioQueue.Count > 0)
        {
            AudioClip clip = audioQueue.Dequeue();
            if (clip == null)
            {
                Debug.LogWarning("Received null AudioClip.");
                continue;
            }

            GuideAudioSource.clip = clip;
            GuideAudioSource.Play();
            Debug.Log("Playing audio clip: " + clip.name);

            // Wait until the current audio clip finishes playing
            yield return new WaitUntil(() => !GuideAudioSource.isPlaying);
        }

        isPlaying = false;
        Debug.Log("Finished playing all audio clips.");
    }

    public void DecodeMessage(string base64Audio)
    {
        // Decode and add to the queue
        AudioClip clip = audioEncoder.DecodeBase64ToAudioClip(base64Audio);

        if (clip != null)
        {
            AddClipToQueue(clip);
        }
    }

    private void AddClipToQueue(AudioClip clip)
    {
        audioQueue.Enqueue(clip);
        if (!isPlaying)
        {
            StartCoroutine(PlayAudioQueue());
        }
    }
}