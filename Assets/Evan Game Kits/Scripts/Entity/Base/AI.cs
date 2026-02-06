

using EvanGameKits.Entity.Module;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvanGameKits.Entity
{
    public class AI : Base
    {
        public List<IAIBehaviour> modules = new List<IAIBehaviour>();
        protected override void Awake()
        {
            base.Awake();
            modules = GetComponents<IAIBehaviour>().ToList();
        }

        private void Update()
        {
            var activeBehaviour = modules.Where(b => b.CanExecute()).OrderByDescending(b => b.Priority).FirstOrDefault();

            Vector3 worldDir = Vector3.zero;
            if (activeBehaviour != null)
            {
                worldDir = activeBehaviour.GetDirection();
            }

            ApplyDigitalMovement(worldDir);
        }

        private void ApplyDigitalMovement(Vector3 worldDir)
        {
            float camY = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0;
            Vector3 compDir = Quaternion.Inverse(Quaternion.Euler(0, camY, 0)) * worldDir;

            float moveX = Mathf.Abs(compDir.x) > 0.1f ? Mathf.Sign(compDir.x) : 0f;
            float moveZ = Mathf.Abs(compDir.z) > 0.1f ? Mathf.Sign(compDir.z) : 0f;

            OnMove(new Vector2(moveX, moveZ));
        }

        public void OnMove(Vector2 moveInput) => MoveInput = moveInput;
        public void OnHover(float hoverInput) => HoverInput = hoverInput;
    }

}

