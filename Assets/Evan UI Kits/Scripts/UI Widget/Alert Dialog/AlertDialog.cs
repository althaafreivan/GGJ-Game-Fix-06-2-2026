using UnityEngine;
using System;
using TMPro;

namespace EvanUIKits.Confirmation
{
    public class Alert : MonoBehaviour
    {
        public static Alert Instance;

        public GameObject modal;
        public TextMeshProUGUI messageText;
        private Action onAction;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            modal.SetActive(false);
        }

        public void Show(string message, Action ok)
        {
            messageText.text = message;
            onAction = onClick;

            modal.SetActive(true);
        }

        public void onClick()
        {
            onAction?.Invoke();
            Hide();
        }

        private void Hide()
        {
            onAction = null;
            modal.SetActive(false);
        }
    }
}