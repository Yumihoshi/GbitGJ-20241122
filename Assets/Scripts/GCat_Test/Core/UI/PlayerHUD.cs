using System;
using System.Collections;
using Aki_Test.Character_Stats;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using GCFramework.Runtime.UI.UISub;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class PlayerHUD : UIPanelBase
    {
        public Button settingBtn;
        public RectTransform taskTarget;
        public RectTransform optionTips;
        public PlayerStateDisplay playerStateDisplay;
        private PlayerAgent _playerAgent;
        private CharacterStats _characterStates;

        private Text _taskTargetText;
        private Text _optionTipsText;
        
        protected override void Awake()
        {
            base.Awake();
            playerStateDisplay = GetComponentInChildren<PlayerStateDisplay>(true);
            _taskTargetText = taskTarget.Find("task_target").GetComponent<Text>();
            _optionTipsText = optionTips.Find("option_tips").GetComponent<Text>();
            settingBtn = transform.Find("header/setting_win_btn").GetComponent<Button>();
        }

        private void Start()
        {
            StartCoroutine(DelayInitState());
            Init();
        }

        private void OnEnable()
        {
            GameLevelManager.Ins.onSetOptionTips += OnSetOptionTips;
            settingBtn.onClick.AddListener(OnOpenSettingWindow);
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                GameLevelManager.Ins.onSetOptionTips -= OnSetOptionTips;
            settingBtn.onClick.RemoveListener(OnOpenSettingWindow);
        }

        private void Init()
        {
            taskTarget.gameObject.SetActive(false);
            optionTips.gameObject.SetActive(false);
        }
        
        
        private void OnOpenSettingWindow()
        {
            UIManager.Ins.ShowUI(UIWindowName.SettingWindow, ui =>
            {
                SettingWindow settingWindow = ui as SettingWindow;
                if (settingWindow)
                    settingWindow.inGaming = true;
            }).Forget();   
        }

        private IEnumerator DelayInitState()
        {
            yield return null;
            _playerAgent = FindObjectOfType<PlayerAgent>();
            _characterStates = _playerAgent.GetComponent<CharacterStats>();
            _characterStates.OnHealthChanged += OnHealthChange;
        }

        private void OnHealthChange(int hp, int maxHp)
        {
            // 0 - 0.2f - 0.6f - 0.8f - 1f
            float value = (float)(maxHp - hp) / maxHp;
            if (value is >= 0.2f and < 0.6f)
                playerStateDisplay.ChangePlayerHpDisplayState(1);
            else if (value is >= 0.6f and < 0.8f)
                playerStateDisplay.ChangePlayerHpDisplayState(2);
            else if (value is >= 0.8f and <= 1f)
                playerStateDisplay.ChangePlayerHpDisplayState(3);
            else 
                playerStateDisplay.ChangePlayerHpDisplayState(0);
        }
        
        private void OnSetOptionTips(string tips)
        {
            optionTips.gameObject.SetActive(!string.IsNullOrEmpty(tips));
            _optionTipsText.text = tips;
        }
    }
}