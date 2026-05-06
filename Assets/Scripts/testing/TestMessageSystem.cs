using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMessageSystem : MonoBehaviour
{
    float timer = 0;

    [SerializeField]
    float interval = 1;

    [SerializeField]
    string message = "testing";

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > interval)
        {
            timer = 0;
            XRMessageSystem.PrintMessage(message);
        }
    }
}
