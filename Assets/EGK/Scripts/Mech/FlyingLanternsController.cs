using UnityEngine;
using System.Collections.Generic;

namespace EvanGameKits.Mechanic
{
    /// <summary>
    /// Controls a group of flying lanterns from a single parent object.
    /// Provides slow, drifting, and looped movement for a magical atmosphere.
    /// </summary>
    public class FlyingLanternsController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _driftSpeed = 0.2f;
        [SerializeField] private Vector3 _driftRange = new Vector3(2f, 1f, 2f);
        [SerializeField] private float _floatFrequency = 0.5f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float _swayAmount = 10f;
        [SerializeField] private float _swaySpeed = 0.5f;

        [Header("Performance")]
        [Tooltip("If true, automatically finds all children on Start.")]
        [SerializeField] private bool _autoCollectChildren = true;

        private struct LanternData
        {
            public Transform transform;
            public Vector3 startPosition;
            public Quaternion startRotation;
            public float seed;
        }

        private List<LanternData> _lanterns = new List<LanternData>();

        private void Start()
        {
            if (_autoCollectChildren)
            {
                CollectLanterns();
            }
        }

        [ContextMenu("Collect Children")]
        public void CollectLanterns()
        {
            _lanterns.Clear();
            foreach (Transform child in transform)
            {
                _lanterns.Add(new LanternData
                {
                    transform = child,
                    startPosition = child.localPosition,
                    startRotation = child.localRotation,
                    seed = Random.value * 100f // Randomize the phase for each lantern
                });
            }
        }

        private void Update()
        {
            float time = Time.time;

            for (int i = 0; i < _lanterns.Count; i++)
            {
                LanternData data = _lanterns[i];
                float individualTime = time * _driftSpeed + data.seed;

                // 1. Calculate Wandering Position (Looped via Sine/Cos)
                // We use different frequencies for X, Y, Z to create a complex path
                Vector3 offset = new Vector3(
                    Mathf.Sin(individualTime) * _driftRange.x,
                    Mathf.Sin(individualTime * 1.5f * _floatFrequency) * _driftRange.y,
                    Mathf.Cos(individualTime * 0.8f) * _driftRange.z
                );

                data.transform.localPosition = data.startPosition + offset;

                // 2. Calculate Gentle Swaying Rotation
                float swayX = Mathf.Sin(time * _swaySpeed + data.seed) * _swayAmount;
                float swayZ = Mathf.Cos(time * _swaySpeed * 0.7f + data.seed) * _swayAmount;
                
                data.transform.localRotation = data.startRotation * Quaternion.Euler(swayX, 0, swayZ);
            }
        }

        // Draw Gizmos to show the drift range in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (Transform child in transform)
            {
                Gizmos.DrawWireCube(child.position, _driftRange * 2);
            }
        }
    }
}
