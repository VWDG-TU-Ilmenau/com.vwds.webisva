using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public AudioSource AudioSource;
    public float RotationFactor, ScaleFactor;
    public List<GameObject> Spectrum;
    private List<Vector3> spectrumScaleInitialValues;
    private List<Quaternion> spectrumRotationInitialValues;
    private bool isReset;
    // public bool isTalking;
    void Start()
    {
        spectrumScaleInitialValues = new List<Vector3>();
        spectrumRotationInitialValues = new List<Quaternion>();
        initializeInitialValues();
    }

    void Update()
    {
        if (!VoiceAssistantController.Instance.GuideAudioSource.isPlaying)
        {
            if (!isReset)
            {
                resetSpectrumRotationAndScale();
            }

            return;
        }

        isReset = false;

        float[] spectrum = new float[256];

        AudioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            Spectrum[0].transform.Rotate(new Vector3(i, spectrum[i + 1] + 10, spectrum[i + 1] + 10) * -RotationFactor);
            Spectrum[1].transform.Rotate(new Vector3(Mathf.Log(spectrum[i]) + 10, Mathf.Log(spectrum[i]) + 10, i) * RotationFactor);
            Spectrum[2].transform.Rotate(new Vector3(Mathf.Log(i), spectrum[i] - 10, spectrum[i] - 10) * RotationFactor);
            Spectrum[3].transform.Rotate(new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), Mathf.Log(i)) * -RotationFactor);

            smoothScale(Spectrum[0], adjustSpectrumScaleValues(spectrum[i], Spectrum[0].transform.localScale), 1f);
            smoothScale(Spectrum[1], adjustSpectrumScaleValues(spectrum[i], Spectrum[1].transform.localScale), 1f);
            smoothScale(Spectrum[2], adjustSpectrumScaleValues(spectrum[i], Spectrum[2].transform.localScale), 1f);
            smoothScale(Spectrum[3], adjustSpectrumScaleValues(spectrum[i], Spectrum[3].transform.localScale), 1f);
        }
    }

    private Vector3 adjustSpectrumScaleValues(float currentSpectrum, Vector3 currentScale)
    {
        Vector3 newScale = currentScale;
        int[] randomMultiplier = new int[2] { -1, 1 };
        int randomFactorX = Random.Range(0, 2);
        int randomFactorY = Random.Range(0, 2);
        int randomFactorZ = Random.Range(0, 2);

        newScale = new Vector3(Mathf.Clamp(currentScale.x + (currentSpectrum * randomMultiplier[randomFactorX]) / ScaleFactor, 0.1f, 0.12f),
            Mathf.Clamp(currentScale.y + (currentSpectrum * randomMultiplier[randomFactorY]) / ScaleFactor, 0.1f, 0.12f),
            Mathf.Clamp(currentScale.z + (currentSpectrum * randomMultiplier[randomFactorZ]) / ScaleFactor, 0.1f, 0.12f));
        return newScale;
    }

    private void smoothScale(GameObject spectrumObject, Vector3 end, float seconds)
    {
        float t = 0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime / seconds;
            spectrumObject.transform.localScale = Vector3.Lerp(spectrumObject.transform.localScale, end, Mathf.SmoothStep(0.0f, 1.0f, t));
        }
    }

    private void initializeInitialValues()
    {
        foreach (GameObject spectrum in Spectrum)
        {
            spectrumScaleInitialValues.Add(spectrum.transform.localScale);
            spectrumRotationInitialValues.Add(spectrum.transform.rotation);
        }
    }

    private void resetSpectrumRotationAndScale()
    {
        isReset = true;

        for (int i = 0; i < Spectrum.Count; i++)
        {
            smoothScale(Spectrum[i], spectrumScaleInitialValues[i], 100f);
            Spectrum[i].transform.rotation = spectrumRotationInitialValues[i];
        }
    }

}
