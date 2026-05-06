using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMaterialSetup : MonoBehaviour
{
    [SerializeField]
    ModelManager modelManager;

    [SerializeField]
    Slider slider;

    public static ChangeMaterialSetup Instance { get; private set; }
    

    private void Start()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (modelManager.UseSimpleMaterials)
            SetSlider(true);
        else
            SetSlider(false);
    }

    public void ChangeSimpleMaterialDisplay()
    {
        if (modelManager.UseSimpleMaterials)
        {
            SetSlider(false);
            modelManager.SetComplexMaterials();
        }
        else
        {
            SetSlider(true);
            modelManager.SetSimpleMaterials();
        }
    }

    public void SetSlider(bool value)
    {
        if (value)
            slider.value = 1;
        else
            slider.value = 0;
    }
}
