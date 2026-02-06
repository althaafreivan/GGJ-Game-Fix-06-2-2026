using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public class M_AI_Sight : AIBehaviourModule
    {
        public Transform target;
        [Range(0, 360)] public float maxViewAngle = 90f;
        public float viewDistance = 10f;
        public Vector3 detectionOffset = new Vector3(0, 0.5f, 0);
        public LayerMask obstacleMask;

        [Header("Lock Rotation")]
        public bool lockX = true;
        public bool lockY = false;
        public bool lockZ = true;

        public bool CanSeeTarget()
        {
            if (target == null) return false;

            Vector3 eyePos = transform.position + detectionOffset;
            Vector3 dirToTarget = (target.position - eyePos).normalized;
            float distToTarget = Vector3.Distance(eyePos, target.position);

            if (distToTarget <= viewDistance)
            {
                if (Vector3.Angle(GetSightForward(), dirToTarget) < maxViewAngle / 2f)
                {
                    if (!Physics.Raycast(eyePos, dirToTarget, distToTarget, obstacleMask))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Vector3 GetSightForward()
        {
            Vector3 rot = transform.eulerAngles;

            float xRot = lockX ? 0 : rot.x;
            float yRot = lockY ? 0 : rot.y;
            float zRot = lockZ ? 0 : rot.z;

            return Quaternion.Euler(xRot, yRot, zRot) * Vector3.forward;
        }

        private void OnDrawGizmos()
        {
            Vector3 eyePos = transform.position + detectionOffset;
            Vector3 forward = GetSightForward();

            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawWireSphere(eyePos, viewDistance);

            Vector3 leftRay = Quaternion.Euler(0, -maxViewAngle / 2, 0) * forward;
            Vector3 rightRay = Quaternion.Euler(0, maxViewAngle / 2, 0) * forward;

            Gizmos.color = CanSeeTarget() ? Color.red : Color.yellow;

            Gizmos.DrawRay(eyePos, leftRay * viewDistance);
            Gizmos.DrawRay(eyePos, rightRay * viewDistance);
            Gizmos.DrawLine(eyePos + leftRay * viewDistance, eyePos + rightRay * viewDistance);

            Gizmos.DrawRay(eyePos, forward * viewDistance);
        }
    }
}