using System;
using Cysharp.Threading.Tasks;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using GCFramework.Runtime.UI.UISub;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class SettingWindow : UIWindowBase
    {
        public Slider musicSlider;
        public Slider soundEffectSlider;
        public Button returnBtn;
        public Button returnMenuBtn;
        public TabManager tabManager;

        public bool inGaming;
        
        private void Start()
        {
            InitState();
        }

        private void OnEnable()
        {
            returnBtn.onClick.AddListener(OnClickReturn);
            returnMenuBtn.onClick.AddListener(OnReturnMenu);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChange);
            soundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChange);
        }

        private void OnDisable()
        {
            returnBtn.onClick.RemoveListener(OnClickReturn);
            returnMenuBtn.onClick.RemoveListener(OnReturnMenu);
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChange);
            soundEffectSlider.onValueChanged.RemoveListener(OnSoundEffectVolumeChange);
        }

        public override void OnShow()
        {
            base.OnShow();
            if (tabManager) tabManager.InitTab();
            returnMenuBtn.gameObject.SetActive(inGaming);
        }

        private void InitState()
        {
            musicSlider.value = AudioManager.Ins.MusicVolume;
            soundEffectSlider.value = AudioManager.Ins.SoundEffectVolume;
        }

        private void OnClickReturn()
        {
            this.HideSelf();
        }
        
        private void OnReturnMenu()
        {
            returnMenuBtn.interactable = false;
            returnBtn.interactable = false;
            GameLevelManager.Ins.LoadSceneById(LevelId.MainScene, onLoadBegin: HandleMainMenuUI);
            returnMenuBtn.interactable = true;
            returnBtn.interactable = true;
        }

        private void HandleMainMenuUI()
        {
            this.ForceHideSelf();
            UIManager.Ins.ClearAllUI();
            UIManager.Ins.SetUI(UIPanelName.MainMenuPanel, false).Forget();
        }

        private void OnMusicVolumeChange(float value)
        {
            AudioManager.Ins.SetMusicVolume(value);
        }

        private void OnSoundEffectVolumeChange(float value)
        {
            AudioManager.Ins.SetSoundEffectVolume(value);   
        }

    }
}