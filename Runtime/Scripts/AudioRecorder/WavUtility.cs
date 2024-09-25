using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // Convert WAV data to AudioClip
    public static AudioClip ToAudioClip(byte[] wavFile)
    {
        using (MemoryStream memoryStream = new MemoryStream(wavFile))
        using (BinaryReader reader = new BinaryReader(memoryStream))
        {
            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            // Seek to "data" chunk
            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();

            // Read audio data
            byte[] audioData = reader.ReadBytes(dataSize);
            float[] floatArray = Convert16BitToFloat(audioData);

            // Create AudioClip
            AudioClip audioClip = AudioClip.Create("Received Audio", floatArray.Length / channels, channels, sampleRate, false);
            audioClip.SetData(floatArray, 0);
            return audioClip;
        }
    }

    // Convert 16-bit PCM audio data to float array
    private static float[] Convert16BitToFloat(byte[] data)
    {
        int length = data.Length / 2; // 2 bytes per sample (16 bits)
        float[] floatArray = new float[length];

        for (int i = 0; i < length; i++)
        {
            short sample = BitConverter.ToInt16(data, i * 2);
            floatArray[i] = sample / 32768f; // Convert to range -1.0f to 1.0f
        }

        return floatArray;
    }
    // Convert an AudioClip to a byte array in WAV format
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (var memoryStream = new MemoryStream())
        {
            // Write WAV header
            WriteWavHeader(memoryStream, clip);
            // Write WAV data
            WriteWavData(memoryStream, clip);
            return memoryStream.ToArray();
        }
    }

    private static void WriteWavHeader(MemoryStream stream, AudioClip clip)
    {
        const int HEADER_SIZE = 44;

        // Clear the memory stream
        stream.SetLength(0);

        // Write the RIFF chunk descriptor
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes((int)(stream.Length - 8)), 0, 4); // File size - 8
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);

        // Write the fmt subchunk
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk size
        stream.Write(BitConverter.GetBytes((short)1), 0, 2); // Audio format (PCM)
        stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
        stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4); // Byte rate
        stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2); // Block align
        stream.Write(BitConverter.GetBytes((short)16), 0, 2); // Bits per sample

        // Write the data subchunk header
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes((int)(clip.samples * clip.channels * 2)), 0, 4); // Data size
    }

    private static void WriteWavData(MemoryStream stream, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];

        int rescaleFactor = 32767; // to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        stream.Write(bytesData, 0, bytesData.Length);
    }
}
