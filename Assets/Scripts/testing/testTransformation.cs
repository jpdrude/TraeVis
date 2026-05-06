using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testTransformation : MonoBehaviour
{
    [SerializeField]
    bool setTransform = false;

    [SerializeField]
    ModelManager modelManager;

    [SerializeField]
    int identifier;

    // Update is called once per frame
    void Update()
    {
        if (!setTransform) return;

        XRMarker testMarker = new XRMarker(identifier, transform, XRMarker.Empty);

        modelManager.AlignObjectToMarker(testMarker);

        setTransform = false;
    }
}
