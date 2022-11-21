using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;
namespace PianoTesisGameplay
{
    public class GameplayMusic : MonoBehaviour
    {
        public float speedMod = 0.2f;
        public static float Speed;
        public Camera Cam;
        public MidiFilePlayer midiFilePlayer;
        public MidiStreamPlayer midiStreamPlayer;
        public static Color ButtonColor = new Color(.7f, .9f, .7f, 1f);
        public GameplayNote NoteDisplay;
        public GameplayControl ControlDisplay;
        public GameObject Plane;

        [SerializeField] public Transform leftMostNote;
        [SerializeField] public Transform rightMostNote;
        [SerializeField] public GameplayValidationLine validationLine;
        [SerializeField] public Transform targetLine;
        [SerializeField] public Transform spawnLine;
        [SerializeField] public GameplayMic gMic;
        [SerializeField] public Transform thePianoParent;

        public Vector3 semitoneDistanceVector;
        public Vector3 spawnLineDistanceVector;
        public int numKeys = 88;

        public int lowestMidiValue = HelperPianoFreq.lowestMidi;
        public int highestMidiValue = HelperPianoFreq.highestMidi;

        public enum GameMode
        {
            NONE, PLAY, TRAIN, WATCH
        }

        public GameMode mode;

        public float minZ, maxZ, minX, maxX;
        public float LastTimeCollider;
        public float DelayCollider = 5;
        public float FirstDelayCollider = 20;
        public Material MatNewNote;
        public Material MatNewController;

