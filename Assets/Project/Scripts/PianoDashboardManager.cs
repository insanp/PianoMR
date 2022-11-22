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
        [SerializeField] GameObject ThePiano;

        [SerializeField] public TextMeshPro titleSong;
        [SerializeField] public TextMeshPro titleSongInMode;
        [SerializeField] public TextMeshPro titleSongInGameplay;

        private GameplayMusic gMusic;

        // Start is called before the first frame update
        void Start()
        {
            gMusic = FindObjectOfType<GameplayMusic>();
            ToggleMovePiano();
            UpdateTitleSong();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void UpdateTitleSong()
        {
            titleSong.text = titleSongInMode.text =
                titleSongInGameplay.text = gMusic.midiFilePlayer.MPTK_MidiName;
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

        public void StopSong()
        {
            gMusic.StopSong();
        }
    }
}