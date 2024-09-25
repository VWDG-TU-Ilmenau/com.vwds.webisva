using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicInputVisualizer : MonoBehaviour
{
    public float ScaleFactor;
    public AudioSource MicAudioSource;
    public GameObject[] MicrophoneVisualizationElement;
    // Start is called before the first frame update
    void Start()
    {
        MicAudioSource.clip = Microphone.Start(null, true, 10, 44100);
        MicAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (MicAudioSource == null)
            return;

        float[] spectrum = new float[256];

        MicAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            MicrophoneVisualizationElement[0].transform.localScale = (new Vector3(1, Mathf.Clamp((spectrum[i] * ScaleFactor * 0.1f), 1f, 2f), 1));
            MicrophoneVisualizationElement[1].transform.localScale = (new Vector3(1, Mathf.Clamp((spectrum[i] * ScaleFactor), 1f, 2f), 1));
            MicrophoneVisualizationElement[2].transform.localScale = (new Vector3(1, Mathf.Clamp((spectrum[i] * ScaleFactor), 1f, 2f), 1));
            MicrophoneVisualizationElement[3].transform.localScale = (new Vector3(1, Mathf.Clamp((spectrum[i] * ScaleFactor * 0.1f), 1f, 2f), 1));
        }
    }
}
