using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;

public class MarkerTrackingManager : MonoBehaviour
{
    [SerializeField]
    Transform deviceOrigin;

    [SerializeField]
    ARTrackedImageManager trackedImageManager;

    [SerializeField]
    ModelManager modelManager;

    [SerializeField]
    List<XRMarker> markerInfoList = new List<XRMarker>();

    Dictionary<int, XRMarker> markerInfo = new Dictionary<int, XRMarker>();
    public Dictionary<int, XRMarker> MarkerInfo { get { return markerInfo; } }

    static bool verticalAlignment = true;
    public static bool VerticalAlignment { get { return verticalAlignment; } }

    public static bool DebugInfo { get; private set; }

    public static MarkerTrackingManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    public void ToggleVerticalAlignment()
    {
        verticalAlignment = !verticalAlignment;

        if (VerticalAlignment)
            XRMessageSystem.PrintMessage("Vertical Alignment");
        else
            XRMessageSystem.PrintMessage("No Vertical Alignment");
    }

    public void ToggleDebugInfo()
    {
        DebugInfo = !DebugInfo;

        if (DebugInfo)
            XRMessageSystem.PrintMessage("Debug Info will be printed");
        else
            XRMessageSystem.PrintMessage("No Debug Info");
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        XRMarker bestMarker = XRMarker.Empty;

        // Handle new markers
        foreach (var trackedImage in eventArgs.added)
        {
            int identifier = -1;
            try
            {
                identifier = int.Parse(trackedImage.referenceImage.name);
            }
            catch
            {
                if (!Application.isEditor)
                {
                    Debug.LogError("Identifier is not a number: " + trackedImage.referenceImage.name);
                    XRMessageSystem.PrintWarning("Identifier is not a number: " + trackedImage.referenceImage.name);
                    continue;
                }
                else
                {
                    identifier = EditorIdentifierSimulation.CheckEditorImage(trackedImage.name);
                }
            }

            Transform t = trackedImage.transform;

            if (!markerInfo.ContainsKey(identifier))
            {
                XRMessageSystem.PrintMessage("Marker_" + identifier + " identified.");

                XRMarker marker = new XRMarker(identifier, t, XRMarker.Empty);
                CheckMarkerAlignment(ref marker);
                markerInfo.Add(identifier, marker);

                if (bestMarker.isEmpty())
                    bestMarker = marker;
                else if (Vector3.Distance(deviceOrigin.position, marker.origin) < Vector3.Distance(deviceOrigin.position, bestMarker.origin))
                    bestMarker = marker;
            }
            else
            {
                XRMarker marker = new XRMarker(identifier, t, markerInfo[identifier]);
                CheckMarkerAlignment(ref marker);
                markerInfo[identifier] = marker;

                if (bestMarker.isEmpty())
                    bestMarker = marker;
                else if (Vector3.Distance(deviceOrigin.position, marker.origin) < Vector3.Distance(deviceOrigin.position, bestMarker.origin))
                    bestMarker = marker;
            }

        }

        // Handle updated markers
        foreach (var trackedImage in eventArgs.updated)
        {
            int identifier = -1;

            try
            {
                identifier = int.Parse(trackedImage.referenceImage.name);
            }
            catch
            {
                if (!Application.isEditor)
                {
                    Debug.LogError("Identifier is not a number: " + trackedImage.referenceImage.name);
                    XRMessageSystem.PrintWarning("Identifier is not a number: " + trackedImage.referenceImage.name);
                    continue;
                }
                else
                {
                    identifier = EditorIdentifierSimulation.CheckEditorImage(trackedImage.name);
                }
            }

            if (!markerInfo.ContainsKey(identifier))
            {
                Debug.LogError("Identifier Marker_" + identifier + " not available in Dict");
                XRMessageSystem.PrintWarning("Identifier Marker_" + identifier + " not available in Dict");
                continue;
            }

            Transform t = trackedImage.transform;

            XRMarker marker = new XRMarker(identifier, t, markerInfo[identifier]);
            CheckMarkerAlignment(ref marker);
            markerInfo[identifier] = marker;

            if (bestMarker.isEmpty())
                bestMarker = marker;
            else if (Vector3.Distance(deviceOrigin.position, marker.origin) < Vector3.Distance(deviceOrigin.position, bestMarker.origin))
                bestMarker = marker;
        }

        // Handle removed markers
        foreach (var trackedImage in eventArgs.removed)
        {
            
        }
        
        if (!bestMarker.isEmpty())
        {
            modelManager.AlignObjectToMarker(bestMarker);
        }        
    }

