using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public List<soundLibrary> sfxLibraries, musicLibraries;
}

[System.Serializable]
public class soundLibrary
{
    public string Name;
    public AudioClip Clip;
}