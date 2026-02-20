using UnityEngine;

namespace EvanGameKits.Entity.Module
{
    public abstract class Locomotion : MonoBehaviour
    {
        protected Entity.Base entity;
        protected virtual void Start()
        {
            entity = GetComponent<Entity.Base>();
        }

        public abstract void ProcessMovement();

        protected virtual void FixedUpdate()
        {
            if (entity != null) ProcessMovement();
        }
    }

}

