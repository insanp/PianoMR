using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PianoTesisGameplay
{
    public class PianoDashboardManager : MonoBehaviour
    {
        [SerializeField] GameObject CallibrationToggle;
        [SerializeField] GameObject ShowKeysToggle;
        [SerializeField] GameObject ThePiano;

        [SerializeField] public TextMeshPro titleSong;
        [SerializeField] public TextMeshPro titleSongInMode;
        [SerializeField] public TextMeshPro titleSongInGameplay;

        [SerializeField] public TextMeshPro totalHit;
        [SerializeField] public TextMeshPro totalMiss;

        [SerializeField] public GameObject blackKeys;
        [SerializeField] public GameObject whiteKeys;

        private GameplayMusic gMusic;

        // Start is called before the first frame update
        void Start()
        {
            gMusic = FindObjectOfType<GameplayMusic>();
            ToggleMovePiano();
            ToggleShowKeys();
            UpdateTitleSong();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTotalNotesPlay();
        }

        public void UpdateTitleSong()
        {
            titleSong.text = titleSongInMode.text =
                titleSongInGameplay.text = gMusic.midiFilePlayer.MPTK_MidiName;
        }

        public void UpdateTotalNotesPlay()
        {
            if (gMusic.midiFilePlayer.MPTK_IsPlaying)
            {
                totalHit.text = gMusic.totalHitNotes.ToString();
                totalMiss.text = gMusic.totalMissNotes.ToString();
            }
        }

        public void ToggleMovePiano()
        {
            if (CallibrationToggle.GetComponent<Interactable>().IsToggled)
            {
                ThePiano.GetComponent<ObjectManipulator>().enabled = true;
            } else
            {
                ThePiano.GetComponent<ObjectManipulator>().enabled = false;
            }
        }

        public void ToggleShowKeys()
        {
            if (ShowKeysToggle.GetComponent<Interactable>().IsToggled)
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
    }
}