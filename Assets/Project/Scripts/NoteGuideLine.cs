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

        private Color cWhite = Color.white;
        private Color cBlack = new Color(0.4f, 0.2f, 0.2f, 1);

        // Start is called before the first frame update
        void Start()
        {
            lr = GetComponent<LineRenderer>();
            note = GetComponent<GameplayNote>();
            CreateGuideLine();
        }

        private void CreateGuideLine()
        {
            if (note.isSharp)
            {
                lr.startColor = lr.endColor = cBlack;
            } else
            {
                lr.startColor = lr.endColor = cWhite;
            }
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, this.transform.position + note.targetVector);
        }
    }

}
