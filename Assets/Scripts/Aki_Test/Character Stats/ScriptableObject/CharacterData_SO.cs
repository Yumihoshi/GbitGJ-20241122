using UnityEngine;

namespace Aki_Test.Character_Stats.ScriptableObject
{
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "角色数据/创建新数据")]
    public class CharacterData_SO : UnityEngine.ScriptableObject
    {
        [Header("血量（San值）")]
        public int maxHealth;
        public int currentHealth;
        [Header("血量自动流失")]
        public int healthLossNum; // 单次血量流失数
        public int defaultHealthLossNum; // 默认的血量流失数
        public float healthLossTime; // 血量流失速度
        public float defaultHealthLossTime; // 默认的血量流失速度
    }
}
