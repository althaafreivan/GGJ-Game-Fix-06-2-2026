using UnityEngine;
using UnityEngine.Events;
using EvanGameKits.Entity.Module;
using EvanGameKits.Core;

namespace EvanGameKits.GameMechanic
{
    public class InsideDoorTrigger : MonoBehaviour
    {
        private bool blackCatInside = false;
        private bool whiteCatInside = false;

        private const string WHITE_CAT_NAME = "Expected";
        private const string BLACK_CAT_NAME = "Nothing";

        [Header("Events")]
        public UnityEvent onBothCatsInside;

        private void OnTriggerEnter(Collider other)
        {
            M_CatIdentity identity = other.GetComponent<M_CatIdentity>();
            if (identity == null) identity = other.GetComponentInParent<M_CatIdentity>();

            if (identity != null)
            {
                if (identity.catType == CatType.Black) blackCatInside = true;
                else if (identity.catType == CatType.White) whiteCatInside = true;

                UpdateHUD(identity.catType, true);
                ShowPortalNotification(identity.catType);
                CheckWinCondition();
            }
        }

        private void ShowPortalNotification(CatType type)
        {
            // Only show if exactly one cat is inside
            if (blackCatInside && whiteCatInside) return;

            string currentCat = (type == CatType.White) ? WHITE_CAT_NAME : BLACK_CAT_NAME;
            string otherCat = (type == CatType.White) ? BLACK_CAT_NAME : WHITE_CAT_NAME;

            NotificationController.instance?.ShowNotification($"{currentCat} in the portal, waiting for {otherCat}", Color.cyan);
        }

        private void OnTriggerExit(Collider other)
        {
            M_CatIdentity identity = other.GetComponent<M_CatIdentity>();
            if (identity == null) identity = other.GetComponentInParent<M_CatIdentity>();

            if (identity != null)
            {
                if (identity.catType == CatType.Black) blackCatInside = false;
                else if (identity.catType == CatType.White) whiteCatInside = false;

                UpdateHUD(identity.catType, false);
            }
        }

        private void UpdateHUD(CatType type, bool inside)
        {
            if (CompletionHUDController.instance != null)
            {
                CompletionHUDController.instance.SetMark(type, inside);
            }
        }

        private void CheckWinCondition()
        {
            if (blackCatInside && whiteCatInside)
            {
                Debug.Log("Both cats are inside the trigger!");
                onBothCatsInside?.Invoke();
                if (GameCore.instance != null)
                {
                    GameCore.instance.win();
                }
            }
        }
    }
}
