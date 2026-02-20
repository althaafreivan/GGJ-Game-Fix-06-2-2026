using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_AI_Chase : AIBehaviourModule, IAIBehaviour
    {
        public Transform target;
        private M_AI_Sight sight;
        public UnityEvent<bool> onEvent;
        public UnityEvent onChased;

        private bool isCurrentlyChasing = false;
        public int Priority => 50;

        protected override void Start()
        {
            base.Start();
            sight = GetComponent<M_AI_Sight>();
        }

        public bool CanExecute()
        {
            if (sight == null) return false;

            bool canSee = sight.CanSeeTarget();

            if (canSee)
            {
                target = sight.target;
            }

            if (canSee != isCurrentlyChasing)
            {
                isCurrentlyChasing = canSee;
                onEvent?.Invoke(isCurrentlyChasing);
            }

            return isCurrentlyChasing;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("its here");
            if (collision.gameObject.CompareTag("Player")) onChased?.Invoke();
        }

        public Vector3 GetDirection()
        {
            if (target == null) return Vector3.zero;

            Vector3 desiredDir = (target.position - transform.position).normalized;
            float checkDistance = 3f;

            if (Physics.Raycast(transform.position, desiredDir, out RaycastHit hit, checkDistance))
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(desiredDir, hit.normal);
                return slideDir.normalized;
            }

            return desiredDir;
        }
    }
}