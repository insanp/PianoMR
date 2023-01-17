using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>@brief
/// Demo CatchMusic
/// </summary>
namespace PianoTesisGameplay
{
    
    /// <summary>@brief
    /// Defined behavior of a note
    /// </summary>
    public class GameplayNote : MonoBehaviour
    {
        public static bool FirstNotePlayed = false;
        public MPTKEvent note;
        public MidiStreamPlayer midiStreamPlayer;
        public bool played = false;
        public bool correctNote = false;
        public bool isSharp = false;
        public bool canSound = true;

        [SerializeField] public Material matBlackKey;
        [SerializeField] public Material matWhiteKey;
        [SerializeField] public Material matPlayed;

        public Vector3 targetVector;

        private void Start()
        {
            played = false;
            correctNote = false;
            if (MidiPlayerTK.HelperNoteLabel.IsSharp(note.Value))
            {
                isSharp = true;
                gameObject.GetComponent<Renderer>().material = matBlackKey;
            } else
            {
                isSharp = false;
                gameObject.GetComponent<Renderer>().material = matWhiteKey;
            }
        }

        // 
        /// <summary>@brief
        /// Update
        /// @code
        /// midiFilePlayer.MPTK_PlayNote(note);
        /// FirstNotePlayed = true;
        /// @endcode
        /// </summary>
        public void Update()
        {
            // The midi event is played with a MidiStreamPlayer when position X < -45 (falling)
            if (!played && correctNote) //transform.position.y < 10f)
            {
                played = true;

                // Now play the note with a MidiStreamPlayer prefab
                if (canSound) midiStreamPlayer.MPTK_PlayEvent(note);
                //! [Example PlayNote]
                FirstNotePlayed = true;

                gameObject.GetComponent<Renderer>().material = matPlayed;// .color = Color.red;
            }
            if (transform.position.y < -2f || correctNote)
            {
                Destroy(this.gameObject);
            }

        }
        void FixedUpdate()
        {
            // Move the note along all axis up to target line
            //float translation = Time.fixedDeltaTime * GameplayMusic.Speed;
            //transform.Translate(0, -translation, 0);
            Vector3 translation = targetVector * Time.fixedDeltaTime * GameplayMusic.Speed;
            transform.Translate(translation);
        }

        public void CorrectNote()
        {
            correctNote = true;
        }
    }
}