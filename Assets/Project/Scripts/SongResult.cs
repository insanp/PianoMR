using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PianoTesisGameplay
{
    public class SongResult
    {
        public string title;
        public int sampleRate;
        public int dspBufferSize;
        public int fftSize;
        public string fftWindow;
        public float noiseSize;
        public float minLatency;
        public float maxLatency;
        public int numLatencyArtifacts;
        public int totalNotes;
        public int totalHitNotes;
        public int totalMissNotes;
        public Dictionary<string, NotePlayStatistics> noteStats; 
    }

}