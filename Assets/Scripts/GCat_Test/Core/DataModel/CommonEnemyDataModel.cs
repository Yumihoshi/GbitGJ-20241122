using UnityEngine;

namespace GCat_Test.Core.DataModel
{
    [CreateAssetMenu(menuName = "DataModel/普通敌人数据配置", fileName = "EnemyData_")]
    public class CommonEnemyDataModel : ScriptableObject
    {
        public float moveSpeed = 5f;
        public float rotTime = 0.3f;
        public Vector2 idleTime = new Vector2(3, 5);
        public float attackInterval = 0.5f;
    }
}