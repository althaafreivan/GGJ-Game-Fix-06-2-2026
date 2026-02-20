using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public abstract class GroundDetector : MonoBehaviour
    {
        public abstract bool isGrounded();
        public virtual Rigidbody GetGroundRigidbody() => null;
    }

}
