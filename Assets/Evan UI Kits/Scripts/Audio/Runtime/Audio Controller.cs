using UnityEngine;
using UnityEngine.UI;

namespace EvanUIKits.Audio
{
    public class AudioController : MonoBehaviour
    {
        public AudioManager.VolumeType volumeType = AudioManager.VolumeType.SFX;
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            if (_slider == null) return;

            _slider.onValueChanged.AddListener(adjustVolume);

            string key = (volumeType == AudioManager.VolumeType.SFX) ? "sfxVolume" : "musicVolume";
            _slider.value = PlayerPrefs.GetFloat(key, 0.75f);
        }

        private void OnDisable()
        {
            if (_slider != null)
                _slider.onValueChanged.RemoveListener(adjustVolume);
        }

        public void adjustVolume(float volume)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.AdjustVolume(volumeType, volume);
            }
        }
    }
}