    void CheckMarkerAlignment(ref XRMarker marker)
    {
        foreach (KeyValuePair<int, XRMarker> pair in markerInfo)
        {
            if (pair.Value.identifier == marker.identifier) continue;

            if (Vector3.Angle(pair.Value.origin - marker.origin, marker.right) < 5 && Vector3.Angle(marker.right, Vector3.up) > 15 && Vector3.Angle(-marker.right, Vector3.up) > 15)
            {
                if (marker.rightNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + pair.Key + " and " + marker.identifier + " aligned across right axis");

                marker.SetRightNeighbour(pair.Key, 1);
                pair.Value.SetRightNeighbour(marker.identifier, -1);
            }
            else if (Vector3.Angle(pair.Value.origin - marker.origin, -marker.right) < 5)
            {
                if (marker.rightNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + pair.Key + " and " + marker.identifier + " aligned across right axis");

                marker.SetRightNeighbour(pair.Key, -1);
                pair.Value.SetRightNeighbour(marker.identifier, 1);
            }
            else
            {
                if (marker.rightNeighbour > 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + pair.Key + " and " + marker.identifier + " reset across right axis");

                marker.SetRightNeighbour(-1, 1);
                pair.Value.SetRightNeighbour(-1, 1);
            }

            if (Vector3.Angle(pair.Value.origin - marker.origin, marker.forward) < 5 && Vector3.Angle(marker.forward, Vector3.up) > 15 && Vector3.Angle(-marker.forward, Vector3.up) > 15)
            {
                if (marker.forwardNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + marker.identifier + " and " + pair.Key + " aligned across forward axis");

                marker.SetForwardNeighbour(pair.Key, 1);
                pair.Value.SetForwardNeighbour(marker.identifier, -1);
            }
            else if (Vector3.Angle(pair.Value.origin - marker.origin, -marker.forward) < 5)
            {
                if (marker.forwardNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + marker.identifier + " and " + pair.Key + " aligned across forward axis");

                marker.SetForwardNeighbour(pair.Key, -1);
                pair.Value.SetForwardNeighbour(marker.identifier, 1);
            }
            else
            {
                if (marker.forwardNeighbour > 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + pair.Key + " and " + marker.identifier + " reset across forward axis");

                marker.SetForwardNeighbour(-1, 1);
                pair.Value.SetForwardNeighbour(-1, 1);
            }

            if (Vector3.Angle(pair.Value.origin - marker.origin, marker.up) < 5 && Vector3.Angle(marker.up, Vector3.up) > 15 && Vector3.Angle(-marker.up, Vector3.up) > 15)
            {
                if (marker.upNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + marker.identifier + " and " + pair.Key + " aligned across up axis");

                marker.SetUpNeighbour(pair.Key, 1);
                pair.Value.SetUpNeighbour(marker.identifier, -1);
            }
            else if (Vector3.Angle(pair.Value.origin - marker.origin, -marker.up) < 5)
            {
                if (marker.upNeighbour < 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + marker.identifier + " and " + pair.Key + " aligned across up axis");

                marker.SetUpNeighbour(pair.Key, -1);
                pair.Value.SetUpNeighbour(marker.identifier, 1);
            }
            else
            {
                if (marker.upNeighbour > 0 && DebugInfo)
                    XRMessageSystem.PrintMessage("Markers " + pair.Key + " and " + marker.identifier + " reset across up axis");

                marker.SetUpNeighbour(-1, 1);
                pair.Value.SetUpNeighbour(-1, 1);
            }
        }
    }
}

[Serializable]
public struct XRMarker
{
    public Vector3 origin;
    public Vector3 right;
    public Vector3 forward;
    public Vector3 up;

    public int identifier;

    public Matrix4x4 transform;

    public int rightNeighbour;
    int rightDir;

    public int forwardNeighbour;
    int forwardDir;

    public int upNeighbour;
    int upDir;


    public XRMarker(int _identifier, Transform _transform, XRMarker oldMarker)
    {
        origin = _transform.position;
        right = _transform.right;
        forward = _transform.up;
        up = _transform.forward;

        identifier = _identifier;

        Quaternion rot = _transform.rotation;

        transform = Matrix4x4.TRS(_transform.position, rot, Vector3.one);

        rightNeighbour = oldMarker.rightNeighbour;
        forwardNeighbour = oldMarker.forwardNeighbour;
        upNeighbour = oldMarker.upNeighbour;

        rightDir = oldMarker.rightDir;
        forwardDir = oldMarker.forwardDir;
        upDir = oldMarker.upDir;
    }

