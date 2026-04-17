using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;

namespace GCFramework.Runtime.ABPackage
{
    public enum AssetType
    {
        Audio,
        Model,
        Prefab,
        Sprite,
        Scene,
        ConfigFile,
        ScriptableObject
    }
    
    public static class AssetPath
    {
        public static readonly string MainFilePath = "Assets/GameAssets/";
        public static readonly string AudioRootPath = "Audios/";
        public static readonly string ModelRootPath = "Models/";
        public static readonly string PrefabRootPath = "Prefabs/";
        public static readonly string SpriteRootPath = "Sprites/";
        public static readonly string SceneRootPath = "Scenes/";
        public static readonly string ConfigFileRootPath = "Configs/";
        public static readonly string ScriptableObjectPath = "ScriptableObjects/";

        #region 后缀名定义

        // Sprite
        public static readonly string SpriteExpNamePng = ".png";
        public static readonly string SpriteExpNameJpg = ".jpg";
        
        // Audio
        public static readonly string AudioExpNameWav = ".wav";
        public static readonly string AudioExpNameMp3 = ".mp3";
        
        // Video
        public static readonly string VideoExpNameMkv = ".mkv";
        public static readonly string VideoExpNameMp4 = ".mp4";
        public static readonly string VideoExpNameMov = ".mov";
        
        // 3DModel
        public static readonly string ModelExpNameFbx = ".fbx";
        
        // 文本文件
        public static readonly string TextExpNameTxt = ".txt";
        public static readonly string TextExpNameJson = ".json";
        
        // 场景
        public static readonly string SceneExpNameUnity = ".unity";
        
        // Prefab
        public static readonly string PrefabExpNamePrefab = ".prefab";
        
        // ScriptableObject
        public static readonly string SoExpNameAsset = ".asset";

        #endregion

#if UNITY_EDITOR
        [MenuItem("Tools/快速创建资源文件夹")]
        public static void CreateAssetDirectory()
        {
            string assetDirectoryPath = Application.dataPath + "/GameAssets";
            if (Directory.Exists(assetDirectoryPath))
            {
                Debug.LogWarning("资源文件夹已存在！Path: " + assetDirectoryPath);
                return;
            }
            
            Directory.CreateDirectory(assetDirectoryPath);
            Directory.CreateDirectory(GetFilePath(AudioRootPath));
            Directory.CreateDirectory(GetFilePath(ModelRootPath));
            Directory.CreateDirectory(GetFilePath(PrefabRootPath));
            Directory.CreateDirectory(GetFilePath(SpriteRootPath));
            Directory.CreateDirectory(GetFilePath(SceneRootPath));
            Directory.CreateDirectory(GetFilePath(ConfigFileRootPath));
            Directory.CreateDirectory(GetFilePath(ScriptableObjectPath));
            AssetDatabase.Refresh();
        }

        public static string GetFilePath(string fileName)
        {
            return Application.dataPath + "/GameAssets/" + fileName;
        }
        
#endif

