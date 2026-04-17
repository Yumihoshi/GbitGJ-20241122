using System;
using Cysharp.Threading.Tasks;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using GCFramework.Runtime.UI.UISub;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class GameOverPanel : UIPanelBase
    {
        public Button restartBtn;
        public Button returnMenuBtn;
        public Button quitGameBtn;
        
        private void OnEnable()
        {
            restartBtn.onClick.AddListener(OnRestartGame);
            returnMenuBtn.onClick.AddListener(OnReturnMenu);
            quitGameBtn.onClick.AddListener(OnQuitGame);
        }

        private void OnDisable()
        {
            restartBtn.onClick.RemoveListener(OnRestartGame);
            returnMenuBtn.onClick.RemoveListener(OnReturnMenu);
            quitGameBtn.onClick.RemoveListener(OnQuitGame);
        }

        private void OnQuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
                        Application.Quit();
#endif
        }

        private void OnReturnMenu()
        {
            GameLevelManager.Ins.LoadSceneById(LevelId.MainScene, onLoadBegin: () =>
            {
                UIManager.Ins.ClearAllUI();
                UIManager.Ins.SetUI(UIPanelName.MainMenuPanel, false).Forget();
            });
            
        }

        private void OnRestartGame()
        {
            GameLevelManager.Ins.LoadSceneById(LevelId.Level1, onLoadBegin: this.ForceHideSelf);
        }
    }
}