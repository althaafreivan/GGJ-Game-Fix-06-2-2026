using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public class M_AI_CustomNavigator : AIBehaviourModule
    {
        public Transform target;
        public float obstacleDetectionRange = 2f;
        public LayerMask obstacleMask;

        protected override void Update()
        {
            if (target == null || entity == null) return;

            Vector3 finalDir = GetDirectionWithAvoidance();

            float camY = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0;
            Vector3 compensatedDir = Quaternion.Inverse(Quaternion.Euler(0, camY, 0)) * finalDir;

            float moveX = Mathf.Abs(compensatedDir.x) > 0.1f ? Mathf.Sign(compensatedDir.x) : 0f;
            float moveZ = Mathf.Abs(compensatedDir.z) > 0.1f ? Mathf.Sign(compensatedDir.z) : 0f;

            entity.OnMove(new Vector2(moveX, moveZ));
        }

        private Vector3 GetDirectionWithAvoidance()
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            dirToTarget.y = 0;

            if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit hit, obstacleDetectionRange, obstacleMask))
            {
                Vector3 avoidanceDir = Vector3.Reflect(dirToTarget, hit.normal);
                return (dirToTarget + avoidanceDir).normalized;
            }

            return dirToTarget;
        }
    }
}