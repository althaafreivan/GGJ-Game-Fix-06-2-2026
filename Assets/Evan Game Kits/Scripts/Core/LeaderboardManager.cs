using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EvanGameKits.Core
{
    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public float totalTime;
        public int highestLevel;
        public string date;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager instance;

        [Header("Current Session")]
        public string currentPlayerName = "Anonymous";
        public float sessionTime = 0f;
        public int currentHighestLevel = 0;
        public bool isTracking = false;

        private string SavePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

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
            if (!isTracking) return;

            // Align with GameCore level progression logic
            int levelNum = -1;
            if (scene.name.Contains("Level"))
            {
                string numPart = scene.name.Replace("Level ", "").Replace("Level", "").Trim();
                if (int.TryParse(numPart, out levelNum)) { }
            }
            else if (scene.name == "Tutorial")
            {
                levelNum = 0;
            }

            if (levelNum > currentHighestLevel)
            {
                currentHighestLevel = levelNum;
            }
        }

        private void Update()
        {
            if (isTracking)
            {
                sessionTime += Time.deltaTime;
            }
        }

        public void SetPlayerName(string name)
        {
            currentPlayerName = string.IsNullOrEmpty(name) ? "Anonymous" : name;
        }

        public void StartTracking()
        {
            isTracking = true;
            sessionTime = 0f;
            currentHighestLevel = 0;
            
            // Initial check for current scene
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        public void SaveAndStop()
        {
            if (!isTracking) return;

            SaveCurrentSession();
            isTracking = false;
            Debug.Log($"Leaderboard: Session saved and tracking stopped for {currentPlayerName}");
        }

        private void SaveCurrentSession()
        {
            LeaderboardData data = LoadLeaderboard();
            
            LeaderboardEntry entry = new LeaderboardEntry
            {
                playerName = currentPlayerName,
                totalTime = sessionTime,
                highestLevel = currentHighestLevel,
                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            data.entries.Add(entry);
            
            // Sort: Level DESC, then Time ASC
            data.entries = data.entries
                .OrderByDescending(e => e.highestLevel)
                .ThenBy(e => e.totalTime)
                .ToList();

            // Keep top 50 local records
            if (data.entries.Count > 50)
            {
                data.entries = data.entries.Take(50).ToList();
            }
            
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Leaderboard: Failed to save to JSON file: {e.Message}");
            }
        }

        public LeaderboardData LoadLeaderboard()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    return JsonUtility.FromJson<LeaderboardData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Leaderboard: Failed to read JSON, starting fresh: {e.Message}");
                }
            }
            return new LeaderboardData();
        }

        public List<LeaderboardEntry> GetEntries()
        {
            return LoadLeaderboard().entries;
        }
    }
}
