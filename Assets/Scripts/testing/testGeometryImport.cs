using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriLibCore;

public class testGeometryImport : MonoBehaviour
{
    [SerializeField]
    bool import;

    [SerializeField]
    string path = "D:\\SeaDrive\\drude\\Shared with me\\dma-unity\\share\\MartinAssembly.fbx";

    [SerializeField]
    AssetLoaderOptions options;


    // Update is called once per frame
    void Update()
    {
        if (!import) return;
        import = false;

        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        //assetLoaderOptions.ReadEnabled = true;
        options = assetLoaderOptions;

        AssetLoader.LoadModelFromFile(path, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, null);
    }

    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // If you want to make sure the GameObject will be visible only when all Materials and Textures have been loaded, you can disable it at this step.
        var myLoadedGameObject = assetLoaderContext.RootGameObject;
    }

    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // You can make the GameObject visible again at this step if you prefer to.
        var go = assetLoaderContext.RootGameObject;
        go.SetActive(true);

        //StartCoroutine(WaitAndSetImportedMesh(10, go));
    }

    private void OnError(IContextualizedError contextualizedError)
    {
        XRMessageSystem.PrintWarning("Error while importing: " + contextualizedError.ToString());
    }

    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log("Progress: " + (progress * 100).ToString("F1") + "%");
        if (MarkerTrackingManager.DebugInfo)
            XRMessageSystem.PrintMessage("Progress: " + (progress * 100).ToString("F1") + "%");
    }
}
