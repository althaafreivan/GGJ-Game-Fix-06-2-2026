using UnityEngine;
using EvanUIKits.Dialogue;

namespace EvanGameKits.Tutorial
{
    public class TutorialActionTrigger : MonoBehaviour
    {
        [Header("Settings")]
        public string actionName;
        public bool triggerOnEnable = false;
        
        private void OnEnable()
        {
            if (triggerOnEnable)
            {
                TriggerAction();
            }
        }

        public void TriggerAction()
        {
            if (DialogueManager.instance != null && DialogueManager.instance.activeTutorial != null)
            {
                if (DialogueManager.instance.activeTutorial is TutorialTask task)
                {
                    task.NotifyAction(actionName);
                }
            }
        }

        // Helper for physics-based triggers (Trampolines, Portals, etc.)
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TriggerAction();
            }
        }
    }
}
