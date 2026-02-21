using System;

namespace EvanUIKits.Dialogue
{
    public interface ITutorialTask
    {
        Action OnCompleted { get; set; }
        bool IsCompleted();
        void StartTutorial();
        void StopTutorial();
    }
}
