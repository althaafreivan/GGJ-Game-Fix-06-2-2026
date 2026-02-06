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
        public UnityEvent OnSentenceStart;
        public UnityEvent OnSentenceEnd;
    }

    [Serializable]
    public class DialogueEntry
    {
        public string key;
        public string characterName;
        public Sprite portrait;
        public List<SentenceData> sentences = new List<SentenceData>();
    }

    public class DialogueDatabase : MonoBehaviour
    {
        public static DialogueDatabase Instance;
        public List<DialogueEntry> dialogues = new List<DialogueEntry>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public DialogueEntry GetDialogue(string key)
        {
            return dialogues.Find(d => d.key == key);
        }
    }
}