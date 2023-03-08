using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PianoTesisGameplay {
    public class GameplayValidationLine : MonoBehaviour
    {
        public List<GameplayNote> notes;
        public GameplayMusic gMusic;

        // Start is called before the first frame update
        void Start()
        {
            notes = new List<GameplayNote>();
            gMusic = FindObjectOfType<GameplayMusic>();
        }

        private void Update()
        {
            if (gMusic.isPlaying)
            {
                switch (gMusic.mode)
                {
                    case GameplayMusic.GameMode.TRAIN:
                        if (notes.Count > 0 && gMusic.onTrainPause)
                        {
                            gMusic.PauseSpeed();
                            CheckNoteCountdown();
                        }
                        else
                        {
                            gMusic.onTrainPause = false;
                            gMusic.ResumeSpeed();
                        }
                        break;
                }
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Note")
            {
                GameplayNote note = collision.GetComponent<GameplayNote>();
                note.GetComponent<Renderer>().material.color = Color.red;
                
                if (gMusic.mode == GameplayMusic.GameMode.WATCH)
                {
                    note.CorrectNote();
                    gMusic.AddNoteHit(HelperPianoFreq.getLabelFromMIDI(note.note.Value));
                } else
                {
                    notes.Add(note);
                }

                if (gMusic.mode == GameplayMusic.GameMode.TRAIN)
                {
                    note.isBeingPaused = true;
                    gMusic.onTrainPause = true;
                }
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.tag == "Note")
            {
                GameplayNote note = collision.GetComponent<GameplayNote>();
                gMusic.AddNoteMiss(HelperPianoFreq.getLabelFromMIDI(note.note.Value));
                notes.Remove(note);
                Destroy(collision.gameObject);
            }
        }

        public void ClearNotes()
        {
            notes.Clear();
        }

        private void CheckNoteCountdown()
        {
            bool check = false;
            for (var i = 0; i < notes.Count; i++)
            {
                check = check || notes[i].isBeingPaused;
                if (check) return;
            }
            gMusic.onTrainPause = false;
        }
    }
}