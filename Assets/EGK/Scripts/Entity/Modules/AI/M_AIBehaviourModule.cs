using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public abstract class AIBehaviourModule : MonoBehaviour
    {
        protected AI entity;

        protected virtual void Start()
        {
            entity = GetComponent<AI>();
        }

        protected virtual void Update() { }
    }

}

namespace EvanGameKits.Entity.Module
{
    public interface IAIBehaviour
    {
        int Priority { get; }
        bool CanExecute();

        Vector3 GetDirection();
    }
}