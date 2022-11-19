using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

namespace PianoTesisGameplay
{

    public class GameplayMic : MonoBehaviour
    {
        public bool useMic;
        private AudioSource _audioSource;
        public AudioMixerGroup mixerGroupMic, mixerGroupMaster;

        [SerializeField] public string defaultMic;
        [SerializeField] public TextMeshPro debugText;

        // FFT Values
        public int sampleRate; // for finer frequency with lower samples
        public int fftSize = 4096;
        public int pianoFreqSize;
        public float[] _samples;
        public float[] _freqBand;
        public float noiseLevel = 0.01f;
        public float time;

        private float freqStep;
        private float[] _bandBuffer = new float[512];
        private float[] _bufferDecrease = new float[512];
        private float[] _freqBandHighest = new float[8];

        public List<NotePeak> pitchValues = new List<NotePeak>();
        public String[] micValues;

        // Start is called before the first frame update
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            sampleRate = AudioSettings.outputSampleRate;
            pianoFreqSize = HelperPianoFreq.keys.Length;

            _samples = new float[fftSize];
            _freqBand = new float[pianoFreqSize];

            freqStep = sampleRate / _samples.Length;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_audioSource.clip != null)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }

            GetSpectrumAudioSource();
            //MakeFrequencyBands();
            //CheckMaxFrequencyFromSample();
            //AnalyzePitch();
            AnalyzeMultiPitch();
            ClearPitchValues();
        }

        public void StartMic()
        {

            if (useMic)
            {
                if (Microphone.devices.Length > 0)
                {
                    StartCoroutine(CaptureMic());
                }
                else
                {
                    useMic = false;
                }
            }
            else
            {
                _audioSource.outputAudioMixerGroup = mixerGroupMaster;
            }
        }

        public void StopMic()
        {
            StopCoroutine(CaptureMic());
            CleanMic();
        }

        IEnumerator CaptureMic()
        {
            micValues = Microphone.devices;
            defaultMic = micValues[0].ToString();
            _audioSource.outputAudioMixerGroup = mixerGroupMic;
            _audioSource.clip = Microphone.Start(defaultMic, true, 1, sampleRate);
            _audioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) {}
            _audioSource.Play();
            if (debugText != null)
            {
                debugText.text = string.Join(" ", micValues);
            }
            yield return null;
        }

        public void CleanMic()
        {
            defaultMic = micValues[0].ToString();
            _audioSource.clip = null;
            _audioSource.loop = false;
            Microphone.End(defaultMic);
        }

        private void GetSpectrumAudioSource()
        {
            _audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
        }

        private void AnalyzeMultiPitch()
        {
            float curVal = 0;
            var maxN = 0;
            int prevIndex = 0;
            float prevVal = 0f;
            bool passedPeak = false;
            float tmpFreq = 0f;
            string tmpLabel = "";
            NotePeak tmpNP;

            for (int i = 0; i < fftSize; i++)
            {
                // filter noise and insignificant harmonics
                if (_samples[i] < 0.01f)
                    continue;

                curVal = _samples[i];

                // check if it is a downward slope
                if (curVal < prevVal)
                {
                    if (!passedPeak)
                    {
                        // check key label
                        tmpFreq = EstimateFrequency(prevIndex);
                        tmpLabel = HelperPianoFreq.labels[HelperPianoFreq.searchNear(tmpFreq)];

                        // there is local peak before
                        // adds key value strength if any sample bin has the same value
                        tmpNP = pitchValues.Find(x => x.key == tmpLabel);

                        if (tmpNP != null)
                        {
                            tmpNP.val += prevVal;
                        }
                        else
                        {
                            pitchValues.Add(new NotePeak(tmpLabel, tmpFreq, prevVal));
                        }
                    }

                    passedPeak = true;
                }
                else
                {
                    passedPeak = false;
                }

                // next point
                prevIndex = i;
                prevVal = curVal;
            }

            //Debug.Log("====");
            foreach (NotePeak kvp in pitchValues)
            {
                Debug.Log("Key = " + kvp.key + " Freq = " + kvp.freq + " Value = " + kvp.val);
            }
        }

        public void ClearPitchValues()
        {
            pitchValues.Clear();
        }

        private float EstimateFrequency(int indexMax)
        {
            float freqN = indexMax; // pass the index to a float variable
            if (indexMax > 0 && indexMax < fftSize - 1)
            {
                // interpolate index using neighbours
                var dL = _samples[indexMax - 1] / _samples[indexMax]; //This line 1
                var dR = _samples[indexMax + 1] / _samples[indexMax]; //This line 2
                freqN += 0.5f * (dR * dR - dL * dL); //This line 3
            }
            float pitchValue = freqN * (sampleRate / 2) / fftSize; // convert index to frequency //This line 4
                                                                   //Debug.Log(indexMax + " = " + pitchValue);
                                                                   //Debug.Log(HelperPianoFreq.labels[HelperPianoFreq.searchNear(pitchValue)]);
            return pitchValue;
        }

        public void DetermineNoiseLevel()
        {
            StartCoroutine(CapturingNoise());
        }


        IEnumerator CapturingNoise()
        {
            Debug.Log("Determining noise");
            float captureTime = 3.0f;
            float timer = 0.0f;
            int numSample = 1;
            float vol = _samples.Max();

            while (timer <= captureTime)
            {
                vol += _samples.Max();
                timer += Time.deltaTime;
                numSample++;
                yield return null;
            }

            vol /= numSample;
            noiseLevel = vol;
            Debug.Log("Noise has max at " + noiseLevel);
        }

        private void AnalyzePitch()
        {
            float maxV = 0;
            var maxN = 0;
            for (int i = 0; i < fftSize; i++)
            {
                // find max and filter noise
                if (!(_samples[i] > maxV) || !(_samples[i] > 0.01f))
                    continue;

                maxV = _samples[i];
                maxN = i; // maxN is the index of max
            }
            float freqN = maxN; // pass the index to a float variable
            if (maxN > 0 && maxN < fftSize - 1)
            { // interpolate index using neighbours
                var dL = _samples[maxN - 1] / _samples[maxN]; //This line 1
                var dR = _samples[maxN + 1] / _samples[maxN]; //This line 2
                freqN += 0.5f * (dR * dR - dL * dL); //This line 3
            }
            float pitchValue = freqN * (sampleRate / 2) / fftSize; // convert index to frequency //This line 4
            Debug.Log(maxN + " = " + pitchValue);
            Debug.Log(HelperPianoFreq.labels[HelperPianoFreq.searchNear(pitchValue)]);
        }

        private void CheckMaxFrequencyFromBand()
        {
            float maxFreq = _freqBand.Max();

            if (maxFreq >= 0.5f)
            {
                //Debug.Log(HelperPianoFreq.labels[Array.IndexOf(_freqBand, maxFreq)]);
            }
        }

        private void CheckMaxFrequencyFromSample()
        {
            float maxFreq = _samples.Max();

            if (maxFreq >= 0.1f)
            {
                //Debug.Log(Array.IndexOf(_samples, maxFreq));
            }
        }
        private void MakeFrequencyBands()
        {
            /*
             * Example :
             * 
             *  (16384/2) / 4096 = 2 Hz per sample
             * 
             * find out band from sample :
             * key freq / (hz per sample) = sample bin
             * 
             * (sample bin floor + sample bin ceil) / 2 = freq band
             * 
             */

            float val;
            int tmpIndexSample;

            for (int i = 0; i < _freqBand.Length; i++)
            {
                val = 0;
                if (i < HelperPianoFreq.keys.Length)
                {
                    if (i == _freqBand.Length - 1)
                    {
                        tmpIndexSample = _samples.Length / 2;
                    }
                    else
                    {
                        // real FFT values is half of spectrum
                        tmpIndexSample = Mathf.FloorToInt(HelperPianoFreq.keys[i] / freqStep);
                    }


                    val = _samples[tmpIndexSample];

                    /*if (tmpIndexSample + 1 < _samples.Length)
                    {
                        val = (val + _samples[tmpIndexSample + 1]) / 2;
                    }*/
                }

                _freqBand[i] = val * 10;
            }
        }
    }
}