# AI System Context

## Architecture
AI entities follow the same modular structure as the Player. They use a `Base` (or `AI` subclass) for movement and data.

## AI Modules (`AIBehaviourModule`)
AI logic is encapsulated in modules that implement `IAIBehaviour`:

```csharp
public interface IAIBehaviour {
    int Priority { get; }
    bool CanExecute();
    Vector3 GetDirection();
}
```

## Key AI Modules
- **`M_AI_Chase`**: Follows a target.
- **`M_AI_Patrol`**: Moves between points.
- **`M_AI_Sight`**: Detects targets within a frustum or radius.
- **`M_AI_CustomNavigator`**: Specialized movement logic.

## Logic Flow
AI modules typically update their desired direction or state based on the `Priority` and environmental conditions, then feed this "input" into standard locomotion modules (like `M_Walk`).
