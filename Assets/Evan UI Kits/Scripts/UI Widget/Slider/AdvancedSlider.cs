using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using EvanUIKits.Tweening;

namespace EvanUIKits.Sliders
{
    public class AdvancedSlider : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [Header("References")]
        private Slider slider;
        private RectTransform sliderHandle;
        public TextMeshProUGUI valueText;

        [Header("Settings")]
        public float minValue = 0f;
        public float maxValue = 100f;
        public bool wholeNumbers = true;
        public string valueFormat = "{0}";
        public bool effect = false;
        public AnimateButton.AnimationType animationType = AnimateButton.AnimationType.Scale;

        [Header("Events")]
        public UnityEvent<float> OnValueUpdated;
        public UnityEvent OnInteractionEnded;

        private void Awake()
        {
            EnsureReferences();
        }
        private void EnsureReferences()
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
                if (slider != null)
                {
                    sliderHandle = slider.handleRect;

                    slider.minValue = minValue;
                    slider.maxValue = maxValue;
                    slider.wholeNumbers = wholeNumbers;

                    slider.onValueChanged.RemoveListener(HandleValueChanged);
                    slider.onValueChanged.AddListener(HandleValueChanged);
                }
            }
        }

        private void Start()
        {
            HandleValueChanged(slider.value);
        }

        public void HandleValueChanged(float value)
        {
            if (valueText != null)
            {
                valueText.text = string.Format(valueFormat, value);
            }

            float normalizedValue = (value - slider.minValue) / (slider.maxValue - slider.minValue);

            float angle = normalizedValue * 360f;
            if (sliderHandle != null) sliderHandle.localRotation = Quaternion.Euler(0, 0, angle);
            OnValueUpdated?.Invoke(value);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (sliderHandle != null) AnimateButton.OnButtonDown(sliderHandle, type: animationType);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (sliderHandle != null) AnimateButton.OnButtonUp(sliderHandle, type: animationType);
            OnInteractionEnded?.Invoke();
        }

        public void SetValue(float value, bool notify = true)
        {
            if (notify) slider.value = value;
            else slider.SetValueWithoutNotify(value);

            HandleValueChanged(slider.value);
        }

        public float GetValue()
        {
            return slider.value;
        }
    }
}