using System;
using UnityEngine;

namespace GCat_Test.Core
{
    public class CommonEnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _rb2D;
        private float _currentMoveSpeed;
        
        private void Awake()
        {
            _rb2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Mathf.Abs(_rb2D.velocity.x) > 0.1f)
            {
                FlipXHandle(_rb2D.velocity.x);
            }
        }

        private void FlipXHandle(float velX)
        {
            var scale = transform.localScale;
            scale.x = velX > 0 ? 1 : -1;
            transform.localScale = scale;
        }
        
        public void SetRbVelocity(Vector2 direction, float moveSpeed = 0)
        {
            SetVelocity(direction.normalized * moveSpeed);
        }

        private void SetVelocity(Vector2 vel)
        {
            _rb2D.velocity = vel;
        }
    }
}