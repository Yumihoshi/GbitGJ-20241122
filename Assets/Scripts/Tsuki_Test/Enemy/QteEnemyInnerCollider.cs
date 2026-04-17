// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/24 11:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Tsuki_Test.Enemy
{
    public class QteEnemyInnerCollider : MonoBehaviour
    {
        [CanBeNull] public event Action<Collider2D> OnTriggerEnter2DEvent;
        [CanBeNull] public event Action<Collider2D> OnTriggerExit2DEvent;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTriggerEnter2DEvent?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnTriggerExit2DEvent?.Invoke(other);
        }
    }
}
