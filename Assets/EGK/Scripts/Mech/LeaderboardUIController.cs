using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EvanGameKits.Core;
using TMPro; // Assuming you are using TextMeshPro for modern UI

namespace EvanGameKits.Mechanic
{
    public class LeaderboardUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform container; // The 'Content' object of the ScrollView
        
        [Header("Card Field Names (Optional)")]
        [Tooltip("Ensure your prefab has these child names or assign them in a custom script on the prefab")]
        public string rankTextName = "Rank";
        public string nameTextName = "Name";
        public string levelTextName = "Level";
        public string timeTextName = "Time";

        private void OnEnable()
        {
            RefreshLeaderboard();
        }

        private void Start()
        {
            RefreshLeaderboard();
        }

        public void RefreshLeaderboard()
        {
            // 1. Clear existing items
            if (container != null)
            {
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }
            }

            // 2. Get data from manager
            if (LeaderboardManager.instance == null)
            {
                LeaderboardManager.instance = FindFirstObjectByType<LeaderboardManager>();
            }

            if (LeaderboardManager.instance == null) return;
            
            List<LeaderboardEntry> entries = LeaderboardManager.instance.GetEntries();

            // 3. Populate
            if (cardPrefab != null && container != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    GameObject card = Instantiate(cardPrefab, container);
                    SetupCard(card, entries[i], i + 1);
                }
            }
        }

        private void SetupCard(GameObject card, LeaderboardEntry entry, int rank)
        {
            // Attempt to find TMPro components in children by name
            // You can also create a specific 'LeaderboardCard' script to hold these references
            var rankText = FindComponent<TextMeshProUGUI>(card, rankTextName);
            var nameText = FindComponent<TextMeshProUGUI>(card, nameTextName);
            var levelText = FindComponent<TextMeshProUGUI>(card, levelTextName);
            var timeText = FindComponent<TextMeshProUGUI>(card, timeTextName);

            if (rankText) rankText.text = rank.ToString();
            if (nameText) nameText.text = entry.playerName;
            if (levelText) levelText.text = "Lv. " + entry.highestLevel;

            // Fallback for time if not found by name
            if (timeText == null)
            {
                var allTMPs = card.GetComponentsInChildren<TextMeshProUGUI>();
                // In the default prefab, there are 4 components: Rank, Name, Level, and Time
                if (allTMPs.Length >= 4) timeText = allTMPs[3]; 
            }

            if (timeText) timeText.text = FormatTime(entry.totalTime);
        }

        private T FindComponent<T>(GameObject parent, string name) where T : Component
        {
            Transform t = parent.transform.Find(name);
            if (t != null) return t.GetComponent<T>();
            
            // Fallback: search all children if direct find fails
            foreach (T component in parent.GetComponentsInChildren<T>())
            {
                if (component.name == name) return component;
            }
            return null;
        }

        private string FormatTime(float time)
        {
            int hours = (int)time / 3600;
            int minutes = ((int)time % 3600) / 60;
            int seconds = (int)time % 60;

            if (hours > 0)
                return string.Format("{0:0}h {1:00}m {2:00}s", hours, minutes, seconds);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
