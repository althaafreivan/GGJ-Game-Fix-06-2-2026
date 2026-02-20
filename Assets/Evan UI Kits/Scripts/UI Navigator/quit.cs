using UnityEngine;

public class quit : MonoBehaviour
{
    public void quitFunction()
    {
        Application.Quit();
    }

    public void OnMouseDown()
    {
        quitFunction();
    }
}
