using UnityEngine;
using EvanGameKits.Entity;
using Unity.Cinemachine;

namespace EvanGameKits.Mechanic
{
    public class OcclusionManager : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private CinemachineCamera cCam;
        [SerializeField] private LayerMask wallLayer;

        [Header("Ray Settings")]
        [SerializeField] private float rayRadius = 0.5f;
        [Tooltip("Number of extra walls to the left and right of the hit point to hide.")]
        [SerializeField] private int neighborSpread = 4;
        [SerializeField] private float neighborDistance = 2.0f;

        private void Awake()
        {
            Player.onPlayerChange += HandlePlayerChange;
        }
        private void OnEnable()
        {
            Player.onPlayerChange += HandlePlayerChange;
            if (Player.ActivePlayer != null) playerTransform = Player.ActivePlayer.transform;
            
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= HandlePlayerChange;
        }

        private void HandlePlayerChange(Player newPlayer)
        {
            if (newPlayer != null) playerTransform = newPlayer.transform;
            cCam.Target.LookAtTarget = playerTransform;
        }

        private void Update()
        {
            if (playerTransform == null) return;
            PerformOcclusionCheck();
        }

        private void PerformOcclusionCheck()
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, rayRadius, direction, distance, wallLayer);

            foreach (var hit in hits)
            {
                WallDissolveController wall = hit.collider.GetComponent<WallDissolveController>();
                if (wall != null)
                {
                    wall.RequestHide();

                    if (neighborSpread > 0)
                    {
                        HideNeighbors(hit.collider.transform, hit.normal);
                    }
                }
            }
        }

        private void HideNeighbors(Transform hitTransform, Vector3 hitNormal)
        {
            Vector3 sideDir = Vector3.Cross(Vector3.up, hitNormal).normalized;
            if (sideDir.sqrMagnitude < 0.1f) sideDir = Vector3.right;

            for (int i = 1; i <= neighborSpread; i++)
            {
                CheckAndHideAtPos(hitTransform.position + (sideDir * neighborDistance * i));
                CheckAndHideAtPos(hitTransform.position - (sideDir * neighborDistance * i));
            }
        }

        private void CheckAndHideAtPos(Vector3 pos)
        {
            Collider[] cols = Physics.OverlapSphere(pos, 0.5f, wallLayer);
            foreach (var col in cols)
            {
                WallDissolveController neighbor = col.GetComponent<WallDissolveController>();
                if (neighbor != null) neighbor.RequestHide();
            }
        }
    }
}