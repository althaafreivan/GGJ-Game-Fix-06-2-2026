using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public abstract class Upforce : MonoBehaviour
    {
        protected Entity.Base player;

        protected virtual void Start()
        {
            player = GetComponent<Entity.Base>();
            player.IsJumpPressed += ProcessJump;
        }

        public abstract void ProcessJump(bool isPressed);

        private void OnEnable()
        {
            if (player != null) player.IsJumpPressed += ProcessJump;
        }

        private void OnDisable()
        {
            player.IsJumpPressed -= ProcessJump;
        }

        private void OnDestroy()
        {
            player.IsJumpPressed -= ProcessJump;
        }
    }

}
