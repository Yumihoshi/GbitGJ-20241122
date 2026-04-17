using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCat_Test.Core.Events;
using GCat_Test.Core.StoryModule;
using GCFramework.Runtime.MessageCenter;
using GCFramework.Runtime.UI;
using GCFramework.Runtime.UI.UISub;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    /// <summary>
    /// 剧情显示框窗口
    /// </summary>
    public class StoryDisplayWindow : UIWindowBase
    {
        public TextMeshProUGUI contentText;
        public Text contentText1;
        public Text nameText;
        public Image nextIcon;
        public float typewriteDuration = 5f;
        private List<StoryData> _storyDataList;

        private Action _storyEndCallback;
        private StringBuilder _contentStrBuilder;
        private int _currentTextDataIdx;
        private int _textDataLength;
        private bool _isSkip;
        private bool _isTypewriting;
        private bool _isInStoryDisplay;

        private CancellationTokenSource _skipCancelTokenSource = new CancellationTokenSource();

        protected override void Awake()
        {
            base.Awake();
            _contentStrBuilder = new StringBuilder();
        }

        protected override void Update()
        {
            base.Update();

            if (_isTypewriting)
            {
                if (!_isSkip && ClickNextContent())
                {
                    _skipCancelTokenSource?.Cancel();
                }
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            Debug.Log("显示对话框");
            // if (_isInStoryDisplay)
            //     return;
            _contentStrBuilder.Clear();
            StoryDisplayHandle();   
            // 触发剧情事件
            EventCenter.Fire<StoryTriggerEvent>();
        }

        public void SetStoryData(List<StoryData> data, Action storyEndCallback)
        {
            _storyDataList = data;
            _currentTextDataIdx = 0;
            _textDataLength = _storyDataList.Count;
            _storyEndCallback = storyEndCallback;
        }

        private async void StoryDisplayHandle()
        {
            _isInStoryDisplay = true;
            while (_currentTextDataIdx < _textDataLength)
            {
                var data = _storyDataList[_currentTextDataIdx];
                nextIcon.gameObject.SetActive(false);
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(data.name));
                nameText.text = data.name;
                await DisplayTextByTypewriting(data.contentText);
                nextIcon.gameObject.SetActive(true);
                await UniTask.WaitUntil(ClickNextContent);
                await UniTask.Yield();
                _contentStrBuilder.Clear();
                _currentTextDataIdx++;
            }
            _storyEndCallback?.Invoke();
            this.HideSelf();
            // 触发剧情结束事件
            EventCenter.Fire<StoryEndEvent>();
            _isInStoryDisplay = false;
        }

        private async UniTask DisplayTextByTypewriting(string texts)
        {
            float delayTime = typewriteDuration / texts.Length;
            _isTypewriting = true;
            for (int i = 0; i < texts.Length; i++)
            {
                _contentStrBuilder.Append(texts[i]);
                // contentText.text = _contentStrBuilder.ToString();
                contentText1.text = _contentStrBuilder.ToString();
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: _skipCancelTokenSource.Token).SuppressCancellationThrow();
                if (_skipCancelTokenSource.IsCancellationRequested)
                {
                    _isSkip = true;
                    _contentStrBuilder.Append(texts.Substring(i + 1, texts.Length - (i + 1)));
                    _skipCancelTokenSource.Dispose();
                    _skipCancelTokenSource = new CancellationTokenSource();
                    break;
                }
            }

            _isTypewriting = false;
            _isSkip = false;
            // contentText.text = _contentStrBuilder.ToString();
            contentText1.text = _contentStrBuilder.ToString();
        }

        private bool ClickNextContent()
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame;
        }
    }
}