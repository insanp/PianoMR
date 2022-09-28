using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizerFrequency : MonoBehaviour
{
    [SerializeField] public CubeKey cubeKeyPrefab;
    CubeKey[] cubes;

    private GameplayMic gMic;
    private int keyLength;

    // Start is called before the first frame update
    void Start()
    {
        keyLength = 1024;// HelperPianoFreq.keys.Length;
        cubes = new CubeKey[keyLength];

        for (int i = 0; i < keyLength; i++)
        {
            CubeKey cubeInstance = (CubeKey)Instantiate(cubeKeyPrefab);
            cubeInstance.transform.position = this.transform.position;
            cubeInstance.transform.parent = this.transform;
            cubeInstance.name = i.ToString();
            cubeInstance.transform.position = Vector3.right * 1.1f * i;
            cubeInstance.keyText.text = i.ToString();
            cubes[i] = cubeInstance;
        }

        /*
        for (int i = 0; i < keyLength; i++)
        {
            CubeKey cubeInstance = (CubeKey)Instantiate(cubeKeyPrefab);
            cubeInstance.transform.position = this.transform.position;
            cubeInstance.transform.parent = this.transform;
            cubeInstance.name = "Cube Key " + HelperPianoFreq.labels[i];
            cubeInstance.transform.position = Vector3.right * 1.1f * i;
            cubeInstance.keyText.text = HelperPianoFreq.labels[i];
            cubes[i] = cubeInstance;
        }*/

        gMic = FindObjectOfType<GameplayMic>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 1024; i++)
        {
            if (cubes != null)
            {
                cubes[i].val = gMic._samples[i];
            }
        }
    }
}
