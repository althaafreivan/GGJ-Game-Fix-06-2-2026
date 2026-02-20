using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EvanUIKits.PanelController
{
    public class UINavigator : MonoBehaviour
    {
        public static UINavigator instance;

        public List<UIPanel> panels = new List<UIPanel>();
        private Stack<UIPanel> history = new Stack<UIPanel>();

        public string Breadcrumb { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (panels.Count > 0)
            {
                for (int i = 0; i < panels.Count; i++)
                {
                    if (panels[i] != null) panels[i].gameObject.SetActive(false);        
                }

                PushPanel(panels[0].name);
            }
        }

        public void PushPanel(string target)
        {
            UIPanel panel = panels.Find(p => p.name == target);

            if (panel == null)
            {
                Debug.LogWarning($"UINavigator: Panel '{target}' not found in the list!");
                return;
            }

            if (history.Count > 0)
            {
                if (history.Peek() == panel) return;
                history.Peek().Hide();
            }

            panel.Show();
            history.Push(panel);

            UpdateBreadcrumbs();
        }

        public void PopPanel()
        {
            if (history.Count <= 1) return;

            UIPanel current = history.Pop();
            current.Hide();

            UIPanel previous = history.Peek();
            previous.Show();

            UpdateBreadcrumbs();
        }

        private void UpdateBreadcrumbs()
        {
            Breadcrumb = string.Join(" > ", history.Select(p => p.name).Reverse());      
        }
    }
}
