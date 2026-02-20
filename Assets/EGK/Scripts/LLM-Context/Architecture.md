# Codebase Architecture Overview

## Design Philosophy
The codebase utilizes a **Modular Component-Based Architecture**. Instead of large, monolithic classes, functionality is decomposed into small, specialized "Modules" that attach to an "Entity Base".

## Core Components

### 1. The Entity System (`EvanGameKits.Entity`)
- **`Base.cs`**: Abstract foundation for all actors (Players/AIs). It caches the `Rigidbody` and holds standard input data (`MoveInput`, `IsJumpPressed`, etc.).
- **`Player.cs`**: Concrete implementation that bridges the Unity Input System to the `Base` variables.

### 2. The Module System (`EvanGameKits.Entity.Module`)
Modules are scripts that perform specific logic by interacting with the `Base` component on the same GameObject.
- **`Locomotion`**: Abstract base for horizontal movement (e.g., `M_Walk`). Runs logic in `FixedUpdate` via `ProcessMovement()`.
- **`Upforce`**: Abstract base for vertical movement (e.g., `M_BasicJump`, `M_Jetpack`). Subscribes to the `IsJumpPressed` event from the `Base` entity.
- **AI Modules**: Follow a similar pattern (`M_AIBehaviourModule`) for behaviors like chasing or patrolling.

### 3. Core Management (`EvanGameKits.Core`)
- **`GameCore.cs`**: A Singleton hub for global game events (Pause, Resume, Scene Loading).
- **`IState`**: Interface used by entities to respond to global state changes via `GameCore` events.

## Communication Pattern
1. **Input**: `Player.cs` updates state in `Base.cs`.
2. **Logic**: Modules (Locomotion/Upforce) read state from `Base.cs` and apply forces to the `Rigidbody`.
3. **Events**: `GameCore` triggers global state changes that entities listen for via `IState`.
