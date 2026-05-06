using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRepositioning : MonoBehaviour
{
    Vector3 savePos;
    void Start()
    {
        savePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(savePos, transform.position) > 0.001f)
            XRMessageSystem.PrintMessage("Model repositioned by " + (Vector3.Distance(savePos, transform.position) * 1000).ToString("F1") + " mm");

        savePos = transform.position;
    }
}
