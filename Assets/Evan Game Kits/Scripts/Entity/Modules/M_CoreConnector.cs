using UnityEngine;
using EvanGameKits.Core;
using System;

namespace EvanGameKits.Entity.Module
{
    public abstract class M_Core : MonoBehaviour
    {
        GameCore instance;
        private Action<bool> isPause;

        private void Start() 
        {
            instance = GameCore.instance;
            instance.onPause?.AddListener(pause);
            instance.onResume?.AddListener(resume);
            instance.onWin?.AddListener(win);
        }

        private void pause()
        {

        }

        private void resume() 
        {
        
        }

        private void win()
        {

        }

    }
}
