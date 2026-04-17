// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/24 13:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using System.Collections.Generic;
using Tsuki_Test.Enemy;
using UnityEngine;

namespace Tsuki_Test.Manager
{
    public class QteEnemyManager : MonoBehaviour
    {
        public static QteEnemyManager instance;
        public QteEnemy test;

        private bool _tag = false;
        private float _timer;
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_tag) return;
            if (_timer >= 3f)
            {
                test.QteEnemyHideComponent.RemoveListEnemiesHide();
                _tag = true;
            }
        }
    }
}