        public static string GetAssetPath(string key, AssetType type)
        {
            switch (type)
            {
                case AssetType.Audio:
                    return MainFilePath + AudioRootPath + key + AudioExpNameWav;
                case AssetType.Model:
                    return MainFilePath + ModelRootPath + key + ModelExpNameFbx;
                case AssetType.Prefab:
                    return MainFilePath + PrefabRootPath + key + PrefabExpNamePrefab;
                case AssetType.Sprite:
                    return MainFilePath + SpriteRootPath + key + SpriteExpNamePng;
                case AssetType.Scene:
                    return MainFilePath + SceneRootPath + key + SceneExpNameUnity;
                case AssetType.ConfigFile:
                    return MainFilePath + ConfigFileRootPath + key + TextExpNameJson;
                case AssetType.ScriptableObject:
                    return MainFilePath + ScriptableObjectPath + key + SoExpNameAsset;
                default:
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"加载的资源类型不存在！");    
#endif
                    return "";
                }
            }
        }

    }
    
    /// <summary>
    /// 资源拥有者，管理某个模块的资源
    /// TODO:: 可以实现一个对象池，管理资源实例化的对象
    /// </summary>
    public class AssetHolder
    {
        // 资源缓存
        private readonly List<AbstractAssetHandle> _assetHandleCaches = new();
        // 加载的实例缓存
        private readonly List<AssetHandle<GameObject>> _assetInsHandleList = new();

        private bool InAssetCaches(object key, out AbstractAssetHandle handle)
        {
            handle = _assetHandleCaches.Find(x => x.Key == key);
            return handle != null;
        }

        private void HandleResultNullExceptionMsg(object path, AsyncOperationHandle operationHandle)
        {
            if (operationHandle.Status == AsyncOperationStatus.Failed || operationHandle.Result == null)
            {
                throw new OperationException($"加载资源失败！加载路径: {path}，资源结果: {operationHandle.Result}");
            }
        }

        /// <summary>
        /// 检测参数Key是否为空，并且尝试获取资源缓存
        /// </summary>
        /// <param itemName="key"></param>
        /// <param itemName="handleCache"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool CheckParamValidAndTryGetCache(object key, out AbstractAssetHandle handleCache)
        {
            if (key == null)
                throw new ArgumentNullException($"资源名为空！");
            if (InAssetCaches(key, out handleCache))
                return true;
            return false;
        }

        #region 指定类型加载 & 快捷实例化

        /// <summary>
        /// 指定类型单个资源同步加载
        /// </summary>
        /// <param itemName="key">资源名</param>
        /// <param itemName="type"></param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        public AssetHandle<TObject> LoadAssetByType<TObject>(string key, AssetType type)
        {
            return LoadAsset<TObject>(AssetPath.GetAssetPath(key, type));
        }

        /// <summary>
        /// 指定类型单个资源异步加载
        /// </summary>
        /// <param itemName="key">资源名</param>
        /// <param itemName="type"></param>
        /// <param itemName="completed"></param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<AssetHandle<TObject>> LoadAssetByTypeAsync<TObject>(string key, AssetType type,
            Action<TObject> completed)
        {
           var ah = await LoadAssetAsync(AssetPath.GetAssetPath(key, type), completed);
           return ah;
        }

        /// <summary>
        /// 指定类型多个资源异步加载
        /// </summary>
        /// <param itemName="key">资源名</param>
        /// <param itemName="type"></param>
        /// <param itemName="allCompleted"></param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<AssetGroupHandlePlus<TObject>> LoadGroupAssetByTypeAsync<TObject>(string key,
            AssetType type, Action<TObject> allCompleted)
        {
            var agh = await LoadAssetGroupAsync(key, allCompleted);
            return agh;
        }

        /// <summary>
        /// 实例化单个资源同步
        /// </summary>
        /// <param itemName="key">资源名</param>
        /// <param itemName="pos"></param>
        /// <param itemName="rota"></param>
        /// <param itemName="parent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public AssetHandle<GameObject> InstantiateAssetQuick(string key, Vector3 pos = default, Quaternion rota = default, Transform parent = default)
        {
            if (key == string.Empty)
                throw new ArgumentNullException($"资源名为空！");
            
            return InstantiateAsset(AssetPath.GetAssetPath(key, AssetType.Prefab), pos, rota, parent);
        }

        /// <summary>
        /// 实例化单个资源异步
        /// </summary>
        /// <param itemName="key"></param>
        /// <param itemName="pos"></param>
        /// <param itemName="rota"></param>
        /// <param itemName="parent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async UniTask<AssetHandle<GameObject>> InstantiateAssetQuickAsync(string key, Vector3 pos = default,
            Quaternion rota = default, Transform parent = null)
        {
            if(key == string.Empty)
                throw new ArgumentNullException($"资源名为空！");
            var handleIns = await InstantiateAssetAsync(AssetPath.GetAssetPath(key, AssetType.Prefab), pos, rota, parent);
            return handleIns;
        }

        #endregion
        
        
        #region 同步加载，仅加载单个资源

        /// <summary>
        /// 同步加载资源，单个
        /// </summary>
        /// <param itemName="key">资源路径</param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public AssetHandle<TObject> LoadAsset<TObject>(object key)
        {
            if (CheckParamValidAndTryGetCache(key, out var handleCache))
                return (AssetHandle<TObject>)handleCache;

            var handle = Addressables.LoadAssetAsync<TObject>(key);
            handle.WaitForCompletion();
            HandleResultNullExceptionMsg(key, handle);

            AssetHandle<TObject> ah = new AssetHandle<TObject>(key, this, handle);
            _assetHandleCaches.Add(ah);
            return ah;
        }

        #endregion

        #region 异步加载资源

        /// <summary>
        /// 异步加载资源，单个
        /// </summary>
        /// <param itemName="key">资源路径</param>
        /// <param itemName="completed"></param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<AssetHandle<TObject>> LoadAssetAsync<TObject>(object key, Action<TObject> completed)
        {
            if (CheckParamValidAndTryGetCache(key, out var handleCache))
                return (AssetHandle<TObject>)handleCache;

            var handle = Addressables.LoadAssetAsync<TObject>(key);

            handle.Completed += operationHandle =>
            {
                completed?.Invoke(operationHandle.Result);
            };
            
            await handle.ToUniTask();
            HandleResultNullExceptionMsg(key, handle);

            AssetHandle<TObject> ah = new AssetHandle<TObject>(key, this, handle);
            _assetHandleCaches.Add(ah);
            return ah;
        }

        /// <summary>
        /// 异步加载多个资源
        /// </summary>
        /// <param itemName="key">资源路径</param>
        /// <param itemName="allCompleted"></param>
        /// <typeparam itemName="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<AssetGroupHandlePlus<TObject>> LoadAssetGroupAsync<TObject>(object key,
            Action<TObject> allCompleted)
        {
            if (CheckParamValidAndTryGetCache(key, out var handleCache))
                return (AssetGroupHandlePlus<TObject>)handleCache;

            var handleGroup = Addressables.LoadAssetsAsync(key, allCompleted);
            await handleGroup.ToUniTask();

            AssetGroupHandlePlus<TObject> agh = new AssetGroupHandlePlus<TObject>(key, this, handleGroup);
            _assetHandleCaches.Add(agh);
            return agh;
        }

        #endregion

        #region 实例化资源

        /// <summary>
        /// 异步实例化资源，单个
        /// </summary>
        /// <param itemName="key">资源路径</param>
        /// <param itemName="pos"></param>
        /// <param itemName="rota"></param>
        /// <param itemName="parent"></param>
        /// <returns></returns>
        private async UniTask<AssetHandle<GameObject>> InstantiateAssetAsync(object key, Vector3 pos = default, Quaternion rota = default, Transform parent = null)
        {
            var handleIns = Addressables.InstantiateAsync(key, pos, rota, parent);
            await handleIns.ToUniTask();
            HandleResultNullExceptionMsg(key, handleIns);
            AssetHandle<GameObject> ah = new AssetHandle<GameObject>(key, this, handleIns);
            _assetInsHandleList.Add(ah);
            return ah;
        }

        /// <summary>
        /// 同步实例化资源，单个
        /// </summary>
        /// <param itemName="key">资源路径</param>
        /// <param itemName="pos"></param>
        /// <param itemName="rota"></param>
        /// <param itemName="parent"></param>
        /// <returns></returns>
        private AssetHandle<GameObject> InstantiateAsset(object key, Vector3 pos = default, Quaternion rota = default, Transform parent = null)
        {
           var handleIns = Addressables.InstantiateAsync(key, pos, rota, parent);
           handleIns.WaitForCompletion();
           HandleResultNullExceptionMsg(key, handleIns);
           AssetHandle<GameObject> ah = new AssetHandle<GameObject>(key, this, handleIns);
           _assetInsHandleList.Add(ah);
           return ah;
        }

        #endregion

        #region 资源释放

        /// <summary>
        /// 释放指定类型单个的资源
        /// </summary>
        /// <param itemName="resName">资源名字</param>
        /// <param itemName="type"></param>
        public void ReleaseHandleCache(string resName, AssetType type)
        {
            string path = AssetPath.GetAssetPath(resName, type);
            if (!InAssetCaches(path, out var handleCache))
            {
#if UNITY_EDITOR
                Debug.LogError($"资源不存在，无法释放！");
#endif
                return;
            }

            _assetHandleCaches.Remove(handleCache);
            handleCache.Release();
        }

        /// <summary>
        /// 释放指定Handle单个资源
        /// </summary>
        /// <param itemName="handle"></param>
        /// <typeparam itemName="TObject"></typeparam>
        public void ReleaseHandleCache<TObject>(AssetHandle<TObject> handle)
        {
            if (_assetHandleCaches.Contains(handle))
                _assetHandleCaches.Remove(handle);
            else
            {
#if UNITY_EDITOR
                Debug.LogError("该资源不在缓存中！");
#endif
            }
            handle.Release();
        }

        /// <summary>
        /// 释放所有资源缓存
        /// </summary>
        public void ReleaseAllHandleCache()
        {
            foreach (var handleCache in _assetHandleCaches)
            {
                handleCache.Release();
            }
            _assetHandleCaches.Clear();
        }

        /// <summary>
        /// 释放指定资源实例，它会将同一预制体资源的实例全部销毁
        /// </summary>
        /// <param itemName="resName"></param>
        public void ReleaseHandleIns(string resName)
        {
            string path = AssetPath.GetAssetPath(resName, AssetType.Prefab);
            List<AssetHandle<GameObject>> handleInsList = new();
            foreach (var handleIns in _assetInsHandleList)
            {
                if (handleIns.Key.ToString() == path)
                {
                    handleInsList.Add(handleIns);
                    _assetInsHandleList.Remove(handleIns);
                }
            }
            if (handleInsList.Count <= 0)
            {
#if UNITY_EDITOR
                Debug.LogError("没有可以释放的资源实例！");
#endif
                return;
            }

            foreach (var handleIns in handleInsList)
                handleIns.ReleaseInstance();
        }

        /// <summary>
        /// 释放所有实例化的资源
        /// </summary>
        public void ReleaseAllHandleIns()
        {
            foreach (var handleIns in _assetInsHandleList)
            {
                handleIns.ReleaseInstance();
            }
            _assetInsHandleList.Clear();
        }

        #endregion

    }
}