using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeKey : MonoBehaviour
{
    [SerializeField] public TextMeshPro keyText;
    [SerializeField] public Transform cubePivot;
    public float val = 0.0f;
    public float scaleMod = 100f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float newY = 1 + val * scaleMod;
        cubePivot.localScale = new Vector3(1, newY, 1);
    }
}
