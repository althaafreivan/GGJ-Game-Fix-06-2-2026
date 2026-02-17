using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace EvanGameKits.Entity.Module
{
    [RequireComponent(typeof(Player))]
    public class M_ActiveMark : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject activeMark;
        [SerializeField] private GameObject highlightMark;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 180f; // degrees per second
        [SerializeField] private Ease rotationEase = Ease.Linear;

        private Player player;
        private Tween rotationTween;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private void OnEnable()
        {
            Player.onPlayerChange += HandlePlayerChange;
            // Initial check to sync with current active player
            UpdateActiveMark(Player.ActivePlayer);
        }

        private void OnDisable()
        {
            Player.onPlayerChange -= HandlePlayerChange;
            StopRotation();
        }

        private void HandlePlayerChange(Player activePlayer)
        {
            UpdateActiveMark(activePlayer);
        }

        private void UpdateActiveMark(Player activePlayer)
        {
            if (activeMark == null) return;

            bool isActive = (activePlayer == player);
            activeMark.SetActive(isActive);
            highlightMark.SetActive(isActive);

            if (isActive)
            {
                StartRotation();
            }
            else
            {
                StopRotation();
            }
        }

        private void StartRotation()
        {
            if (rotationTween != null || activeMark == null) return;

            // Calculate duration based on speed (360 degrees / speed)
            float duration = 360f / Mathf.Max(rotationSpeed, 0.1f);

            rotationTween = activeMark.transform.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                .SetEase(rotationEase)
                .SetLoops(-1, LoopType.Incremental)
                .SetRelative(true);
        }

        private void StopRotation()
        {
            if (rotationTween != null)
            {
                rotationTween.Kill();
                rotationTween = null;
            }
        }
    }
}
