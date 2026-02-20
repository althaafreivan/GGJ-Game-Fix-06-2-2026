using EvanUIKits.PanelController;
using EvanUIKits.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EvanUIKits
{
    public enum ButtonType
    {
        Default,
        PushPanel,
        PopPanel,
    }
}

namespace EvanUIKits.AdvancedButton
{
    [RequireComponent(typeof(RectTransform))]
    public class AdvancedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ButtonType buttonType;
        public AnimateButton.AnimationType animationType = AnimateButton.AnimationType.Scale;

        [HideInInspector] public string targetPanelName;
        [HideInInspector] public int selectedPanelIndex;
        [HideInInspector] private UINavigator nav;

        private RectTransform button;

        public bool isUsingCustomEvent = false;
        public UnityEvent OnButtonDown;
        public UnityEvent OnButtonUp;
        public UnityEvent OnButtonEnter;
        public UnityEvent OnButtonExit;

        private void Start()
        {
            nav = UINavigator.instance;
            button = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateButton.OnButtonDown(button);
            OnButtonDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateButton.OnButtonUp(type: animationType, button: button, onComplete: () => {

                switch (buttonType)
                {
                    case ButtonType.PushPanel:
                        if (nav != null) nav.PushPanel(targetPanelName);
                        break;
                    case ButtonType.PopPanel:
                        if (nav != null) nav.PopPanel();
                        break;
                    default: break;
                }

                OnButtonUp?.Invoke();
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnButtonEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnButtonExit.Invoke();
        }
    }
}
