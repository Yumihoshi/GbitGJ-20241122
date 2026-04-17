using System.Collections.Generic;
using GCFramework.Singleton;
using UnityEngine;

namespace GCFramework.Runtime.ABPackage
{
    public class AssetHolderManager : SingletonNoMono<AssetHolderManager>
    {
        private readonly List<AssetHolder> _assetHolderList = new();

        public AssetHolder CreateAssetHolder()
        {
            AssetHolder assetHolder = new AssetHolder();
            _assetHolderList.Add(assetHolder);
            return assetHolder;
        }

        public void RemoveAssetHolder(AssetHolder assetHolder)
        {
            if (!_assetHolderList.Contains(assetHolder))
            {
#if UNITY_EDITOR
                Debug.LogError("该AssetHolder没有被Manager管理！");
#endif
                return;
            }

            _assetHolderList.Remove(assetHolder);
            assetHolder.ReleaseAllHandleCache();
            assetHolder.ReleaseAllHandleIns();
        }

        public void ClearAllHolder()
        {
            foreach (var assetHolder in _assetHolderList)
            {
                assetHolder.ReleaseAllHandleCache();
                assetHolder.ReleaseAllHandleIns();
            }
            _assetHolderList.Clear();
        }
        
    }
}