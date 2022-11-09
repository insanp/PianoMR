using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperPianoFreq
{
    public static int lowestMidi = 21;
    public static int highestMidi = 108;

    public static readonly float[] keys =
    {
        27.50000f, // A0
        29.13524f,
        30.86771f,
        32.70320f, // C1
        34.64783f,
        36.70810f,
        38.89087f,
        41.20344f,
        43.65353f,
        46.24930f,
        48.99943f,
        51.91309f,
        55.00000f, // A1
        58.27047f,
        61.73541f,
        65.40639f, // C2
        69.29566f,
        73.41619f,
        77.78175f,
        82.40689f,
        87.30706f,
        92.49861f,
        97.99886f,
        103.8262f,
        110.0000f, // A2
        116.5409f,
        123.4708f,
        130.8128f, // C3
        138.5913f,
        146.8324f,
        155.5635f,
        164.8138f,
        174.6141f,
        184.9972f,
        195.9977f,
        207.6523f,
        220.0000f, // A3
        233.0819f,
        246.9417f,
        261.6256f, // C4
        277.1826f,
        293.6648f,
        311.1270f,
        329.6276f,
        349.2282f,
        369.9944f,
        391.9954f,
        415.3047f,
        440.0000f, // A4
        466.1638f,
        493.8833f,
        523.2511f, // C5
        554.3653f,
        587.3295f,
        622.2540f,
        659.2551f,
        698.4565f,
        739.9888f,
        783.9909f,
        830.6094f,
        880.0000f, // A5
        932.3275f,
        987.7666f,
        1046.502f, // C6
        1108.731f,
        1174.659f,
        1244.508f,
        1318.510f,
        1396.913f,
        1479.978f,
        1567.982f,
        1661.219f,
        1760.000f, // A6
        1864.655f,
        1975.533f,
        2093.005f, // C7
        2217.461f,
        2349.318f,
        2489.016f,
        2637.020f,
        2793.826f,
        2959.955f,
        3135.963f,
        3322.438f,
        3520.000f, // A7
        3729.310f,
        3951.066f,
        4186.009f, // C8
    };

    public static readonly string[] labels = {
        "A0",
        "A#0",
        "B0",
        "C1", // octave #1
        "C#1",
        "D1",
        "D#1",
        "E1",
        "F1",
        "F#1",
        "G1",
        "G#1",
        "A1",
        "A#1",
        "B1",
        "C2", // octave #2
        "C#2",
        "D2",
        "D#2",
        "E2",
        "F2",
        "F#2",
        "G2",
        "G#2",
        "A2",
        "A#2",
        "B2",
        "C3", // octave #3
        "C#3",
        "D3",
        "D#3",
        "E3",
        "F3",
        "F#3",
        "G3",
        "G#3",
        "A3",
        "A#3",
        "B3",
        "C4", // octave #4
        "C#4",
        "D4",
        "D#4",
        "E4",
        "F4",
        "F#4",
        "G4",
        "G#4",
        "A4",
        "A#4",
        "B4",
        "C5", // octave #5
        "C#5",
        "D5",
        "D#5",
        "E5",
        "F5",
        "F#5",
        "G5",
        "G#5",
        "A5",
        "A#5",
        "B5",
        "C6", // octave #6
        "C#6",
        "D6",
        "D#6",
        "E6",
        "F6",
        "F#6",
        "G6",
        "G#6",
        "A6",
        "A#6",
        "B6",
        "C7", // octave #7
        "C#7",
        "D7",
        "D#7",
        "E7",
        "F7",
        "F#7",
        "G7",
        "G#7",
        "A7",
        "A#7",
        "B7",
        "C8", // octave #8
    };

    public static int searchNear(float val)
    {
        int min = 0;
        int max = keys.Length - 1;
        int index = -1;
        int indexTop = max;
        int indexBot = min;
        float delta = 0f;
        float deltaTop = 0f;
        float deltaBot = 0f;

        while (min <= max)
        {
            index = (min + max) / 2;
            if (val == keys[index])
            {
                // for very rare float equality...
                return index;
            }
            else if (val < keys[index])
            {
                max = index - 1;
                indexTop = index;
            }
            else
            {
                min = index + 1;
                indexBot = index;
            }

            // record delta for later comparison
            delta = Mathf.Abs(keys[index] - val);
        }

        // now find nearest
        deltaTop = Mathf.Abs(keys[indexTop] - val);
        deltaBot = Mathf.Abs(keys[indexBot] - val);

        // Debug.Log(indexTop + " top : bot " + indexBot);
        // Debug.Log(deltaTop + " deltaTop : deltaBot " + deltaBot);

        // now compare which index is nearest
        if (deltaTop < deltaBot)
        {
            return indexTop;
        }

        return indexBot;
    }

    public static string getLabelFromMIDI(int index)
    {
        if (index >= lowestMidi && index <= highestMidi)
        {
            return labels[index - lowestMidi];
        }
        return "";
    }
}
