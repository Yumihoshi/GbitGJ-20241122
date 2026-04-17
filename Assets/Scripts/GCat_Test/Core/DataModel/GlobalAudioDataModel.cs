using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace GCat_Test.Core.DataModel
{
    [CreateAssetMenu(menuName = "DataModel/音频数据配置")]
    public class GlobalAudioDataModel : GlobalConfig<GlobalAudioDataModel>
    {
        public List<AudioInfo> audioInfoList = new List<AudioInfo>();

        public AudioClip GetClip(string tag)
        {
            return audioInfoList.Find(x => x.audioTag == tag)?.clip;
        }
    }

    [System.Serializable]
    public class AudioInfo
    {
        public string audioTag;
        public AudioClip clip;
    }
}