using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using TriLibCore;
using TriLibCore.General;
using TMPro;
using System.Threading.Tasks;

public class LoadModel : MonoBehaviour
{
    [SerializeField]
    ModelManager modelManager;

    string fileUrl = "";
    string fileName = "model.fbx";

    private AssetLoaderContext loaderContext;



    public void LoadFile()
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
            File.Delete(Path.Combine(Application.persistentDataPath, fileName));

        StartCoroutine(DownloadFile(fileUrl, fileName));
    }

    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // If you want to make sure the GameObject will be visible only when all Materials and Textures have been loaded, you can disable it at this step.
        var myLoadedGameObject = assetLoaderContext.RootGameObject;
        myLoadedGameObject.SetActive(false);
    }

    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // You can make the GameObject visible again at this step if you prefer to.
        var go = assetLoaderContext.RootGameObject;
        go.SetActive(true);

        StartCoroutine(WaitAndSetImportedMesh(10, go));
    }

    private void OnError(IContextualizedError contextualizedError)
    {
        XRMessageSystem.PrintWarning("Error while importing: " + contextualizedError.ToString());
        Debug.LogError("Error while importing: " + contextualizedError.ToString());
    }

    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log("Progress: " + (progress * 100).ToString("F1") + "%");
        if (MarkerTrackingManager.DebugInfo)
            XRMessageSystem.PrintMessage("Progress: " + (progress * 100).ToString("F1") + "%");
    }

    void ImportModel()
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.ReadEnabled = true;
        assetLoaderOptions.OptimizeMeshes = false;
        assetLoaderOptions.MergeVertices = false;
        assetLoaderOptions.BufferizeFiles = FileBufferingMode.Disabled;
        //loaderContext = AssetLoader.LoadModelFromFileNoThread(Path.Combine(Application.persistentDataPath, fileName), OnError, null, assetLoaderOptions);
        //OnMaterialsLoad(loaderContext);

        loaderContext = AssetLoader.LoadModelFromFile(Path.Combine(Application.persistentDataPath, fileName), OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
    }

    public void DownloadModelFromURL(TMP_InputField inputField)
    {
        fileUrl = inputField.text;
        LoadFile();
    }

    IEnumerator DownloadFile(string url, string fileName)
    {
        // Create UnityWebRequest to get the file
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("File download error: " + request.error);
            XRMessageSystem.PrintWarning("File download error: " + request.error);
        }
        else
        {
            // Get the file bytes
            byte[] fileData = request.downloadHandler.data;

            // Save the file to persistentDataPath
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, fileData);

            Debug.Log("File downloaded and saved to: " + filePath);
            XRMessageSystem.PrintMessage("File downloaded and saved to: " + filePath);

            ImportModel();
        }
    }

    IEnumerator WaitAndSetImportedMesh(int frameCount, GameObject go)
    {
        for (int i = 0; i < frameCount; i++)
        {
            yield return null; // Waits for the next frame
        }

        modelManager.SetImportedMesh(go);
    }
}

