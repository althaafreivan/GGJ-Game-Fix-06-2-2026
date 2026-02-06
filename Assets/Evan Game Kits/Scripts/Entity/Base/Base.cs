using System;
using UnityEngine;

namespace EvanGameKits.Entity
{
    public abstract class Base : MonoBehaviour
    {
        public Rigidbody rb { get; protected set; }
        public Vector2 MoveInput { get; protected set; }
        public float HoverInput { get; protected set; }
        public Action<bool> IsJumpPressed { get; set; }
        public Action<bool> IsRunPressed { get; set; }

        protected virtual void Awake()
        {
           rb = GetComponent<Rigidbody>();
        }
    }

}
