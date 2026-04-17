// 探测技能
// 条件：在地面上且CD为0

using Aki_Test.Player;
using GCat_Test.Core.Manager;
using Tsuki_Test.Enemy;
using UnityEngine;

namespace Aki_Test.Skills
{
    public class Detect : Skill
    {
    
        private new void Awake()
        {
            base.Awake();
        
            skillName = "Detect";
            skillShape = SkillShape.Circle;
            isMainSkill = true;
        }

        public override void Engage()
        {
            if (!_player.GetComponent<PlayerController>().isGrounded) return;
            base.Engage();
            if (!isReady) return;
            var detected = Physics2D.OverlapCircleAll(_player.transform.position, skillRadius, LayerMask.GetMask("QteEnemyOuterCollider"));
            foreach (var i in detected)
            {
                // todo - 敌人全部取消隐身状态
                if (i.CompareTag("Enemy"))
                {
                    // 遍历所有连接的敌人，解除伪装状态
                    var qteEnemy = i.gameObject.GetComponent<QteEnemy>();
                    qteEnemy.QteEnemyHideComponent.RemoveListEnemiesHide();
                }
            }
            AudioManager.Ins.PlaySfx("detect");
            isReady = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_player.transform.position, skillRadius);
        }
    }
}
