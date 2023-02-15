using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PianoTesisGameplay {
    public class DebugDelayAnalyzeLine : MonoBehaviour
    {
        private GameplayValidationLine gVL;

        // Start is called before the first frame update
        void Start()
        {
            gVL = FindObjectOfType<GameplayValidationLine>();
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Note")
            {
                GameplayNote note = collision.GetComponent<GameplayNote>();

                // auto sound from midi stream playback for analyzing
                if (gVL.gMusic.mode == GameplayMusic.GameMode.ANALYZE)
                {
                    note.ForcePlayNote();
                }
            }
        }

        public void UpdatePositionZ(GameObject slider)
        {
            float val = slider.GetComponent<PinchSlider>().SliderValue;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y,
                -0.1f * val);
            Debug.Log(this.transform.localPosition);
        }
    }
}

