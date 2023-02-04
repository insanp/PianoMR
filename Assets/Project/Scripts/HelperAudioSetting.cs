using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperAudioSetting
{
    public static readonly int[] dspBufferSizes = { 128, 256, 512 };
    public static readonly int[] sampleRates = { 44100, 48000, 96000 };
    public static readonly int[] fftSizes = { 2048, 4096, 8192 };
}
