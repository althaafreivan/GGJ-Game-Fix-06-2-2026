using UnityEngine;

namespace EvanUIKits.PanelController
{
    public class UIPanel : MonoBehaviour
    {
        [HideInInspector]public PanelMap panel;

        private void Start()
        {
            panel.canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }


        [System.Serializable]
        public class PanelMap
        {
            [HideInInspector]public CanvasGroup canvasGroup;
        }
    }
}

