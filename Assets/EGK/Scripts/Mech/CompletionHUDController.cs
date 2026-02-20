using UnityEngine;

namespace EvanGameKits.GameMechanic
{
    public class CompletionHUDController : MonoBehaviour
    {
        public static CompletionHUDController instance;

        [Header("Marks")]
        [SerializeField] private GameObject blackCatMark;
        [SerializeField] private GameObject whiteCatMark;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()
        {
            if (blackCatMark != null) blackCatMark.SetActive(false);
            if (whiteCatMark != null) whiteCatMark.SetActive(false);
        }

        public void SetMark(Entity.Module.CatType type, bool active)
        {
            if (type == Entity.Module.CatType.Black)
            {
                if (blackCatMark != null) blackCatMark.SetActive(active);
            }
            else if (type == Entity.Module.CatType.White)
            {
                if (whiteCatMark != null) whiteCatMark.SetActive(active);
            }
        }
    }
}
