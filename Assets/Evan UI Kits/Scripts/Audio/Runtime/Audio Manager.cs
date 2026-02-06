using UnityEngine;
using UnityEngine.Audio;

namespace EvanUIKits.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private SoundLibrary soundLibrary;
        [SerializeField, Range(0.1f, 2f)] private float pitchVariation = 0.1f;

        private AudioSource sfx, music;

        public enum VolumeType { Music, SFX }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            sfx = gameObject.AddComponent<AudioSource>();
            sfx.outputAudioMixerGroup = mixer.FindMatchingGroups("sfx")[0];
            sfx.playOnAwake = false;

            music = gameObject.AddComponent<AudioSource>();
            music.outputAudioMixerGroup = mixer.FindMatchingGroups("music")[0];
            music.playOnAwake = false;
            music.loop = true;

            AdjustVolume(VolumeType.SFX, PlayerPrefs.GetFloat("sfxVolume", 0.75f));
            AdjustVolume(VolumeType.Music, PlayerPrefs.GetFloat("musicVolume", 0.75f));
        }

        public void AdjustVolume(VolumeType type, float level)
        {
            string parameterName = type == VolumeType.Music ? "musicVolume" : "sfxVolume";
            mixer.SetFloat(parameterName, Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20);
            PlayerPrefs.SetFloat(parameterName, level);
        }

        public void PlaySFX(string name)
        {
            var entry = soundLibrary.sfxLibraries.Find(s => s.Name == name);
            if (entry != null && entry.Clip != null)
            {
                sfx.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                sfx.PlayOneShot(entry.Clip);
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

