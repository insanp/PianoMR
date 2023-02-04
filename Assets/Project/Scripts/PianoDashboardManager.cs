using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PianoTesisGameplay
{
    public class PianoDashboardManager : MonoBehaviour
    {
        [SerializeField] GameObject callibrationToggle;
        [SerializeField] GameObject showKeysToggle;
        [SerializeField] GameObject thePiano;

        [SerializeField] GameObject dspBufferSizeRadial;
        [SerializeField] GameObject sampleRateRadial;
        [SerializeField] GameObject fftSizeRadial;

        [SerializeField] public TextMeshPro titleSong;
        [SerializeField] public TextMeshPro titleSongInMode;
        [SerializeField] public TextMeshPro titleSongInGameplay;

        [SerializeField] public TextMeshPro totalHit;
        [SerializeField] public TextMeshPro totalMiss;

        [SerializeField] public GameObject blackKeys;
        [SerializeField] public GameObject whiteKeys;

        private GameplayMusic gMusic;
        private GameplayMic gMic;

        // Start is called before the first frame update
        void Start()
        {
            gMusic = FindObjectOfType<GameplayMusic>();
            gMic = FindObjectOfType<GameplayMic>();
            ToggleMovePiano();
            ToggleShowKeys();
            UpdateTitleSong();
            InitializeSetupAudio();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTotalNotesPlay();
            //InitializeSetupAudio();
        }

        public void UpdateTitleSong()
        {
            titleSong.text = titleSongInMode.text =
                titleSongInGameplay.text = gMusic.midiFilePlayer.MPTK_MidiName;
        }

        public void UpdateTotalNotesPlay()
        {
            totalHit.text = gMusic.totalHitNotes.ToString();
            totalMiss.text = gMusic.totalMissNotes.ToString();
        }

        public void ToggleMovePiano()
        {
            if (callibrationToggle.GetComponent<Interactable>().IsToggled)
            {
                thePiano.GetComponent<ObjectManipulator>().enabled = true;
            } else
            {
                thePiano.GetComponent<ObjectManipulator>().enabled = false;
            }
        }

        public void ToggleShowKeys()
        {
            if (showKeysToggle.GetComponent<Interactable>().IsToggled)
            {
                blackKeys.SetActive(true);
                whiteKeys.SetActive(true);
            }
            else
            {
                blackKeys.SetActive(false);
                whiteKeys.SetActive(false);
            }
        }

        public void StopSong()
        {
            gMusic.StopSong();
        }

        public void InitializeSetupAudio()
        {
            // all hardcoded default to 256 DSP buffer size, 48000 Hz, 4096 FFT size
            dspBufferSizeRadial.GetComponent<InteractableToggleCollection>().SetSelection(1, true);
            sampleRateRadial.GetComponent<InteractableToggleCollection>().SetSelection(1, true);
            fftSizeRadial.GetComponent<InteractableToggleCollection>().SetSelection(1, true);
        }

        public void ApplySetupAudio()
        {
            gMic.dspBufferSize = HelperAudioSetting.dspBufferSizes[dspBufferSizeRadial.GetComponent<InteractableToggleCollection>().CurrentIndex];
            gMic.sampleRate = HelperAudioSetting.sampleRates[sampleRateRadial.GetComponent<InteractableToggleCollection>().CurrentIndex];
            gMic.fftSize = HelperAudioSetting.fftSizes[fftSizeRadial.GetComponent<InteractableToggleCollection>().CurrentIndex];
            gMic.SetupGlobalAudio();
        }
    }
}