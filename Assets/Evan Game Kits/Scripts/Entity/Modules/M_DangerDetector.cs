using UnityEngine;
using EvanGameKits.Core;
using EvanGameKits.Entity;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Player))]
    public class M_DangerDetector : MonoBehaviour
    {
        private Player player;

        private void Start()
        {
            player = GetComponent<Player>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckDanger(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckDanger(other.gameObject);
        }

        private void CheckDanger(GameObject obj)
        {
            if (GameCore.instance == null) return;

            // Priority 1: Check if the object has the "Danger" tag specifically as requested
            bool isDangerTag = obj.CompareTag("Danger") || (!string.IsNullOrEmpty(GameCore.instance.dangerTag) && obj.CompareTag(GameCore.instance.dangerTag));
            
            // Priority 2: Check if the object is in the Danger layer defined in GameCore
            bool isDangerLayer = (GameCore.instance.dangerLayer.value & (1 << obj.layer)) != 0;

            if (isDangerTag || isDangerLayer)
            {
                GameCore.instance.TakeDamage(player);
            }
        }
    }
}
