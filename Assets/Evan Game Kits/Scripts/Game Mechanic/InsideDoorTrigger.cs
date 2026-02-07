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
                CheckWinCondition();
            }
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
