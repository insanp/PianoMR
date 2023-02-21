using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;
using System.IO;
using Newtonsoft.Json;
using System.Dynamic;

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

        public int totalNotes;
        public int totalHitNotes;
        public int totalMissNotes;
        public float score;
        private float thresholdLevelUp = 80f;
        public bool isPlaying;
        public bool hasLeveledUp;
        public PlayerData playerData;

        public Dictionary<string, NotePlayStatistics> notePlayStats;

        public enum GameMode
        {
            ANALYZE, PLAY, TRAIN, WATCH
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
            isPlaying = false;
            hasLeveledUp = false;

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
            InitializeNotePlayStats();
            InitializePlayerData();
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

                            if (mode == GameMode.PLAY || mode == GameMode.TRAIN || mode == GameMode.ANALYZE) noteview.canSoundOnHit = false;
                            if (mode == GameMode.ANALYZE) noteview.canAutoSoundOnValidationLine = true;

                            noteview.transform.position = position;
                            noteview.transform.rotation = Quaternion.identity;
                            noteview.transform.localScale = NoteDisplay.transform.localScale;

                            noteview.targetVector = targetLine.position - spawnLine.position;

                            AddTotalNote(HelperPianoFreq.getLabelFromMIDI(mptkEvent.Value));
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
            if (!CheckSongLevel()) midiFilePlayer.MPTK_Previous();
            PauseSpeed();
        }

        public void PrevSong()
        {
            Clear();
            midiFilePlayer.MPTK_Previous();
            if (!CheckSongLevel()) midiFilePlayer.MPTK_Next();
            PauseSpeed();
        }

        private bool CheckSongLevel()
        {
            var splitString = midiFilePlayer.MPTK_MidiName.Split(' ');
            if (splitString.Length > 0)
            {
                int level = Convert.ToInt32(splitString[1]);
                if (level <= playerData.playerLevel) return true;
            }
            return false;
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

            // destroy all notes in validation line
            validationLine.ClearNotes();
        }

        void Update()
        {
            if (midiFilePlayer != null && isPlaying)
            {
                // check game mode
                ExecuteGameMode();
                gMic.ClearPitchValues();

                if (totalNotes > 0 && totalNotes == (totalHitNotes + totalMissNotes))
                {
                    isPlaying = false;
                    ShowResults();
                }
            }
        }

        private void ShowResults()
        {
            StopSong();
            SaveLog();
            if (CheckLevelUp())
            {
                playerData.LevelUp();
                SaveLoadManager.SavePlayerData(playerData);
                hasLeveledUp = true;
            }
        }

        private void SaveLog()
        {
            string path = Application.persistentDataPath + "/" + midiFilePlayer.MPTK_MidiName +
                "_" + DateTimeOffset.Now.ToUnixTimeSeconds() + ".txt";

            StreamWriter writer = new(path, true);

            string data = CreateResultJSON();
            writer.WriteLine(data);
            writer.Close();

            Debug.Log(path);
        }

        private string CreateResultJSON()
        {
            SongResult result = new SongResult();
            result.title = midiFilePlayer.MPTK_MidiName;
            result.sampleRate = gMic.sampleRate;
            result.dspBufferSize = gMic.dspBufferSize;
            result.fftSize = gMic.fftSize;
            result.fftWindow = gMic.fftWindow.ToString();
            result.noiseSize = gMic.noiseLevel;
            result.minLatency = (float)GameplayMic.minLatency / (float)gMic.sampleRate;
            result.maxLatency = (float)GameplayMic.maxLatency / (float)gMic.sampleRate;
            result.numLatencyArtifacts = gMic.numArtifacts;
            result.totalNotes = totalNotes;
            result.totalHitNotes = totalHitNotes;
            result.totalMissNotes = totalMissNotes;
            result.noteStats = notePlayStats;

            return JsonConvert.SerializeObject(result);
        }

        private void ExecuteGameMode()
        {
            switch (mode)
            {
                case GameMode.ANALYZE:
                    ValidateNote();
                    break;
                case GameMode.PLAY:
                    ValidateNote();
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
                    // if any of the peaks the same, then the note is played
                    if (notePeak.key == HelperPianoFreq.getLabelFromMIDI(gameNote.note.Value))
                    {
                        // correct node and remove from validation list
                        gameNote.correctNote = true;
                        validationLine.notes.Remove(gameNote);
                        AddNoteHit(notePeak.key);
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

        public void StartSong(int val)
        {
            switch (val) {
                case 0:
                    mode = GameMode.ANALYZE;
                    gMic.StartMic();
                    gMic.DetermineNoiseLevel();
                    break;
                case 1:
                    mode = GameMode.PLAY;
                    gMic.StartMic();
                    gMic.DetermineNoiseLevel();
                    break;
                case 2:
                    mode = GameMode.TRAIN;
                    gMic.StartMic();
                    gMic.DetermineNoiseLevel();
                    break;
                case 3:
                default:
                    mode = GameMode.WATCH;
                    break;
            }
            
            ResetNotePlayStats();
            ResumeSpeed();
            isPlaying = true;
            hasLeveledUp = false;
        }

        public void StopSong()
        {
            PauseSpeed();
            Clear();
            midiFilePlayer.MPTK_Stop();
            isPlaying = false;
            if (mode != GameMode.WATCH) gMic.StopMic();
        }

        public void InitializeNotePlayStats()
        {
            notePlayStats = new Dictionary<string, NotePlayStatistics>();
            foreach (string label in HelperPianoFreq.labels)
            {
                notePlayStats.Add(label, new NotePlayStatistics(label));
            }
        }

        public void ResetNotePlayStats()
        {
            totalNotes = totalHitNotes = totalMissNotes = 0;
            foreach (KeyValuePair<string, NotePlayStatistics> entry in notePlayStats)
            {
                entry.Value.Reset();
            }
        }

        public void AddNoteHit(string label)
        {
            notePlayStats[label].AddHit();
            totalHitNotes++;
        }

        public void AddNoteMiss(string label)
        {
            notePlayStats[label].AddMiss();
            totalMissNotes++;
        }

        public void AddTotalNote(string label)
        {
            notePlayStats[label].AddTotal();
            totalNotes++;
        }

        private bool CheckLevelUp()
        {
            Debug.Log((float)totalHitNotes / (float)totalNotes * 100);
            score = (float)Math.Round((float)totalHitNotes / (float)totalNotes * 100, 0);
            Debug.Log(score);
            if (score >= thresholdLevelUp) return true;

            return false;
        }

        private void InitializePlayerData()
        {
            playerData = SaveLoadManager.LoadPlayerData();
            if (playerData == null)
            {
                playerData = new PlayerData();
                SaveLoadManager.SavePlayerData(playerData);
            }
        }

        public void ResetPlayerData()
        {
            playerData.Initialize();
            SaveLoadManager.SavePlayerData(playerData);
        }
    }
}