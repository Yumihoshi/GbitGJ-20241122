using System;
using GCat_Test.Core.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace Aki_Test
{
    public class RoomEnemyChecker : MonoBehaviour
    {
        public UnityAction<int> EnemyAllClear;
        public int RoomId;
        private bool _isClear = false;
        
        private void Start()
        {
            if (GameLevelManager.Ins != null) EnemyAllClear += GameLevelManager.Ins.EnableTransportPoint;
            else
            {
                Debug.LogError("GameLevelManager Not Initialized.");
            }
        }

        private void Update()
        {
            CheckEnemies();
        }
        
        void CheckEnemies()
        {
            if (!_isClear && transform.childCount <= 0)
            {
                _isClear = true;
                EnemyAllClear?.Invoke(RoomId);
            }
        }
    }
}
