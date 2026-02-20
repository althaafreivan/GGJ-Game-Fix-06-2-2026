# Input & State Integration

## Input Handling (`Player.cs`)
The project uses the **Unity Input System**. `Player.cs` maps input actions directly to state variables and events in the `Base` class:

- **Move**: Updates `MoveInput` (Vector2).
- **Jump**: Invokes `IsJumpPressed` (Action<bool>).
- **Run**: Invokes `IsRunPressed` (Action<bool>).
- **Hover**: Updates `HoverInput` (float).

## State Management (`IState`)
Entities implement `IState` to handle global game transitions. The `Player` class caches all `IState` components on the object and can propagate events:

```csharp
public interface IState {
    void OnPause();
    void OnResume();
    void OnRestart();
    void OnExit();
    void OnWin();
}
```

## Physics Logic
Horizontal movement is generally smoothed using `Vector3.SmoothDamp` or direct velocity manipulation within modules. Vertical movement often includes a `fallMultiplier` to prevent "floaty" gravity.
