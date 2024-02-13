using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // ... (other methods remain unchanged)

    // Save audio clip to WAV file
    public static byte[] SaveToWav(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // Write WAV header
            WriteWavHeader(writer, clip);

            // Convert AudioClip data to byte array
            float[] data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);

            // Write audio data
            ConvertAndWrite(writer, data);

            // Close the writer and return the byte array
            writer.Close();
            return stream.ToArray();
        }
    }

    private static void WriteWavHeader(BinaryWriter writer, AudioClip clip)
    {
        writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + clip.samples * 2);
        writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
        writer.Write(new char[4] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((ushort)1);
        writer.Write((ushort)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(clip.frequency * clip.channels * 2);
        writer.Write((ushort)(clip.channels * 2));
        writer.Write((ushort)16);
        writer.Write(new char[4] { 'd', 'a', 't', 'a' });
        writer.Write(clip.samples * 2);
    }

    private static void ConvertAndWrite(BinaryWriter writer, float[] samples)
    {
        Int16[] intData = new Int16[samples.Length];
        // Convert float to Int16
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767.0f);
        }
        // Write data as bytes
        byte[] byteData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
        writer.Write(byteData);
    }

    // ... (other methods remain unchanged)
}
