namespace EvanUIKits.Dialogue
{
    public interface ITutorialTask
    {
        bool IsCompleted();
        void StartTutorial();
        void StopTutorial();
    }
}
