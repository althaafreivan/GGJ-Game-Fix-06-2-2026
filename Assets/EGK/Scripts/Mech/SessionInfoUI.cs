using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using EvanGameKits.Core;

namespace EvanGameKits.Mechanic
{
    public class SessionInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Text component to display the current player's name")]
        public TextMeshProUGUI playerNameText;
        [Tooltip("Text component to display the real-time session time")]
        public TextMeshProUGUI timePlayedText;
        [Tooltip("Text component to display the current estimated rank")]
        public TextMeshProUGUI rankText;
        [Tooltip("The parent object of the session info UI (usually a Panel or Canvas)")]
        public GameObject container;

        [Header("Settings")]
        public bool showOnlyInLevels = true;
        public string mainMenuSceneName = "MainMenuScene";

        private void Start()
        {
            UpdateVisibility();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (container == null) return;

            string currentScene = SceneManager.GetActiveScene().name;
            if (showOnlyInLevels)
            {
                // Show if NOT in main menu
                container.SetActive(currentScene != mainMenuSceneName);
            }
        }

        private void Update()
        {
            if (LeaderboardManager.instance == null) return;
            if (!LeaderboardManager.instance.isTracking)
            {
                // Optional: hide if not tracking
                // if (container != null && container.activeSelf) container.SetActive(false);
                return;
            }

            if (playerNameText != null)
                playerNameText.text = LeaderboardManager.instance.currentPlayerName;

            if (timePlayedText != null)
                timePlayedText.text = FormatTime(LeaderboardManager.instance.sessionTime);

            if (rankText != null)
                rankText.text = "Rank: " + GetCurrentRank();
        }

        private string FormatTime(float time)
        {
            int hours = (int)time / 3600;
            int minutes = ((int)time % 3600) / 60;
            int seconds = (int)time % 60;
            int milliseconds = (int)((time - (int)time) * 100);

            if (hours > 0)
                return string.Format("{0:0}h {1:02}m {2:02}s", hours, minutes, seconds);
            else
                return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private string GetCurrentRank()
        {
            var data = LeaderboardManager.instance.LoadLeaderboard();
            if (data == null || data.entries == null || data.entries.Count == 0)
                return "1st";

            float currentTime = LeaderboardManager.instance.sessionTime;
            int currentLevel = LeaderboardManager.instance.currentHighestLevel;

            int rank = 1;
            foreach (var entry in data.entries)
            {
                // Better rank if higher level reached
                if (currentLevel < entry.highestLevel)
                {
                    rank++;
                }
                // If same level, better rank if less time
                else if (currentLevel == entry.highestLevel && currentTime > entry.totalTime)
                {
                    rank++;
                }
            }

            return OrdinalSuffix(rank);
        }

        private string OrdinalSuffix(int n)
        {
            if (n <= 0) return n.ToString();

            switch (n % 100)
            {
                case 11:
                case 12:
                case 13:
                    return n + "th";
            }

            switch (n % 10)
            {
                case 1: return n + "st";
                case 2: return n + "nd";
                case 3: return n + "rd";
                default: return n + "th";
            }
        }
    }
}
