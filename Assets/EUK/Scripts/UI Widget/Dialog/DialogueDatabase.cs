using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace EvanUIKits.Dialogue
{
    [Serializable]
    public class SentenceData
    {
        [TextArea(3, 5)] public string text;
        public bool useCharacter2;
        public UnityEvent OnSentenceStart;
        public UnityEvent OnSentenceEnd;
    }

    [Serializable]
    public class DialogueEntry
    {
        public string key;
        public bool isOneTime;
        [HideInInspector] public bool hasBeenPlayed;
        public string characterName;
        public Sprite portrait;
        public string characterName2;
        public Sprite portrait2;
        public List<SentenceData> sentences = new List<SentenceData>();
    }

    public class DialogueDatabase : MonoBehaviour
    {
        public static DialogueDatabase Instance;
        public bool resetOnStart = true;
        public List<DialogueEntry> dialogues = new List<DialogueEntry>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (resetOnStart) ResetPlayedStates();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ResetPlayedStates()
        {
            foreach (var dialogue in dialogues)
            {
                dialogue.hasBeenPlayed = false;
            }
        }

        public DialogueEntry GetDialogue(string key)
        {
            return dialogues.Find(d => d.key == key);
        }
    }
}
