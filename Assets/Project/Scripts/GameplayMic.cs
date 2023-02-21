using Microsoft.MixedReality.Toolkit.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace PianoTesisGameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class GameplayMic : MonoBehaviour
    {
        public bool useMic;
        public bool useMRTK;
        private AudioSource _audioSource;
        public AudioMixerGroup mixerGroupMic, mixerGroupMaster;
        private WindowsMicrophoneStream micStream = null;

        [SerializeField] public string defaultMic;

        // FFT Values
        [SerializeField] public int sampleRate = 48000; // for finer frequency with lower samples
        [SerializeField] public int fftSize = 4096; // 2048, 4096, 8192
        [SerializeField] public int pianoFreqSize;
        [SerializeField] public int dspBufferSize = 512;
        [SerializeField] public FFTWindow fftWindow = FFTWindow.BlackmanHarris; // defaults to BlackmanHarris

        public float[] _samples;
        public float[] _freqBand;
        public float noiseLevel = 0.0001f;
        public int numArtifacts = 0;
        public float time;

        private float freqStep;
        private float[] _bandBuffer = new float[512];
        private float[] _bufferDecrease = new float[512];
        private float[] _freqBandHighest = new float[8];

        // for calculating latency
        public static int readPosition = 0;
        public static int minLatency = 0;
        public static int maxLatency = 0;
        public static int prevWritePosition = 0;

        public List<NotePeak> pitchValues = new List<NotePeak>();
        public String[] micValues;

        // Start is called before the first frame update
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            pianoFreqSize = HelperPianoFreq.keys.Length;

            _samples = new float[fftSize];
            _freqBand = new float[pianoFreqSize];

            freqStep = sampleRate / _samples.Length;

            dspBufferSize = PlayerPrefs.GetInt("dspBufferSize", dspBufferSize);
            sampleRate = PlayerPrefs.GetInt("sampleRate", sampleRate);
            fftSize = PlayerPrefs.GetInt("fftSize", fftSize);
            fftWindow = HelperAudioSetting.fftWindows[PlayerPrefs.GetInt("fftWindow", 3)];

            //SetupGlobalAudio();
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
                GetSpectrumAudioSource();
                //MakeFrequencyBands();
                //CheckMaxFrequencyFromSample();
                //AnalyzePitch();
                AnalyzeMultiPitch();
            }
        }

        private void InitializeMicMRTK()
        {
            micStream = new WindowsMicrophoneStream();
            micStream.Uninitialize();

            if (micStream == null)
            {
                Debug.Log("Failed to create the Windows Microphone Stream object");
            }

            micStream.Gain = 1.0f;

            // Initialize the microphone stream.
            WindowsMicrophoneStreamErrorCode result = micStream.Initialize(WindowsMicrophoneStreamType.RoomCapture);
            if (result != WindowsMicrophoneStreamErrorCode.Success)
            {
                Debug.Log($"Failed to initialize the microphone stream. {result}");
                return;
            }
        }

        public void StartMic()
        {
            if (useMic)
            {
                //SetupGlobalAudio();
                if (useMRTK) StartMicMRTK(); else StartMicNormal();
                
            }
            else
            {
                _audioSource.outputAudioMixerGroup = mixerGroupMaster;
            }
        }

        private void StartMicNormal()
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

        private void StartMicMRTK()
        {
            InitializeMicMRTK();

            _audioSource.outputAudioMixerGroup = mixerGroupMic;

            // Start the microphone stream.
            // Do not keep the data and do not preview.
            WindowsMicrophoneStreamErrorCode result = micStream.StartStream(false, true);
            if (result != WindowsMicrophoneStreamErrorCode.Success)
            {
                Debug.Log($"Failed to start the microphone stream. {result}");
                useMic = false;
            }
            Debug.Log(result);
        }

        IEnumerator CaptureMic()
        {
            micValues = Microphone.devices;
            Debug.Log(string.Join(", ", micValues));
            defaultMic = micValues[0].ToString();
            _audioSource.outputAudioMixerGroup = mixerGroupMic;
            _audioSource.clip = Microphone.Start(defaultMic, true, 1, sampleRate);
            while (!(Microphone.GetPosition(defaultMic) > 0)) { }
            _audioSource.loop = true;
            _audioSource.PlayDelayed(0.001f);
            minLatency = 99999;
            maxLatency = 0;
            readPosition = 0;
            prevWritePosition = 0;
            numArtifacts = 0;
            yield return null;
        }

        public void StopMic()
        {
            if (useMRTK) StopMicMRTK(); else StopMicNormal();
        }

        private void StopMicNormal()
        {
            StopCoroutine(CaptureMic());
            StopAllCoroutines();
            CleanMic();
        }

        private void StopMicMRTK()
        {
            // Stop the microphone stream.
            WindowsMicrophoneStreamErrorCode result = micStream.StopStream();
            if (result != WindowsMicrophoneStreamErrorCode.Success)
            {
                Debug.Log($"Failed to stop the microphone stream. {result}");
            }

            // Uninitialize the microphone stream.
            micStream.Uninitialize();
            micStream = null;
        }

        public void CleanMic()
        {
            defaultMic = micValues[0].ToString();
            _audioSource.clip = null;
            _audioSource.loop = false;
            Microphone.End(defaultMic);
        }

        private void OnAudioFilterRead(float[] buffer, int numChannels)
        {
            if (useMRTK)
            {
                if (micStream == null) { return; }

                // Read the microphone stream data.
                WindowsMicrophoneStreamErrorCode result = micStream.ReadAudioFrame(buffer, numChannels);
                if (result != WindowsMicrophoneStreamErrorCode.Success)
                {
                    Debug.Log($"Failed to read the microphone stream data. {result}");
                }

                float sumOfValues = 0;

                // Calculate this frame's average amplitude.
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (float.IsNaN(buffer[i]))
                    {
                        buffer[i] = 0;
                    }

                    buffer[i] = Mathf.Clamp(buffer[i], -1.0f, 1.0f);
                    sumOfValues += Mathf.Clamp01(Mathf.Abs(buffer[i]));
                }

                Debug.Log(sumOfValues / buffer.Length);
            } else
            {
                // normal microphone, analyze latency
                readPosition += buffer.Length;
                int writePosition = Microphone.GetPosition(defaultMic);

                if (writePosition < prevWritePosition)    //Check if write buffer looped
                {
                    readPosition = buffer.Length;
                }
                // latency in samples
                int latency = readPosition - writePosition;
                if (latency > maxLatency)
                    maxLatency = latency;

                if (minLatency > latency)
                    minLatency = latency;

                if (latency < 0f) numArtifacts++;

                prevWritePosition = writePosition;
                //Debug.Log("Read: " + (float)readPosition + " Write: " + (float)writePosition);
            }
        }

        private void GetSpectrumAudioSource()
        {
            _audioSource.GetSpectrumData(_samples, 0, fftWindow);
        }

        private void AnalyzeMultiPitch()
        {
            float curVal = 0;
            int prevIndex = 0;
            float prevVal = 0f;
            bool passedPeak = false;
            float tmpFreq = 0f;
            string tmpLabel = "";
            NotePeak tmpNP;

            for (int i = 0; i < fftSize; i++)
            {
                // filter noise and insignificant harmonics
                if (_samples[i] < noiseLevel)
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

            /*
            Debug.Log("====");
            foreach (NotePeak kvp in pitchValues)
            {
                Debug.Log("Key = " + kvp.key + " Freq = " + kvp.freq + " Value = " + kvp.val);
            }*/
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
            float captureTime = 1.0f;
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

        public void SetupGlobalAudio()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.dspBufferSize = (int)dspBufferSize;  // 128, 256, 512
            config.sampleRate = sampleRate;        // 11025, 22050, 44100, 48000, 88200, 96000
            config.numVirtualVoices = 512;  // 1, 2, 4, 8, 16, 32, 50, 64, 100, 128, 256, 512
            config.numRealVoices = 32;      // 1, 2, 4, 8, 16, 32, 50, 64, 100, 128, 256, 512
            config.speakerMode = AudioSpeakerMode.Stereo;
            AudioSettings.Reset(config);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}