using System;
using System.Collections;
using System.Collections.Generic;
using Aki_Test.Player;
using Cysharp.Threading.Tasks;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.Levels;
using GCat_Test.Core.UI;
using GCFramework.Runtime.UI;
using GCFramework.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GCat_Test.Core.Manager
{
    public enum LevelRoom
    {
        Room1,
        Room2,
        Room3,
        Room4,
        BossRoom
    }
    
    public class GameLevelManager : SingletonMono<GameLevelManager>
    {
        public GameLevelDataModel levelDataModel;
        [LabelText("场景过渡的过渡UI特效")]
        public TransitionManager.TransitionEffect transitionEffect;

        public Action onLoadLevelBegin;
        public Action onLoadLevelAfter;
        public Action<string> onSetOptionTips;

        private readonly Dictionary<int, TransportPoint> _transportPointDic = new Dictionary<int, TransportPoint>();
        private readonly Dictionary<int, CheckingPoint> _checkingPointDic = new Dictionary<int, CheckingPoint>();

        public PlayerController Player { get; private set; }
        private CheckingPoint _lastCheckingPoint;
        
        private void Start()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            yield return null;
            yield return new WaitWhile(() =>
            {
                Player = FindObjectOfType<PlayerController>();
                return Player == null;
            });
            Player.onDie += OnPlayerDie;
        }

        private void OnPlayerDie()
        {
            DelayResurrectionPlayer().Forget();
        }

        private async UniTask DelayResurrectionPlayer()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            
            if (_lastCheckingPoint != null)
            {
                await TransitionManager.Ins.EnterTransition(TransitionManager.TransitionEffect.FadeInOut_Custom);
                Player.transform.position = _lastCheckingPoint.revivalPoint.position;
                Player.characterStats.RespawnPlayer();
                await TransitionManager.Ins.ExitTransition(TransitionManager.TransitionEffect.FadeInOut_Custom);
            }
            else
            {
                UIManager.Ins.ClearAllUI();
                UIManager.Ins.SetUI(UIPanelName.MainMenuPanel, false).Forget();
                LoadSceneById(LevelId.MainScene);
            }
        }

        #region 传送点

        /// <summary>
        /// 开启传送点
        /// </summary>
        /// <param name="id">传送点ID，可在场景Transport_Point_Root下查看</param>
        public void EnableTransportPoint(int id)
        {
            if (_transportPointDic.TryGetValue(id, out var point))
            {
                point.gameObject.SetActive(true);
                Debug.Log($"激活{point.name}传送点");
            }
        }
        
        public void RegisterTransportPoint(TransportPoint point)
        {
            _transportPointDic.TryAdd(point.transportId, point);
        }

        public void UnRegisterTransportPoint(TransportPoint point)
        {
            if (_transportPointDic.ContainsKey(point.transportId))
                _transportPointDic.Remove(point.transportId);
        }
        
        
        public TransportPoint GetDestination(int destinationId)
        {
            if (_transportPointDic.TryGetValue(destinationId, out var destination))
                return destination;
            return null;
        }


        #endregion

        public void AddCheckingPoint(CheckingPoint point)
        {
            _checkingPointDic.TryAdd(point.id, point);
            _lastCheckingPoint = point;
        }

        public void OnSetOptionTips(string tips)
        {
            onSetOptionTips?.Invoke(tips);
        }

        /// <summary>
        /// 加载到某个关卡
        /// </summary>
        /// <param name="levelId">关卡编号</param>
        /// <param name="onLoadBegin"></param>
        /// <param name="onLoadAfter"></param>
        public void LoadSceneById(LevelId levelId, Action onLoadBegin = null, Action onLoadAfter = null)
        {
            var sceneName = levelDataModel.GetSceneName(levelId);
            if (string.IsNullOrEmpty(sceneName))
                return;
            InternalLoadSceneHandle(sceneName, onLoadBegin, onLoadAfter);
        }

        private async void InternalLoadSceneHandle(string sceneName, Action onLoadBegin = null, Action onLoadAfter = null)
        {
            // 如果有过渡UI则显示
            if (TransitionManager.Ins)
                await TransitionManager.Ins.EnterTransition(transitionEffect);

            onLoadBegin?.Invoke();
            onLoadLevelBegin?.Invoke();
            
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();

            onLoadLevelAfter?.Invoke();
            onLoadLevelAfter?.Invoke();
            
            if (TransitionManager.Ins)
                await TransitionManager.Ins.ExitTransition(transitionEffect);
        }
    }
}