        // Count gameobject for each z position in the plan. Useful to stack them.
        int[] countZ;

        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Samples: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Samples Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);
        }

        void Start()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Default size of a Unity Plan
            float planSize = 10f;
            Speed = speedMod;

            CalculateSemitoneDistance();

            minZ = Plane.transform.localPosition.z - Plane.transform.localScale.z * planSize / 2f;
            maxZ = Plane.transform.localPosition.z + Plane.transform.localScale.z * planSize / 2f;

            minX = Plane.transform.localPosition.x - Plane.transform.localScale.x * planSize / 2f;
            maxX = Plane.transform.localPosition.x + Plane.transform.localScale.x * planSize / 2f;

            if (midiFilePlayer != null)
            {
                // If call is already set from the inspector there is no need to set another listeneer
                if (!midiFilePlayer.OnEventNotesMidi.HasEvent())
                {
                    // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
                    Debug.Log("MusicView: no OnEventNotesMidi defined, set by script");
                    midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);
                }
            }
            else
                Debug.Log("MusicView: no MidiFilePlayer detected");

            // stop music at first
            PauseSpeed();
        }

        public void CalculateSemitoneDistance()
        {
            // must be divided by numKeys - 1 so that distance correctly placed from end to end
            semitoneDistanceVector = (rightMostNote.position - leftMostNote.position) / (numKeys - 1);
            spawnLineDistanceVector = spawnLine.position - targetLine.position;
            Debug.Log(spawnLineDistanceVector);
        }

        /// <summary>@brief
        /// Call when a group of midi events is ready to plays from the the midi reader.
        /// Playing the events are delayed until they "fall out"
        /// </summary>
        /// <param name="notes"></param>
        public void NotesToPlay(List<MPTKEvent> notes)
        {
            // Count gameobject for each z position in the plan. Useful to stack them.
            countZ = new int[Convert.ToInt32(maxZ - minZ) + 1];

            //Debug.Log(midiFilePlayer.MPTK_PlayTime.ToString() + " count:" + notes.Count);
            foreach (MPTKEvent mptkEvent in notes)
            {
                switch (mptkEvent.Command)
                {
                    case MPTKCommand.NoteOn:
                        //Debug.Log($"NoteOn Channel:{note.Channel}  Preset index:{midiStreamPlayer.MPTK_ChannelPresetGetIndex(note.Channel)}  Preset name:{midiStreamPlayer.MPTK_ChannelPresetGetName(note.Channel)}");
                        if (mptkEvent.Value >= lowestMidiValue && mptkEvent.Value <= highestMidiValue)// && note.Channel==1)
                        {
                            // x and z position is placed depending on the note and rotation
                            // y rotation is prohibited and start from spawn line
                            // spawn from the spawn line position

                            Vector3 position = new Vector3();
                            position = leftMostNote.transform.position + spawnLineDistanceVector;
                            position.x += (mptkEvent.Value - lowestMidiValue) * semitoneDistanceVector.x;
                            position.z += (mptkEvent.Value - lowestMidiValue) * semitoneDistanceVector.z;

                            // Instantiate a gamobject to represent this midi event in the 3D world
                            GameplayNote noteview = Instantiate<GameplayNote>(NoteDisplay, Plane.transform, false);
                            noteview.gameObject.SetActive(true);
                            noteview.hideFlags = HideFlags.HideInHierarchy;
                            noteview.midiStreamPlayer = midiStreamPlayer;
                            noteview.note = mptkEvent; // the midi event is attached to the gameobject, will be played more later
                            noteview.gameObject.GetComponent<Renderer>().material = MatNewNote;

                            noteview.transform.position = position;
                            noteview.transform.rotation = Quaternion.identity;
                            noteview.transform.localScale = NoteDisplay.transform.localScale;

                            noteview.targetVector = targetLine.position - spawnLine.position;

                            if (!GameplayNote.FirstNotePlayed)
                                PlaySound();
                        }
                        break;

                    case MPTKCommand.PatchChange:
                        {
                            /*
                            //Debug.Log($"PatchChange Channel:{note.Channel}  Preset index:{note.Value}");
                            // Z position is set depending the note value:mptkEvent.Value
                            float z = Mathf.Lerp(minZ, maxZ, mptkEvent.Value / 127f);
                            // Y position is set depending the count of objects at the z position
                            countZ[Convert.ToInt32(z - minZ)]++;
                            Vector3 position = new Vector3(maxX, 8f + countZ[Convert.ToInt32(z - minZ)] * 4f, z);
                            // Instanciate a gamobject to represent this midi event in the 3D world
                            GameplayControl patchview = Instantiate<GameplayControl>(ControlDisplay, position, Quaternion.identity);
                            patchview.gameObject.SetActive(true);
                            patchview.hideFlags = HideFlags.HideInHierarchy;
                            patchview.midiStreamPlayer = midiStreamPlayer;
                            patchview.note = mptkEvent; // the midi event is attached to the gameobjet, will be played more later
                            patchview.gameObject.GetComponent<Renderer>().material = MatNewController;
                            patchview.zOriginal = position.z;
                            */
                        }
                        break;
                }
            }
        }

        private void PlaySound()
        {
            // Some sound for waiting the notes, will be disbled at the fist note played ...
            //! [Example PlayNote]
            midiStreamPlayer.MPTK_PlayEvent
            (
                new MPTKEvent()
                {
                    Channel = 9,
                    Duration = 999999,
                    Value = 48,
                    Velocity = 100
                }
            );
            //! [Example PlayNote]
        }

        public void NextSong()
        {
            Clear();
            midiFilePlayer.MPTK_Next();
            PauseSpeed();
        }

        public void PrevSong()
        {
            Clear();
            midiFilePlayer.MPTK_Previous();
            PauseSpeed();
        }

        void OnGUI()
        {
            //return;

            int startx = 5;
            int starty = 90;
            int maxwidth = Screen.width;

            if (!HelperDemo.CheckSFExists()) return;

            if (midiFilePlayer != null)
            {
                GUILayout.BeginArea(new Rect(startx, starty, maxwidth, 200));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Previous", ""), GUILayout.Width(150)))
                {
                    PrevSong();
                }
                if (GUILayout.Button(new GUIContent("Next", ""), GUILayout.Width(150)))
                {
                    NextSong();
                }
                if (GUILayout.Button(new GUIContent("Clear", ""), GUILayout.Width(150)))
                    Clear();
                GUILayout.EndHorizontal();
                GUILayout.Label("Midi '" + midiFilePlayer.MPTK_MidiName + (midiFilePlayer.MPTK_IsPlaying ? "' is playing" : " is not playing"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("Midi Position :", GUILayout.Width(100));
                double currentposition = Math.Round(midiFilePlayer.MPTK_Position / 1000d, 2);
                double newposition = Math.Round(GUILayout.HorizontalSlider((float)currentposition, 0f, (float)midiFilePlayer.MPTK_DurationMS / 1000f, GUILayout.Width(200)), 2);
                if (newposition != currentposition)
                    midiFilePlayer.MPTK_Position = newposition * 1000d;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed Music :", GUILayout.Width(100));
                float speed = GUILayout.HorizontalSlider(midiFilePlayer.MPTK_Speed, 0.1f, 5f, GUILayout.Width(200));
                if (speed != midiFilePlayer.MPTK_Speed) midiFilePlayer.MPTK_Speed = speed;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed Note :", GUILayout.Width(100));
                Speed = GUILayout.HorizontalSlider(Speed, 5f, 20f, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Camera Y:", GUILayout.Width(100));
                float y = GUILayout.HorizontalSlider(Cam.transform.position.y, 50f, 150f, GUILayout.Width(200));
                if (y != Cam.transform.position.y)
                    Cam.transform.Translate(new Vector3(0, y - Cam.transform.position.y, 0), Space.World);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Camera X:", GUILayout.Width(100));
                float x = GUILayout.HorizontalSlider(Cam.transform.position.x, -50f, 50f, GUILayout.Width(200));
                if (x != Cam.transform.position.x)
                    Cam.transform.Translate(new Vector3(x - Cam.transform.position.x, 0, 0), Space.World);
                GUILayout.EndHorizontal();

                GUILayout.Label("Be careful with the notes traffic jam!!!");

                GUILayout.EndArea();
            }
        }

        /// <summary>@brief
        /// Remove all gameobject Note on the screen
        /// </summary>
        public void Clear()
        {
            GameplayNote[] components = GameObject.FindObjectsOfType<GameplayNote>();
            foreach (GameplayNote noteview in components)
            {
                if (noteview.enabled)
                    //Debug.Log("destroy " + ut.name);
                    DestroyImmediate(noteview.gameObject);
            }
        }

        void Update()
        {
            if (midiFilePlayer != null && midiFilePlayer.MPTK_IsPlaying)
            {
                // check game mode
                ExecuteGameMode();
                gMic.ClearPitchValues();
            }
        }

        private void ExecuteGameMode()
        {
            switch (mode)
            {
                case GameMode.PLAY:
                    break;
                case GameMode.TRAIN:
                    ValidateNote();
                    break;
                case GameMode.WATCH:
                default:
                    break;
            }
        }

        private void ValidateNote()
        {
            if (gMic.pitchValues.Count == 0) return;

            foreach (NotePeak notePeak in gMic.pitchValues)
            {
                foreach (GameplayNote gameNote in validationLine.notes)
                {
                    Debug.Log(HelperPianoFreq.getLabelFromMIDI(gameNote.note.Value));
                    // if any of the peaks the same, then the note is played
                    if (notePeak.key == HelperPianoFreq.getLabelFromMIDI(gameNote.note.Value))
                    {
                        // correct node and remove from validation list
                        gameNote.correctNote = true;
                        validationLine.notes.Remove(gameNote);
                    }
                }
            }
        }

        public void PauseSpeed()
        {
            midiFilePlayer.MPTK_Pause();
            Speed = 0f;
        }

        public void ResumeSpeed()
        {
            midiFilePlayer.MPTK_Play();
            Speed = speedMod;
        }

    }
}