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
        [SerializeField] private int allowedDeaths = 3;
        private int deathCount = 0;

        private void OnEnable()
        {
            if(instance == null) instance = this;
            DontDestroyOnLoad(instance);
            onSceneLoaded.Invoke();
            deathCount = 0;
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void win()
        {
            onWin?.Invoke();
        }

        public void Dead(EvanGameKits.Entity.Player player)
        {
            deathCount++;
            if (deathCount <= allowedDeaths)
            {
                player.Respawn();
            }
            else
            {
                EndGame();
            }
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
