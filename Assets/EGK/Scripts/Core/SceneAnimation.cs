using UnityEngine;
using UnityEngine.Events;

public abstract class SceneAnimation : MonoBehaviour
{
    public UnityEvent onComplete;

    public abstract void PlayAnimation();
}
