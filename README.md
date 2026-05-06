# TraeVis

An augmented reality application for mobile devices that lets you load 3D models and align them to physical printed image markers in real space.

Developed at Aarhus University.

## What it does

1. Point your phone camera at a printed AR image marker
2. Load a 3D model (FBX) by entering a URL in the app
3. The model snaps to the marker's position and orientation in the real world
4. Cycle through model layers and switch rendering modes for inspection

Models are authored in Rhino/Grasshopper and exported as FBX with marker metadata embedded in the geometry names.

## Requirements

- Unity **2022.3.19f1**
- Android (API level 30+) or iOS (12.0+)
- ARCore-capable Android device or LiDAR/TrueDepth iOS device

## Getting started

### 1. Clone and open

```
git clone <repo-url>
```

Open the project in Unity 2022.3.19f1. The Package Manager will resolve all open-source dependencies automatically.

### 2. Re-import proprietary packages

Two packages are excluded from the repository due to licensing restrictions. Import them manually before building:

| Package | Source |
|---|---|
| **TriLib 2** | Unity Asset Store |
| **OccaSoftware Wireframe Shader** | occasoftware.com |

Place TriLib into `Assets/TriLib/` and the Wireframe Shader into `Packages/com.occasoftware.wireframe-shader/`.

### 3. Build

Open **File → Build Settings**, select Android or iOS, and click Build.

## Preparing models

Models must be exported as FBX from Rhino. Marker objects embedded in the geometry are identified by their name using the pattern:

```
{identifier}_{size}
```

For example: `1_A4`, `2_A3`

- `identifier` — an integer matching the AR image reference library entry
- `size` — the physical paper size of the printed marker (used to infer real-world scale)

Geometry is organized into **layers**. The app cycles through layers for step-by-step inspection. The rendering mode (wireframe or flat color) switches automatically based on vertex count.

## License

This project is licensed under the [MIT License](LICENSE).

The following third-party assets are **not** covered by this license and are excluded from the repository:

- **TriLib 2** — Unity Asset Store EULA
- **OccaSoftware Wireframe Shader** — proprietary license
