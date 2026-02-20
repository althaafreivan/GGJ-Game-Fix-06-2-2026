using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace EvanUIKits.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [System.Serializable]
        public struct SoundData
        {
            public string key;
            public AudioClip clip;
        }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private List<SoundData> soundEffects;
        [SerializeField] private AudioClip startupMusic;
        [SerializeField, Range(0.1f, 2f)] private float pitchVariation = 0.1f;

        public AudioSource sfx, music;

        public enum VolumeType { Music, SFX }

        private void Awake()
        {
            if (instance == null || instance != this)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            AdjustVolume(VolumeType.Music, 1f); AdjustVolume(VolumeType.SFX, 1f);
            PlayerPrefs.SetFloat("music", 1f); PlayerPrefs.SetFloat("sfx", 1f);

            music = gameObject.AddComponent<AudioSource>();
            music.outputAudioMixerGroup = mixer.FindMatchingGroups("music")[0];
            music.playOnAwake = false;
        }

        private void Start()
        {
            if (startupMusic != null)
            {
                music.loop = true;
                music.clip = startupMusic;
                music.Play();
            }
        }

        public void AdjustVolume(VolumeType type, float level)
        {
            string parameterName = type == VolumeType.Music ? "musicVolume" : "sfxVolume";
            mixer.SetFloat(parameterName, Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20);
            PlayerPrefs.SetFloat(parameterName, level);
        }

        public void PlaySFX(string name)
        {
            var entry = soundEffects.Find(s => s.key == name);
            if (entry.clip != null)
            {
                sfx.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                sfx.PlayOneShot(entry.clip);
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (music.clip == clip) return;
            music.clip = clip;
            music.Play();
        }
    }
}