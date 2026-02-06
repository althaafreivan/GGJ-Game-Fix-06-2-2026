using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelData : ScriptableObject
{
    public string[] level;

    private void LoadSceneMode(int level)
    {
        Debug.Log(level);
    }
}
