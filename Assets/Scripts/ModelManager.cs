using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using System.Linq;
using OccaSoftware.Wireframe.Runtime;
using UnityEngine.UI;


public class ModelManager : MonoBehaviour
{
    [SerializeField]
    ARTrackedImageManager trackedImageManager;

    [SerializeField]
    List<XRImageLibraryReference> availableLibraries;

    Dictionary<string, XRReferenceImageLibrary> libraries = new Dictionary<string, XRReferenceImageLibrary>();

    [SerializeField]
    List<GameObject> modelComponents = new List<GameObject>();

    Dictionary<int, RhinoMarker> markers = new Dictionary<int, RhinoMarker>();

    [SerializeField]
    List<RhinoMarker> showMarkers = new List<RhinoMarker>();

    [SerializeField]
    Material focusMaterial;

    [SerializeField]
    Material wireFrameMaterial;

    [SerializeField]
    Material occlusionMaterial;

    [SerializeField]
    bool useSimpleMaterials = false;
    public bool UseSimpleMaterials { get { return useSimpleMaterials; } }

    [SerializeField]
    Material simpleMaterial;

    [SerializeField]
    float focusOpacity = 0.7f;

    [SerializeField]
    float simpleUnfocusOpacity = 0.4f;

    [SerializeField]
    Slider unfocusSlider;

    GameObject objectBase = null;

    int highlightIndex = 0;

    string size;

    private void Start()
    {
        foreach (var lib in availableLibraries)
        {
            libraries.Add(lib.identifier, lib.library);
        }
    }

    public void SetImportedMesh(GameObject go)
    {
        if (objectBase != null)
        {
            markers.Clear();
            modelComponents.Clear();
            Destroy(objectBase);
        }

        List<int> sortIndeces = new List<int>();

        objectBase = go;
        //objectBase.AddComponent<TestRepositioning>();



        for (int i = 0; i < go.transform.childCount; ++i)
        {
            GameObject layer = go.transform.GetChild(i).gameObject;
            if (layer.name == "Markers")
            {
                for (int j = 0; j < layer.transform.childCount; ++j)
                {
                    RhinoMarker mark = new RhinoMarker(layer.transform.GetChild(j).gameObject);
                    markers.Add(mark.identifier, mark);
                    showMarkers.Add(mark);

                    GameObject placeholder = new GameObject();
                    placeholder.name = "MarkerPlaceholder_" + mark.identifier;
                    placeholder.transform.position = mark.transform.GetPosition();
                    placeholder.transform.rotation = mark.transform.rotation;
                    layer.transform.GetChild(j).gameObject.SetActive(false);                }
            }
            else
            {
                foreach (var mesh in layer.GetComponentsInChildren<MeshFilter>())
                {
                    GameObject parent = mesh.transform.parent.gameObject;
                    string name = parent.name;
                    string[] parts = name.Split('|');
                    int index = 0;

                    if (parts.Length > 1)
                    {
                        try
                        {
                            index = int.Parse(parts[0]);
                            sortIndeces.Add(index);
                        }
                        catch
                        {
                            Debug.LogError("Layer naming incorrect. Ordering won't work");
                            XRMessageSystem.PrintWarning("Layer naming incorrect. Ordering won't work");
                        }
                    }

                    modelComponents.Add(parent);
                }
            }
        }

        if (sortIndeces.Count == modelComponents.Count)
        {
            GameObject[] reorderComponents = modelComponents.ToArray();
            int[] order = sortIndeces.ToArray();

            System.Array.Sort(order, reorderComponents);
            modelComponents = reorderComponents.ToList<GameObject>();
        }
        else
        {
            Debug.LogError("Sorting didn't work");
            XRMessageSystem.PrintWarning("Sorting didn't work");
        }

        try
        {
            List<string> sizes = new List<string>();
            foreach (var marker in markers.Values)
            {
                sizes.Add(marker.size);
            }

            size = sizes[0];

            foreach (string s in sizes)
            {
                if (s != size)
                {
                    XRMessageSystem.PrintWarning("More than one Library in System");
                }
            }

            trackedImageManager.referenceLibrary = libraries[size];
        }
        catch
        {
            Debug.LogError("Library not found for marker");
            Debug.LogError(libraries.Count + " libraries available");
            XRMessageSystem.PrintWarning("Library not found for marker");
        }

        if (GetModelVertexCountWorstCase(go) > 1000 || useSimpleMaterials)
        {
            SetSimpleMaterials();

            if (ChangeMaterialSetup.Instance != null)
                ChangeMaterialSetup.Instance.SetSlider(true);

            Debug.Log("High model complexity detected. Using simple Materials");
            XRMessageSystem.PrintMessage("High model complexity detected. Using simple Materials");
        }
        else
            SetComplexMaterials();

        XRMessageSystem.PrintMessage("Model loaded with Markerset " + size);
    }

