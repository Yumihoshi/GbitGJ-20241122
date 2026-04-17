using System;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GCat_Test.Core.Levels
{
    public class GameOverPoint : MonoBehaviour
    {
        private bool _flag;
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Keyboard.current.eKey.wasPressedThisFrame && !_flag)
                {
                    GameLevelManager.Ins.LoadSceneById(LevelId.Level2);
                    _flag = true;
                }
            }
        }
    }
}