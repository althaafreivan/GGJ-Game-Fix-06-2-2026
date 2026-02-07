using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EvanGameKits.Core;

namespace EvanGameKits.GameMechanic
{
    public class HeartUIController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite brokenHeartSprite;
        [SerializeField] private GameObject heartPrefab;

        private List<Image> heartImages = new List<Image>();

        private void Start()
        {
            if (GameCore.instance != null)
            {
                InitializeHearts(GameCore.instance.maxHearts);
                GameCore.instance.onHeartsChanged.AddListener(UpdateHearts);
            }
        }

        private void InitializeHearts(int maxHearts)
        {
            // Clear existing children in case of reload
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            heartImages.Clear();

            for (int i = 0; i < maxHearts; i++)
            {
                GameObject heartObj = Instantiate(heartPrefab, transform);
                Image img = heartObj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = fullHeartSprite;
                    heartImages.Add(img);
                }
            }
        }

        private void UpdateHearts(int currentHearts)
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (i < currentHearts)
                {
                    heartImages[i].sprite = fullHeartSprite;
                }
                else
                {
                    heartImages[i].sprite = brokenHeartSprite;
                }
            }
        }

        private void OnDestroy()
        {
            if (GameCore.instance != null)
            {
                GameCore.instance.onHeartsChanged.RemoveListener(UpdateHearts);
            }
        }
    }
}