    public void SetSimpleMaterials()
    {
        useSimpleMaterials = true;

        unfocusSlider.interactable = true;

        if (objectBase == null) return;

        foreach (var renderer in objectBase.GetComponentsInChildren<MeshRenderer>(true))
        {
            Color color = Color.white;

            if (renderer.material.HasProperty("_BaseColor"))
                color = renderer.material.GetColor("_BaseColor");
            else if (renderer.material.HasProperty("_Emission"))
                color = renderer.material.GetColor("_Emission");
            else
                color = new Color(0.8f, 0.8f, 0.8f);
            
            if (renderer.GetComponent<MakeWireframe>() != null)
                Destroy(renderer.GetComponent<MakeWireframe>());

            Material simpleMatInstance = Instantiate(simpleMaterial);
            simpleMatInstance.SetColor("_BaseColor", color);

            renderer.materials = new Material[2]
            {
                simpleMatInstance,
                occlusionMaterial
            };

        }

        SetMaterialOpacity();
    }

    public void SetComplexMaterials()
    {
        useSimpleMaterials = false;
        
        unfocusSlider.interactable = false;

        if (objectBase == null) return;


        foreach (var renderer in objectBase.GetComponentsInChildren<MeshRenderer>(true))
        {
            Color color = renderer.material.GetColor("_BaseColor");

            Material wireframeInstance = Instantiate(wireFrameMaterial);
            wireframeInstance.SetColor("_Emission", color);

            renderer.materials = new Material[2]{
            wireframeInstance,
            occlusionMaterial };

            var wireframe = renderer.gameObject.AddComponent<MakeWireframe>();
            wireframe.preferQuads = false;

        }

        SetMaterialOpacity();
    }

    public void FocusNext()
    {
        if (modelComponents == null || modelComponents.Count == 0)
            return;

        ++highlightIndex;

        if (highlightIndex >= modelComponents.Count)
            highlightIndex = 0;

        SetMaterialOpacity();
    }

    public void FocusPrev()
    {
        if (modelComponents == null || modelComponents.Count == 0)
            return;

        --highlightIndex;

        if (highlightIndex < 0)
            highlightIndex = modelComponents.Count - 1;

        SetMaterialOpacity();

    }

    void SetMaterialOpacity()
    {
        if (!useSimpleMaterials)
            SetMaterialOpacityComplex();
        else
            SetMaterialOpacitySimple();
    }

    void SetMaterialOpacityComplex()
    {
        if (modelComponents == null || modelComponents.Count == 0)
            return;


        foreach (GameObject go in modelComponents)
        {
            var unfocusRenderer = go.GetComponentInChildren<MeshRenderer>();

            SetOccludedMaterial(unfocusRenderer);
        }

        var focusRenderer = modelComponents[highlightIndex].GetComponentInChildren<MeshRenderer>();
        SetFocusMaterial(focusRenderer);

        foreach (GameObject go in modelComponents)
        {
            go.SetActive(false);
        }

        for (int i = 0; i <= highlightIndex; ++i)
        {
            modelComponents[i].SetActive(true);
            var wireframe = modelComponents[i].GetComponentInChildren<MakeWireframe>();

            if (wireframe != null)
            {
                GameObject wireframeGO = wireframe.gameObject;

                Destroy(wireframe);
                wireframe = wireframeGO.AddComponent<MakeWireframe>();
                wireframe.preferQuads = false;
            }
        }
    }

    void SetMaterialOpacitySimple()
    {
        if (modelComponents == null || modelComponents.Count == 0)
            return;


        foreach (GameObject go in modelComponents)
        {
            var unfocusRenderer = go.GetComponentInChildren<MeshRenderer>();

            Color col1 = unfocusRenderer.material.GetColor("_BaseColor");
            col1.a = simpleUnfocusOpacity;

            unfocusRenderer.material.SetColor("_BaseColor", col1);
        }

        var focusRenderer = modelComponents[highlightIndex].GetComponentInChildren<MeshRenderer>();

        Color col2 = focusRenderer.material.GetColor("_BaseColor");
        col2.a = focusOpacity;

        focusRenderer.material.SetColor("_BaseColor", col2);

        foreach (GameObject go in modelComponents)
        {
            go.SetActive(false);
        }

        for (int i = 0; i <= highlightIndex; ++i)
        {
            modelComponents[i].SetActive(true);
        }
    }

    private void SetOccludedMaterial(MeshRenderer renderer)
    {
        var mats = new Material[2]
        {
            renderer.materials[0],
            occlusionMaterial
        };

        renderer.materials = mats;
    }

    private void SetFocusMaterial(MeshRenderer renderer)
    {
        Color color = renderer.materials[0].GetColor("_Emission");
        Color alphaColor = focusMaterial.GetColor("_BaseColor");

        color.a = focusOpacity;
        focusMaterial.SetColor("_BaseColor", color);

        var mats = new Material[3]
        {
            renderer.materials[0],
            focusMaterial,
            occlusionMaterial
        };
        renderer.materials = mats;
    }

