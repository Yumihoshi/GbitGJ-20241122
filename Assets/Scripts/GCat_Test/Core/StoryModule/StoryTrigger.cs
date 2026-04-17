using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GCat_Test.Core.UI;
using GCFramework.Runtime.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace GCat_Test.Core.StoryModule
{
    [System.Serializable]
    public class StoryData
    {
        public string name;
        [Multiline(5)] 
        public string contentText;
        public bool isShakeCamera;
    }

    [System.Serializable]
    public class StoryInfo
    {
        [LabelText("剧情ID")]
        public int storyId;
        [LabelText("触发剧情ID")]
        public int nextStoryId;
        [LabelText("剧情触发时事件")]
        public string[] beginEventNames;
        [LabelText("剧情结束时事件")]
        public string[] endEventNames;
        public UnityEvent[] onBeginEvents;
        public UnityEvent[] onEndEvents;
        public List<StoryData> storyDataList;
    }
    
    [RequireComponent(typeof(BoxCollider2D))]
    public class StoryTrigger : MonoBehaviour
    {
        public bool isTrigger;
        public StoryInfo storyInfo;

        // ------- Event ---------
        public delegate void TriggerStoryDelegate();
        public event TriggerStoryDelegate TriggerStoryEvent;

        private BoxCollider2D _triggerCollider;
        
        /// <summary>
        /// 剧情是否被触发过
        /// </summary>
        public bool IsStoryTriggered { get; private set; }

        private void Awake()
        {
            _triggerCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            IsStoryTriggered = false;
            _triggerCollider.isTrigger = true;
            if (!isTrigger)
                _triggerCollider.enabled = false;
        }

        private void OnEnable()
        {
            TriggerStoryEvent += OnTriggerStory;
        }

        private void OnDisable()
        {
            TriggerStoryEvent -= OnTriggerStory;
        }

        /// <summary>
        /// 触发该剧情
        /// </summary>
        public void TriggerStory()
        {
            TriggerStoryEvent?.Invoke();
        }
        
        /// <summary>
        /// 当剧情事件触发
        /// </summary>
        private void OnTriggerStory()
        {
            if (IsStoryTriggered)
                return;
            IsStoryTriggered = true;
            
            StoryManager.Ins.OnBeginStory();
            
            UIManager.Ins.ShowUI(UIWindowName.StoryDisplayWindow, ui =>
            {
                StoryDisplayWindow storyDisplayWindow = (StoryDisplayWindow)ui;
                storyDisplayWindow.SetStoryData(storyInfo.storyDataList, OnTriggerEnd);
            }).Forget();
            foreach (var eventName in storyInfo.beginEventNames)
            {
                StoryManager.Ins.FireEvent(eventName);
            }

            foreach (var unityEvent in storyInfo.onBeginEvents)
            {
                unityEvent?.Invoke();
            }
        }

        /// <summary>
        /// 当剧情结束了
        /// </summary>
        public void OnTriggerEnd()
        {
            StoryManager.Ins.OnEndStory();
            foreach (var eventName in storyInfo.endEventNames)
            {
                StoryManager.Ins.FireEvent(eventName);
            }

            foreach (var unityEvent in storyInfo.onEndEvents)
            {
                unityEvent?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isTrigger || !other.CompareTag("Player"))
                return;
            
            TriggerStory();
        }
    }
}