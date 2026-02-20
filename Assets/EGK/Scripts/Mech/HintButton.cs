using UnityEngine;
using UnityEngine.UI;

namespace EvanGameKits.Mechanic
{
    [RequireComponent(typeof(Button))]
    public class HintButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OpenGuide);
        }

        public void OpenGuide()
        {
            if (GuideSystem.instance != null)
            {
                GuideSystem.instance.ShowGuide();
            }
            else
            {
                Debug.LogWarning("GuideSystem instance not found in scene!");
            }
        }
    }
}