using UnityEngine;
using EvanGameKits.Entity.Module;
using System.Linq;

namespace EvanGameKits.Entity.Module
{
    public class M_AI_Patrol : AIBehaviourModule, IAIBehaviour
    {
        public int Priority => 10;
        public bool CanExecute() => waypoints.Length > 0 && enabled;

        [Header("Detection")]
        public float obstacleDetectionRange = 2f;
        public float avoidanceForce = 5f;
        public LayerMask obstacleMask;

        [Header("Movement")]
        public Transform waypointParent;
        private Transform[] waypoints;
        public float stopDistance = 2f;
        private Vector3 lastChosenDir;
        private int index = 0;

        private void OnEnable()
        {
            waypoints = waypointParent.GetComponentsInChildren<Transform>();
        }

        public Vector3 GetDirection()
        {
            Vector3 targetPos = waypoints[index].position;
            Vector3 rawDir = (targetPos - transform.position).normalized;
            rawDir.y = 0;

            Vector3 combinedDir = Vector3.zero;
            int clearPaths = 0;

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 candidateDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, candidateDir, obstacleDetectionRange, obstacleMask))
                {
                    float weight = Mathf.Max(0, Vector3.Dot(candidateDir, rawDir));

                    combinedDir += candidateDir * weight;
                    clearPaths++;
                }
            }

            if (combinedDir == Vector3.zero || clearPaths == 0)
            {
                return -transform.forward;
            }

            if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
            {
                index = (index + 1) % waypoints.Length;
            }

            return combinedDir.normalized;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * obstacleDetectionRange);
        }
    }
}