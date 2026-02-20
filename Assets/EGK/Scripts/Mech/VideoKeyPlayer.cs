using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace EvanGameKits.Mechanic
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoKeyPlayer : MonoBehaviour
    {
        [Serializable]
        public struct VideoClipMapping
        {
            public string key;
            public VideoClip clip;
        }

        [SerializeField] private List<VideoClipMapping> videoClips = new List<VideoClipMapping>();
        
        private VideoPlayer _videoPlayer;

        private void Awake()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        /// <summary>
        /// Plays a video clip associated with the given key.
        /// </summary>
        /// <param name="key">The unique identifier for the video clip.</param>
        public void PlayClip(string key)
        {
            if (_videoPlayer == null) _videoPlayer = GetComponent<VideoPlayer>();

            foreach (var mapping in videoClips)
            {
                if (mapping.key == key)
                {
                    if (mapping.clip != null)
                    {
                        _videoPlayer.clip = mapping.clip;
                        _videoPlayer.Play();
                    }
                    else
                    {
                        Debug.LogWarning($"[VideoKeyPlayer] Clip for key '{key}' is null.");
                    }
                    return;
                }
            }

            Debug.LogWarning($"[VideoKeyPlayer] No clip found for key: {key}");
        }

        /// <summary>
        /// Stops the current video.
        /// </summary>
        public void Stop()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }
    }
}
