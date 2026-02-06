using DG.Tweening;
using UnityEngine;

public class MainMenuCameraAnimation : MonoBehaviour
{
    public float duration;
    public Vector3 position;
    private Transform cam, originalPosition;

    private void Start()
    {
        cam = GetComponent<Transform>();
        originalPosition = cam;
        transform.DOMove(originalPosition.position, duration).From(originalPosition.position+position);

    }
}
