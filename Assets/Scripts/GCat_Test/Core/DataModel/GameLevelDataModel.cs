using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GCat_Test.Core.DataModel
{
    public enum LevelId
    {
        MainScene,
        Level1,
        Level2,
        Level3
    }
    
    [CreateAssetMenu(menuName = "DataModel/游戏关卡数据配置", fileName = "GameLevelDataModel")]
    public class GameLevelDataModel : ScriptableObject
    {
        public List<LevelField> levelFields;

        public string GetSceneName(LevelId levelId)
        {
            return levelFields.Find(x => x.levelId == levelId)?.sceneName;
        }
    }

    [System.Serializable]
    public class LevelField
    {
        /// <summary>
        /// 关卡Id，使用枚举方便在Inspector上设置
        /// </summary>
        public LevelId levelId;
        /// <summary>
        /// 场景名字
        /// </summary>
        [ValueDropdown("GetSceneNameFromEditor")]
        public string sceneName;

#if UNITY_EDITOR
        private List<string> GetSceneNameFromEditor()
        {
            return EditorBuildSettings.scenes.ToList().ConvertAll(x =>
            {
                var result = x.path.Split('/')[^1];
                string[] strs = result.Split('.');
                return strs[0];
            });
        }
#endif
    }
    
}