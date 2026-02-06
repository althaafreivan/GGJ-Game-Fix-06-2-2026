using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Application;

namespace EvanUIKits.Confirmation
{
    public class Confirmation : MonoBehaviour
    {
        public static Confirmation Instance;

        public GameObject modal;
        public TextMeshProUGUI messageText;
        private Action onYesAction;
        private Action onNoAction;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            modal.SetActive(false);
        }

        public void ShowWithoutCallback(string message)
        {
            messageText.text = message;
            modal.SetActive(true);
        }

        public void Show(string message, Action yesCallback, Action noCallback = null)
        {
            messageText.text = message;
            onYesAction = yesCallback;
            onNoAction = noCallback;

            modal.SetActive(true);
        }

        public void yes()
        {
            onYesAction?.Invoke();
            Hide();
        }

        public void no()
        {
            onNoAction?.Invoke();
            Hide();
        }

        private void Hide()
        {
            onYesAction = null;
            onNoAction = null;
            modal.SetActive(false);
        }
    }
}