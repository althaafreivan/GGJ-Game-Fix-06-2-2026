# Unique "Meta" Mechanics

The project contains specialized scripts that interact directly with the Windows Operating System via `user32.dll`.

## 1. Window Manipulation (`ScreenResize.cs`)
This script allows the game to resize and move its own window programmatically.
- **Functionality**:
    - Removes window borders and captions (making it look like a splash screen or custom UI).
    - Smoothly interpolates window size using `Vector2.SmoothDamp`.
    - Forces the window to be "Always on Top" when borders are hidden.
    - Adjusts the camera's sensor size or orthographic size to match the window dimensions.
- **Key Methods**:
    - `ResizeWindow(float x, float y)`: Targets a new pixel dimension.
    - `ShowWindowBorders(bool value)`: Toggles standard Windows UI elements.

## 2. Hardware Interaction (`USBFileFinder.cs`)
This script monitors for physical USB device events.
- **Functionality**:
    - Hooks into the Windows `WndProc` to listen for `WM_DEVICECHANGE`.
    - Automatically scans removable drives for a specific `targetFileName`.
    - Triggers an event (`onVariableChange`) when the file is found or lost.
- **Use Case**: Likely used for "ARG" style gameplay where the player must provide a physical file on a USB stick to progress.

## 3. Desktop Integration
These scripts suggest a game that blurs the line between the game world and the user's desktop environment.
