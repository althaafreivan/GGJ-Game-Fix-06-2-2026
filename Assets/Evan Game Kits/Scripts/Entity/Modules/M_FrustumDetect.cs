using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_FrustumDetect : MonoBehaviour
    {
        public static M_FrustumDetect instance;
        public bool isReverse = false;

        private void Awake()
        {
            Player player = GetComponent<Player>();
         
            if (Player.ActivePlayer == player) 
            {
                instance = this; 
            }
        }

        private void OnEnable()
        {
            if(instance != this) instance = this;
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