// 清除范围内QTE敌人

using Aki_Test.Player;
using GCat_Test.Core.Manager;
using Tsuki_Test.Enemy;
using Unity.Mathematics;
using UnityEngine;

namespace Aki_Test.Skills
{
    public class Eliminate : Skill
    {
        private QteEnemy _enemy;
        private PlayerController _playerController;
        private bool _isEliminating;

        protected new void Awake()
        {
            base.Awake();
            _playerController = _player.GetComponent<PlayerController>();
            skillName = "Eliminate";
            skillShape = SkillShape.Target;
            isMainSkill = false;
        }


        protected new void Update()
        {
            base.Update();
            if (_playerController.isGrounded && _isEliminating)
            {
                _playerController.canMove = true;
                _playerController.isBraking = true;
                _isEliminating = false;
            }
            
        }

        public void GetEnemy(QteEnemy enemy)
        {
            _enemy = enemy;
        }
    
        public override void Engage()
        {
            if (_enemy == null)
            {
                // todo - 提示：找不到可攻击对象
                Debug.Log("无攻击对象");
                return;
            }

            base.Engage();
            // todo - 清除怪物
            // 当enemy前后节点都已经被清除时，清除当前怪物
            _isEliminating = true;
            if (_enemy.next != null)
            {
                if (_enemy.prev != null)
                {
                    // 未击杀前面的敌人
                    Debug.Log("前面还有敌人残余，释放失败");
                    return;
                }
                
                if (!_enemy.GetQteable())
                {
                    Debug.Log("该敌人不能被QTE，释放失败");
                    return;
                }

                if (_enemy.QteEnemyHideComponent.IsHide)
                {
                    Debug.Log("该敌人处于伪装状态，释放失败");
                    return;
                }
                var bounceVector = _enemy.GetBounceVector(_player.transform);
                _playerController.canMove = false;
                _playerController.Bounce(new Vector2(bounceVector.x,
                    bounceVector.y));
            }
            else if (_enemy.prev == null && _enemy.next == null) 
            { 
                _playerController.canMove = false;
                _playerController.Bounce(_playerController.bounceForce, false);
            }
            _enemy.KillSelf();
            AudioManager.Ins.PlaySfx("eliminate");
            isReady = false;
        }
    }
}