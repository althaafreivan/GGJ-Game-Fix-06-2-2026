# Key Class Signatures & Interfaces

## Core Singleton
```csharp
namespace EvanGameKits.Core {
    public class GameCore : MonoBehaviour {
        public static GameCore instance;
        public UnityEvent onPause, onResume, onRestart, onWin;
        public void Pause();
        public void Resume();
        public void Restart();
    }

    public interface IState {
        void OnPause();
        void OnResume();
        void OnRestart();
        void OnWin();
    }
}
```

## Entity Foundation
```csharp
namespace EvanGameKits.Entity {
    public abstract class Base : MonoBehaviour {
        public Rigidbody rb { get; }
        public Vector2 MoveInput { get; }
        public Action<bool> IsJumpPressed { get; set; }
        // standardized data for modules to consume
    }
}
```

## Modular Movement
```csharp
namespace EvanGameKits.Entity.Module {
    // Horizontal Movement
    public abstract class Locomotion : MonoBehaviour {
        protected Entity.Base entity;
        public abstract void ProcessMovement(); // Called in FixedUpdate
    }

    // Vertical Movement
    public abstract class Upforce : MonoBehaviour {
        protected Entity.Base player;
        public abstract void ProcessJump(bool isPressed); // Event-driven
    }
}
```
