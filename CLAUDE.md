# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**TraeVis** is a mobile AR application (Aarhus University) that lets users load 3D architectural/engineering models (FBX via URL) and spatially align them to physical printed image markers using ARCore/ARKit.

- Unity **2022.3.19f1**, Universal Render Pipeline
- Targets: Android (API 30+) and iOS (12.0+)
- Android package ID: `com.aarhusuniversity.arsequencing`

## Building

Open the project in Unity 2022.3.19f1. Builds are produced via **File → Build Settings**. Switch platform to Android or iOS, then Build. There are no CLI build scripts.

### Required after cloning

Two proprietary packages are excluded from the repo (see `.gitignore`). Re-import them before building:

- **TriLib 2** (`Assets/TriLib/`) — Unity Asset Store
- **OccaSoftware Wireframe Shader** (`Packages/com.occasoftware.wireframe-shader/`) — occasoftware.com

All other dependencies are resolved automatically by the Unity Package Manager from `Packages/manifest.json`.

## Architecture

### Core data flow

```
Camera → ARTrackedImageManager → MarkerTrackingManager
                                         ↓
User inputs URL → LoadModel (TriLib) → ModelManager → AlignObjectToMarker()
```

1. `MarkerTrackingManager` maintains a live dictionary of `XRMarker` structs — one per tracked image. Each struct stores world-space position, rotation axes, and neighbor-alignment state (which adjacent markers are co-planar on the right/forward/up axes).

2. `LoadModel` downloads an FBX from a URL via `UnityWebRequest`, saves it to `Application.persistentDataPath`, then passes it to TriLib's async importer.

3. `ModelManager` receives the imported GameObject hierarchy and:
   - Scans child objects named `"{id}_{size}"` (e.g. `"1_A4"`) to extract **RhinoMarker** structs — marker metadata baked into the geometry by Rhino/Grasshopper.
   - Organizes geometry by named **layers** for focus cycling (`FocusNext()` / `FocusPrev()`).
   - Calls `AlignObjectToMarker()` to compute a rigid transform from the model's best-matched RhinoMarker onto the corresponding live `XRMarker` pose.

### Material system

`ModelManager` selects one of two rendering modes, switching automatically if vertex count exceeds 1000:

| Mode | Trigger | Rendering |
|---|---|---|
| **Complex** | Low vertex count | OccaSoftware wireframe shader + focus highlight on active layer |
| **Simple** | >1000 verts or manual toggle | Flat color with opacity slider; focused layer is opaque, others semi-transparent |

Occlusion materials are applied to non-focused layers in both modes to prevent seeing through geometry.

### Key scripts (`Assets/Scripts/`)

| File | Responsibility |
|---|---|
| `MarkerTrackingManager.cs` | Singleton. Wraps `ARTrackedImageManager`; updates `XRMarker` dict; detects axis-aligned neighbor pairs; optional vertical-alignment correction. |
| `ModelManager.cs` | Singleton. Processes imported hierarchy; manages layer focus; switches material sets; calls alignment math. |
| `LoadModel.cs` | URL input → download coroutine → TriLib import → hands GameObject to `ModelManager`. |
| `UI/MessageSystem/XRMessageSystem.cs` | Singleton. Floating 4-message queue over AR view using TextMeshPro; `PrintMessage()` / `PrintWarning()`. |
| `UI/ChangeMaterialSetup.cs` | Bridges the material-mode UI slider to `ModelManager`. |

Scripts in `Assets/Scripts/testing/` are editor/debug utilities (not shipped): marker simulation, import smoke tests, repositioning logs.

### Scenes & Prefabs

Single scene: `Assets/Scenes/SampleScene.unity`.

Key prefabs:
- `Assets/Prefabs/CustomPlaneVisualizer.prefab` — AR plane mesh visualizer
- `Assets/Prefabs/UI/MessageSystem/MessageSystemMessage.prefab` — individual floating message card

### Marker naming convention

RhinoMarkers embedded in imported FBX geometry use the name pattern `"{identifier}_{size}"`:
- `identifier` — integer, matched against `ARTrackedImage` reference library GUIDs
- `size` — paper size string (e.g. `A4`), used to infer physical marker dimensions
