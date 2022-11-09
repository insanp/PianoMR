using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PianoTesisGameplay
{
    public class NoteGuideLine : MonoBehaviour
    {
        private LineRenderer lr;
        private GameplayNote note;

        // Start is called before the first frame update
        void Start()
        {
            lr = GetComponent<LineRenderer>();
            note = GetComponent<GameplayNote>();
            CreateGuideLine();
        }

        private void CreateGuideLine()
        {
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, this.transform.position + note.targetVector);
        }
    }

}
