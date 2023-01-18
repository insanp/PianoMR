using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePlayStatistics
{
    public string key;
    public int total;
    public int totalHit;
    public int totalMiss;

    public NotePlayStatistics(string key)
    {
        this.key = key;
        total = totalHit = totalMiss = 0;
    }

    public void AddHit()
    {
        totalHit++;
    }

    public void AddMiss()
    {
        totalMiss++;
    }

    public void AddTotal()
    {
        total++;
    }

    public void Reset()
    {
        total = totalHit = totalMiss = 0;
    }
}