    public void SetFocusOpacityValue(float value)
    {
        focusOpacity = value;

        if (modelComponents == null || modelComponents.Count == 0) return;

        int materialIndex = 1;
        if (useSimpleMaterials) materialIndex = 0;

        var renderer = modelComponents[highlightIndex].GetComponentInChildren<MeshRenderer>();
        var col = renderer.materials[materialIndex].GetColor("_BaseColor");
        col.a = focusOpacity;
        renderer.materials[materialIndex].SetColor("_BaseColor", col);
    }

    public void SetUnfocusOpacityValue(float value)
    {
        simpleUnfocusOpacity = value;

        if (modelComponents == null || modelComponents.Count == 0) return;
        if (!useSimpleMaterials) return;

        foreach (GameObject go in modelComponents)
        {
            var renderer = go.GetComponentInChildren<MeshRenderer>();
            var col = renderer.material.GetColor("_BaseColor");
            col.a = simpleUnfocusOpacity;
            renderer.material.SetColor("_BaseColor", col);
        }

        SetFocusOpacityValue(focusOpacity);
    }

    public void AlignObjectToMarker(XRMarker xrMarker)
    {
        if (objectBase == null) return;

        int identifier = xrMarker.identifier;
        if (!markers.ContainsKey(identifier))
        {
            XRMessageSystem.PrintWarning("Marker_" + identifier + " is not contained in the model");
        }

        RhinoMarker rhinoMarker = markers[identifier];

        Matrix4x4 orientMatrix = Matrix4x4.identity;

        if (!MarkerTrackingManager.VerticalAlignment)
            orientMatrix = CreateOrientMatrix(rhinoMarker.origin, rhinoMarker.transform.rotation, xrMarker.origin, xrMarker.transform.rotation);
        else
        {
            Matrix4x4 alignMatrix1 = Matrix4x4.identity;
            Matrix4x4 alignMatrix2 = Matrix4x4.identity;
            string message1 = "";
            string message2 = "";

            bool alignedVertically = xrMarker.AlignedMarkerVertically(out alignMatrix1, out message1);
            bool alignedToNeighbour = xrMarker.AlignMarkerAlongAxis(alignMatrix1, out alignMatrix2, out message2);


            orientMatrix = CreateOrientMatrix(rhinoMarker.origin, rhinoMarker.transform.rotation, alignMatrix2.GetPosition(), alignMatrix2.rotation);
            
            if (alignedVertically && MarkerTrackingManager.DebugInfo)
                XRMessageSystem.PrintMessage(message1);
            if (alignedToNeighbour && MarkerTrackingManager.DebugInfo)
                XRMessageSystem.PrintMessage(message2);
        }

        objectBase.transform.rotation = orientMatrix.rotation;
        objectBase.transform.position = orientMatrix.GetPosition();
    }

    Matrix4x4 CreateOrientMatrix(Vector3 sourcePos, Quaternion sourceRot, Vector3 targetPos, Quaternion targetRot)
    {
        //Rotate target to correct alignment
        targetRot = Quaternion.AngleAxis(90, targetRot * Vector3.right) * targetRot;

        // Calculate rotation (align source rotation to target rotation)
        Quaternion rotation = targetRot * Quaternion.Inverse(sourceRot); // Inverse rotation to map the source orientation to the target

        //Rotate source position and translate
        Vector3 rotatedSource = rotation * sourcePos;
        Vector3 translation = targetPos - rotatedSource;

        // Construct the transformation matrix
        Matrix4x4 matrix = Matrix4x4.TRS(translation, rotation, Vector3.one);
        return matrix;
    }

    int GetModelVertexCountWorstCase(GameObject parent)
    {
        int vertexCount = 0;
        foreach (MeshFilter mf in parent.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh.vertexCount > vertexCount)
                vertexCount = mf.sharedMesh.vertexCount;
        }

        return vertexCount;
    }
}

[System.Serializable]
public struct RhinoMarker
{
    public Vector3 origin;
    public Vector3 xAxis;
    public Vector3 yAxis;
    public Vector3 zAxis;
    public string size;
    public int identifier;
    public Matrix4x4 transform;

    public RhinoMarker(GameObject go)
    {
        identifier = int.Parse(go.name.Split('_')[0]);
        size = go.name.Split('_')[1];

        Mesh m = go.GetComponentInChildren<MeshFilter>().sharedMesh;
        Vector3 pos = go.transform.position;
        Quaternion rot = go.transform.rotation;

        origin = rot * m.vertices[2] + pos;
        xAxis = (rot * m.vertices[1] - rot * m.vertices[2]).normalized;
        yAxis = (rot * m.vertices[0] - rot * m.vertices[2]).normalized;
        zAxis = Vector3.Cross(xAxis, yAxis).normalized;

        Quaternion lookRot = Quaternion.LookRotation(zAxis, yAxis);

        transform = Matrix4x4.TRS(origin, lookRot, Vector3.one);

    }
}

[System.Serializable]
public struct XRImageLibraryReference
{
    public XRReferenceImageLibrary library;
    public string identifier;
}