    public bool AlignedMarkerVertically(out Matrix4x4 alignMatrix, out string message)
    {
        Vector3 forward = transform.GetColumn(2);
        Vector3 up = transform.GetColumn(1);
        Vector3 right = transform.GetColumn(0);

        Quaternion rot = transform.rotation;

        if (Vector3.Angle(forward, Vector3.up) < 15)
        {
            message = "Optimizing forward alignment vertically by " + Vector3.Angle(forward, Vector3.up).ToString("F2") + "°";
            
            rot = Quaternion.FromToRotation(forward, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else if (Vector3.Angle(up, Vector3.up) < 15)
        {
            message = "Optimizing up alignment vertically by " + Vector3.Angle(up, Vector3.up).ToString("F2") + "°";

            rot = Quaternion.FromToRotation(up, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else if (Vector3.Angle(right, Vector3.up) < 15)
        {
            message = "Optimizing right alignment vertically by " + Vector3.Angle(right, Vector3.up).ToString("F2") + "°";

            rot = Quaternion.FromToRotation(right, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else if (Vector3.Angle(-forward, Vector3.up) < 15)
        {
            message = "Optimizing backward alignment vertically by " + Vector3.Angle(-forward, Vector3.up).ToString("F2") + "°";

            rot = Quaternion.FromToRotation(-forward, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else if (Vector3.Angle(-up, Vector3.up) < 15)
        {
            message = "Optimizing down alignment vertically by " + Vector3.Angle(-up, Vector3.up).ToString("F2") + "°";

            rot = Quaternion.FromToRotation(-up, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else if (Vector3.Angle(-right, Vector3.up) < 15)
        {
            message = "Optimizing left alignment vertically by " + Vector3.Angle(-right, Vector3.up).ToString("F2") + "°";

            rot = Quaternion.FromToRotation(-right, Vector3.up) * rot;
            alignMatrix = Matrix4x4.TRS(transform.GetPosition(), rot, Vector3.one);

            return true;
        }
        else
        {
            message = "";

            alignMatrix = transform;
            return false;
        }
            
    }

    public bool AlignMarkerAlongAxis(Matrix4x4 verticalMatrix, out Matrix4x4 alignMatrix, out string message)
    {
        Matrix4x4 matrix = verticalMatrix;
        alignMatrix = matrix;
        message = "";

        Quaternion rot = matrix.rotation;

        if (rightNeighbour != -1)
        {
            Vector3 localRight = matrix.GetColumn(0);
            Vector3 alignedRight = rightDir * MarkerTrackingManager.Instance.MarkerInfo[rightNeighbour].origin - rightDir * matrix.GetPosition();

            rot = Quaternion.FromToRotation(localRight, alignedRight) * rot;
            alignMatrix = Matrix4x4.TRS(matrix.GetPosition(), rot, Vector3.one);

            message = "Marker aligned along right with " + rightNeighbour + " by " + Vector3.Angle(localRight, alignedRight) + "°";
            return true;
        }

        if (forwardNeighbour != -1)
        {
            Vector3 localForward = matrix.GetColumn(2);
            Vector3 alignedForward = forwardDir * MarkerTrackingManager.Instance.MarkerInfo[forwardNeighbour].origin - forwardDir * matrix.GetPosition();

            rot = Quaternion.FromToRotation(localForward, alignedForward) * rot;
            alignMatrix = Matrix4x4.TRS(matrix.GetPosition(), rot, Vector3.one);

            message = "Marker aligned along forward with " + forwardNeighbour + " by " + Vector3.Angle(localForward, alignedForward) + "°";
            return true;
        }

        if (upNeighbour != -1)
        {
            Vector3 localUp = matrix.GetColumn(1);
            Vector3 alignedUp = upDir * MarkerTrackingManager.Instance.MarkerInfo[upNeighbour].origin - upDir * matrix.GetPosition();

            rot = Quaternion.FromToRotation(localUp, alignedUp) * rot;
            alignMatrix = Matrix4x4.TRS(matrix.GetPosition(), rot, Vector3.one);

            message = "Marker aligned along up with " + upNeighbour + " by " + Vector3.Angle(localUp, alignedUp) + "°";
            return true;
        }

        return false;
    }

    public static XRMarker Empty
    {
        get
        {
            XRMarker marker = new XRMarker() {
                origin = Vector3.zero,
                right = Vector3.right,
                forward = Vector3.up,
                up = Vector3.forward,

                identifier = -1,

                transform = Matrix4x4.identity,

                rightNeighbour = -1,
                forwardNeighbour = -1,
                upNeighbour = -1,

                rightDir = 1,
                forwardDir = 1,
                upDir = 1
            };

            return marker;
        }
    }

    public bool isEmpty()
    {
        return identifier == -1;
    }

    public void SetRightNeighbour(int identifier, int dir)
    {
        rightNeighbour = identifier;
        rightDir = dir;
    }

    public void SetForwardNeighbour(int identifier, int dir)
    {
        forwardNeighbour = identifier;
        forwardDir = dir;
    }

    public void SetUpNeighbour(int identifier, int dir)
    {
        upNeighbour = identifier;
        upDir = dir;
    }
}