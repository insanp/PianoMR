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
            switch (gMusic.mode)
            {
                case GameplayMusic.GameMode.TRAIN:
                    if (notes.Count > 0 )
                    {
                        gMusic.PauseSpeed();
                    } else
                    {
                        gMusic.ResumeSpeed();
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Note")
            {
                GameplayNote tmp = collision.GetComponent<GameplayNote>();
                tmp.GetComponent<Renderer>().material.color = Color.blue;
                if (gMusic.mode == GameplayMusic.GameMode.WATCH)
                {
                    tmp.CorrectNote();
                }
                notes.Add(collision.gameObject.GetComponent<GameplayNote>());
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.tag == "Note")
            {
                notes.Remove(collision.gameObject.GetComponent<GameplayNote>());
            }
        }
    }
}