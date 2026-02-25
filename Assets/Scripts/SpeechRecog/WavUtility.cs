using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // 保存AudioClip为WAV文件
    public static bool SaveAudioClipToWav(AudioClip audioClip, string filePath)
    {
        if (audioClip == null)
        {
            Debug.LogError("Invalid audio clip!");
            return false;
        }

        // 获取原始音频数据
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        // 将float数组转换为byte数组（16位PCM）
        byte[] pcmBytes = ConvertFloatTo16BitPcm(samples);

        // 创建WAV头信息
        byte[] header = CreateWavHeader(audioClip, pcmBytes.Length);

        // 合并头文件和音频数据
        byte[] wavFile = new byte[header.Length + pcmBytes.Length];
        Buffer.BlockCopy(header, 0, wavFile, 0, header.Length);
        Buffer.BlockCopy(pcmBytes, 0, wavFile, header.Length, pcmBytes.Length);

        // 保存文件
        File.WriteAllBytes(filePath, wavFile);

        Debug.Log("AudioClip saved to: " + filePath);
        return true;
    }

    // 将float音频数据转换为16位PCM格式
    private static byte[] ConvertFloatTo16BitPcm(float[] samples)
    {
        byte[] pcmBytes = new byte[samples.Length * 2];
        int byteIndex = 0;

        foreach (float sample in samples)
        {
            short value = (short)(sample * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(value);

            pcmBytes[byteIndex++] = bytes[0];
            pcmBytes[byteIndex++] = bytes[1];
        }

        return pcmBytes;
    }

    // 创建WAV文件头
    private static byte[] CreateWavHeader(AudioClip clip, int pcmBytesLength)
    {
        int channelCount = clip.channels;
        int sampleRate = (int)clip.frequency;
        int bitDepth = 16; // 16-bit PCM

        using (MemoryStream headerStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(headerStream))
            {
                // RIFF头
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + pcmBytesLength); // 文件总大小 - 8
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt子块
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);           // PCM格式头大小
                writer.Write((short)1);     // PCM格式标签
                writer.Write((short)channelCount);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channelCount * bitDepth / 8);     // 字节率
                writer.Write((short)(channelCount * bitDepth / 8));         // 块对齐
                writer.Write((short)bitDepth);

                // data子块
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(pcmBytesLength);
            }

            return headerStream.ToArray();
        }
    }
}
