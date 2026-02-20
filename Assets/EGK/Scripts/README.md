# Evan Game Kits - Project Setup & Documentation

This documentation details the setup, configuration, and usage of the Evan Game Kits scripts found in this project.

## 1. Project Configuration

### Tags
The following **Tags** are required for the scripts to function correctly:
*   **`Player`**: Assign this tag to the main player object. Used by:
    *   `InteractionTrigger.cs` (for triggering events via 3D colliders).
    *   `M_AI_Chase.cs` (for AI target detection).
*   **`Ground`**: Assign this tag to floor/terrain objects. Used by:
    *   `Player.cs` (for basic ground collision logic).

### Layers
The following **LayerMasks** are frequently exposed in inspector fields and should be configured:
*   **Ground Layer**: Used by detection modules (`M_RaycastDetection`, `M_ColliderDetection`) to identify walkable surfaces.
*   **Obstacle Layer**: Used by AI modules (`M_AI_Sight`, `M_AI_Patrol`, `M_AI_CustomNavigator`) for obstacle avoidance and line-of-sight checks.

---

## 2. Core Systems

### GameCore
`GameCore.cs` is the central manager for the game loop and global events.
*   **Setup**: Create an empty GameObject in your scene and attach the `GameCore` script.
*   **Usage**: It uses the Singleton pattern (`GameCore.instance`).
*   **Events**: Configure the `UnityEvents` in the Inspector for:
    *   `OnSceneLoaded`
    *   `OnPause` / `OnResume`
    *   `OnRestart`
    *   `OnExit`
    *   `OnWin`

### LevelData
`LevelData.cs` is a `ScriptableObject`.
*   **Usage**: Create a new asset of type `LevelData` to store level configurations (e.g., scene names or level identifiers).

---

## 3. Entity System (Player & AI)

The entity system is built around the `Base` class, which handles common physics and input data.

### Player Setup
1.  **GameObject**: Create your player object.
2.  **Components**:
    *   Attach **`Player`** script (automatically adds `Rigidbody` and `PlayerInput`).
    *   Ensure **`Rigidbody`** settings are appropriate for your game (e.g., freeze rotation if needed).
    *   Configure **`PlayerInput`**:
        *   **Actions Asset**: Assign `PlayerControllerInput.inputactions`.
        *   **Default Map**: Set to `Player`.
3.  **Input Configuration**:
    *   **Move**: Vector2 (WASD, Arrow Keys, Left Stick).
    *   **Look**: Vector2 (Mouse Delta, Right Stick).
    *   **Hover**: 1D Axis (Q/E).
    *   **Jump**: Button (Space).
    *   **Run**: Button (Shift).

### AI Setup
AI scripts generally inherit from `Base` or use specific modules.
*   **Navigation**: AI scripts (`M_AI_Patrol`, `M_AI_Chase`) often require a `LayerMask` for obstacles to function correctly.
*   **Detection**: Ensure the AI has the appropriate detection modules attached if needed.

### Modules (Locomotion & Abilities)
The system uses a modular approach. Scripts in `Entity/Modules` (like `M_Walk`, `M_BasicJump`) often inherit from `Locomotion`.
*   **Dependency**: These modules usually require the `Base` (or `Player`/`AI`) component on the same GameObject to access `rb` (Rigidbody) and input data.
*   **Configuration**: Check individual module inspectors for specific settings (e.g., Speed, Jump Force, LayerMasks).

---

## 4. Game Mechanics

### Interaction Trigger
`InteractionTrigger.cs` handles interactive objects (buttons, levers, zones).
*   **Target**: Assign a `TransformStateTweener` component to the `TargetObject` field. This object will be affected when triggered.
*   **Trigger Types**:
    *   **`UIButton`**: Works with UI pointer events.
    *   **`Collider2D`**: Uses 2D physics triggers.
    *   **`Collider3D`**: Uses 3D physics triggers.
        *   *Note*: If `Collider3D` is selected, the script attempts to add a `PhysicsRaycaster` to the Main Camera if one is missing.
*   **Trigger State**:
    *   **`Switch`**: Toggles state on enter/click.
    *   **`Hold`**: Toggles on enter, toggles back on exit (release).

### Other Mechanics
*   **`USBFileFinder.cs`**: Likely for game-specific logic involving file searching.
*   **`WindowDragging.cs`**: Logic for dragging UI windows.

---

## 5. Directory Structure
*   **Core**: Essential managers (`GameCore`, `LevelData`).
*   **Entity**:
    *   **Base**: Base classes (`Base`, `Player`, `AI`).
    *   **Modules**: Pluggable abilities (`Locomotion`, `Jump`, `Fly`, etc.).
*   **Game Mechanic**: Gameplay specific scripts (`InteractionTrigger`, etc.).
*   **VFX**: Visual effects scripts (`A_Blast`).
*   **Editor**: Custom editor scripts.
