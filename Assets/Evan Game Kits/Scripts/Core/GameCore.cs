using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EvanGameKits.Core
{
    public class GameCore : MonoBehaviour
    {
        public static GameCore instance;
        [SerializeField] public UnityEvent onSceneLoaded;
        [SerializeField] public UnityEvent onPause;
        [SerializeField] public UnityEvent onResume;
        [SerializeField] public UnityEvent onRestart;
        [SerializeField] public UnityEvent onExit;
        [SerializeField] public UnityEvent onWin;

        [Header("Game Settings")]
        public int maxHearts = 3;
        private int currentHearts;

        [Header("Danger Configuration")]
        public string dangerTag = "Danger";
        public LayerMask dangerLayer;
        public bool respawnAllOnDamage = true;

        [Header("Game Events")]
        [SerializeField] public UnityEvent<int> onHeartsChanged;

        private void OnEnable()
        {
            if(instance == null) instance = this;
            DontDestroyOnLoad(instance);
            onSceneLoaded.Invoke();
            currentHearts = maxHearts;
            onHeartsChanged?.Invoke(currentHearts);
        }

        public void Pause()
        {
            Time.timeScale = 0f;
            onPause?.Invoke();
        }

        public void Resume()
        {
            Time.timeScale = 1f;
            onResume?.Invoke();
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            onRestart?.Invoke();
            currentHearts = maxHearts;
            onHeartsChanged?.Invoke(currentHearts);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void win()
        {
            onWin?.Invoke();
        }

        public void TakeDamage(EvanGameKits.Entity.Player player)
        {
            if (currentHearts <= 0) return;

            currentHearts--;
            onHeartsChanged?.Invoke(currentHearts);

            if (currentHearts > 0)
            {
                if (respawnAllOnDamage)
                {
                    foreach (var p in EvanGameKits.Entity.Player.AllPlayers)
                    {
                        if (p != null) p.Respawn();
                    }
                }
                else if (player != null)
                {
                    player.Respawn();
                }
            }
            else
            {
                EndGame();
            }
        }

        public void Dead(EvanGameKits.Entity.Player player)
        {
            TakeDamage(player);
        }

        public void EndGame()
        {
            onExit?.Invoke();
        }

        public void loadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

}


namespace EvanGameKits.Core
{
    public interface IState
    {
        virtual void OnPause() { }
        virtual void OnResume() { }
        virtual void OnRestart(){ }
        virtual void OnExit() { }
        virtual void OnWin() { }
    }
}
