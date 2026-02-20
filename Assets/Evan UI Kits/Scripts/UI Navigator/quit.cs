using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class quit : MonoBehaviour, IPointerDownHandler
{
    public void quitFunction()
    {
        Debug.Log("Quit application requested.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        quitFunction();
    }
}
