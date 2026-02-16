using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

namespace EvanGameKits.Entity.Module
{
    public class M_FrustumDetect : MonoBehaviour
    {
        public static M_FrustumDetect instance;
        public bool isReverse = false;

        private Dictionary<int, UnityAction<bool>> callbacks = new Dictionary<int, UnityAction<bool>>();
        private Dictionary<int, Collider> colliders = new Dictionary<int, Collider>();
        
        // List to keep track of keys for creating arrays
        private List<int> activeKeys = new List<int>();

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

        public void Register(Collider target, UnityAction<bool> callback)
        {
            if (target == null) return;
            int id = target.GetInstanceID();
            if (!callbacks.ContainsKey(id))
            {
                callbacks.Add(id, callback);
                colliders.Add(id, target);
                activeKeys.Add(id);
            }
            else
            {
                // Update callback if re-registering
                callbacks[id] = callback;
            }
        }

        public void Unregister(Collider target)
        {
            if (target == null) return;
            int id = target.GetInstanceID();
            if (callbacks.ContainsKey(id))
            {
                callbacks.Remove(id);
                colliders.Remove(id);
                activeKeys.Remove(id);
            }
        }
        
        // Old method kept for compatibility but redirects to single-frame check
        // Ideally callers should switch to Register/Unregister pattern
        public void CheckVisibility(Collider target, UnityAction<bool> onResult)
        {
            if (Camera.main == null || target == null) return;
            // This is the slow path for legacy calls
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            bool visibleNow = GeometryUtility.TestPlanesAABB(planes, target.bounds);
            onResult?.Invoke(isReverse? !visibleNow : visibleNow);
        }

        private void LateUpdate()
        {
            if (Camera.main == null || activeKeys.Count == 0) return;

            // 1. Clean up any destroyed colliders
            for (int i = activeKeys.Count - 1; i >= 0; i--)
            {
                int id = activeKeys[i];
                if (colliders[id] == null)
                {
                    colliders.Remove(id);
                    callbacks.Remove(id);
                    activeKeys.RemoveAt(i);
                }
            }

            int count = activeKeys.Count;
            if (count == 0) return;

            // 2. Allocations
            NativeArray<Bounds> boundsData = new NativeArray<Bounds>(count, Allocator.TempJob);
            NativeArray<bool> results = new NativeArray<bool>(count, Allocator.TempJob);
            NativeArray<Plane> jobPlanes = new NativeArray<Plane>(6, Allocator.TempJob);

            // 3. Populate Data
            for (int i = 0; i < count; i++)
            {
                boundsData[i] = colliders[activeKeys[i]].bounds;
            }

            Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            NativeArray<Plane>.Copy(camPlanes, jobPlanes, 6);

            // 4. Schedule Job
            var job = new FrustumCheckJob
            {
                Planes = jobPlanes,
                Bounds = boundsData,
                Results = results
            };

            JobHandle handle = job.Schedule(count, 32);
            
            // 5. Complete Job immediately (simplest integration)
            // Ideally we would schedule in Update and complete in LateUpdate, 
            // but we are already in LateUpdate to ensure camera pos is final.
            handle.Complete();

            // 6. Fire Callbacks
            bool reverse = isReverse;
            for (int i = 0; i < count; i++)
            {
                bool visible = results[i];
                int id = activeKeys[i];
                if (callbacks.TryGetValue(id, out var cb))
                {
                    cb?.Invoke(reverse ? !visible : visible);
                }
            }
            
            // 7. Cleanup
            boundsData.Dispose();
            results.Dispose();
            jobPlanes.Dispose();
        }

        [BurstCompile]
        struct FrustumCheckJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Plane> Planes;
            [ReadOnly] public NativeArray<Bounds> Bounds;
            public NativeArray<bool> Results;

            public void Execute(int index)
            {
                Bounds b = Bounds[index];
                Vector3 center = b.center;
                Vector3 extents = b.extents;
                
                bool visible = true;
                for (int i = 0; i < 6; i++)
                {
                    Plane p = Planes[i];
                    Vector3 normal = p.normal;
                    float distance = p.distance;
                    
                    float r = extents.x * Mathf.Abs(normal.x) + 
                              extents.y * Mathf.Abs(normal.y) + 
                              extents.z * Mathf.Abs(normal.z);
                    
                    float d = Vector3.Dot(normal, center) + distance;
                    
                    if (d < -r)
                    {
                        visible = false;
                        break;
                    }
                }
                Results[index] = visible;
            }
        }
    }
}