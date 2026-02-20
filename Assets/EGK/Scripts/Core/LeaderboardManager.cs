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

        [Header("Settings")]
        public string mainMenuSceneName = "MainMenuScene";

        [Header("Current Session")]
        public string currentPlayerName = "Anonymous";
        public float sessionTime = 0f;
        public int currentHighestLevel = 0;
        public bool isTracking = false;
        public bool isPaused = false;

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
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            // Pause timing when a level is unloaded (loading starts)
            if (isTracking && scene.name != mainMenuSceneName)
            {
                isPaused = true;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Stop tracking and save if we return to the main menu
            if (scene.name == mainMenuSceneName)
            {
                if (isTracking)
                {
                    SaveAndStop();
                }
                // Reset session data for a completely new run next time
                sessionTime = 0f;
                currentHighestLevel = 0;
                return;
            }

            // Resume timing when a new level is loaded
            if (isTracking)
            {
                isPaused = false;
            }

            // If we are not tracking and entered a level, start a new session
            if (!isTracking)
            {
                StartTracking();
                return;
            }

            // Align with GameCore level progression logic
            int levelNum = -1;
            
            // 1. Try to find a number in the scene name (e.g., "Level 1", "Level1", "Stage 5")
            string digitsOnly = new string(scene.name.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(digitsOnly) && int.TryParse(digitsOnly, out levelNum))
            {
                // Found a number
            }
            // 2. Fallback for Tutorial or other special names
            else if (scene.name.ToLower().Contains("tutorial"))
            {
                levelNum = 0;
            }
            // 3. Last fallback: use build index if no numbers found
            else
            {
                levelNum = scene.buildIndex;
            }

            if (levelNum > currentHighestLevel)
            {
                currentHighestLevel = levelNum;
            }
        }

        private void Update()
        {
            if (isTracking && !isPaused)
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
            isPaused = false;
            // Note: sessionTime and currentHighestLevel are NOT reset here 
            // to allow accumulation across levels if the session was stopped/restarted.
            // They are reset only in OnSceneLoaded when returning to Main Menu.
            
            // Initial check for current scene
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        public void SaveAndStop()
        {
            if (!isTracking) return;

            SaveCurrentSession();
            isTracking = false;
            isPaused = false;
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
