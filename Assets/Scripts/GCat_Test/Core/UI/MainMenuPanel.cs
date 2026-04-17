using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using GCFramework.Runtime.UI.UISub;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class MainMenuPanel : UIPanelBase
    {
        public Button startGameBtn;
        public Button settingBtn;
        public Button quitGameBtn;
        public Image fallIntoDreamImage;

        protected override void Awake()
        {
            base.Awake();
            startGameBtn = transform.Find("content/btns/start_game_btn").GetComponent<Button>();
            settingBtn = transform.Find("content/btns/setting_win_btn").GetComponent<Button>();
            quitGameBtn = transform.Find("content/btns/quit_game_btn").GetComponent<Button>();
        }
        
        private void OnEnable()
        {
            startGameBtn.onClick.AddListener(OnClickStartGame);
            settingBtn.onClick.AddListener(OnOpenSettingWindow);
            quitGameBtn.onClick.AddListener(OnClickQuitGame);
        }
        

        private void OnDisable()
        {
            startGameBtn.onClick.RemoveListener(OnClickStartGame);
            settingBtn.onClick.RemoveListener(OnOpenSettingWindow);
            quitGameBtn.onClick.RemoveListener(OnClickQuitGame);
        }

        public override void OnShow()
        {
            base.OnShow();
            fallIntoDreamImage.gameObject.SetActive(false);
        }

        private async void OnClickStartGame()
        {
            startGameBtn.interactable = false;
            await TransitionEffectOfGameStart(2f);
            // 跳转下一个场景
            GameLevelManager.Ins.LoadSceneById(LevelId.Level1);
            startGameBtn.interactable = true;
            this.ForceHideSelf();
        }

        private async UniTask TransitionEffectOfGameStart(float duration)
        {
            Color c = fallIntoDreamImage.color;
            c.a = 0f;
            fallIntoDreamImage.color = c;
            fallIntoDreamImage.gameObject.SetActive(true);
            float elapsedTime = 0f;
            // while (elapsedTime < duration)
            // {
            //     elapsedTime += Time.deltaTime;
            //     c.a = Mathf.Lerp(c.a, 1f, elapsedTime / duration);
            //     fallIntoDreamImage.color = c;
            //     await UniTask.Yield();
            // }
            c.a = 1f;
            await fallIntoDreamImage.DOColor(c, duration).SetEase(Ease.InOutSine);
            
            fallIntoDreamImage.color = c;
        }
        
        private void OnOpenSettingWindow()
        {
            UIManager.Ins.ShowUI(UIWindowName.SettingWindow, ui =>
            {
                SettingWindow settingWindow = ui as SettingWindow;
                if (settingWindow)
                    settingWindow.inGaming = false;
            }).Forget();
        }
        
        private void OnClickQuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}