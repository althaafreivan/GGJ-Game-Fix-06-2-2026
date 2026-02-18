using EvanGameKits.Entity.Module;
using UnityEngine;
using UnityEngine.Events;

public class EnterTrigger : MonoBehaviour
{

    public enum allowedCat
    {
        white,
        black,
        both
    }

    public allowedCat ct = allowedCat.both;
    public UnityEvent onPlayerEnter;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (ct == allowedCat.both)
            {
                onPlayerEnter?.Invoke();
            }
            if(ct == allowedCat.black)
            {
                M_CatIdentity identity = other.GetComponent<EvanGameKits.Entity.Module.M_CatIdentity>();
            }
        }
    }
}
