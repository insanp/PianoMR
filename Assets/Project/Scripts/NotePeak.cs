using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePeak
{
    public string key;
    public float freq;
    public float val;

    public NotePeak(string key, float freq, float val)
    {
        this.key = key;
        this.freq = freq;
        this.val = val;
    }

    public void clear()
    {
        key = null;
        freq = 0f;
        val = 0f;
    }
}
