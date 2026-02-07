using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_FrustumDetect : MonoBehaviour
    {
        public static M_FrustumDetect instance;
        public bool isReverse = false;

        private void OnEnable()
        {
            Player.onPlayerChange += HandlePlayerChange;
            UpdateInstance();
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= HandlePlayerChange;
            if (instance == this) instance = null;
        }

        private void HandlePlayerChange(Player newPlayer)
        {
            UpdateInstance();
        }

        private void UpdateInstance()
        {
            Player player = GetComponent<Player>();
            if (player != null && Player.ActivePlayer == player)
            {
                instance = this;
            }
        }

        public void CheckVisibility(Collider target, UnityAction<bool> onResult)
        {
            if (Camera.main == null) return;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            bool visibleNow = GeometryUtility.TestPlanesAABB(planes, target.bounds);
            onResult?.Invoke(isReverse? !visibleNow : visibleNow);
        }
    }
}