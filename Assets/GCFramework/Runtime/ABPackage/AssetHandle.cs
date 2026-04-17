using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GCFramework.Runtime.ABPackage
{
    public abstract class AbstractAssetHandle
    {
        public object Key { get; protected set; }
        public AssetHolder Holder { get; protected set; }

        protected AbstractAssetHandle(object key, AssetHolder holder)
        {
            Key = key;
            Holder = holder;
        }

        public abstract void Release();
        public abstract void ReleaseInstance();
    }

    public class AssetHandle<TObject> : AbstractAssetHandle
    {
        public TObject Result { get; private set; }
        public AsyncOperationHandle<TObject> OperationHandle;

        public AssetHandle(object key, AssetHolder holder, AsyncOperationHandle<TObject> handle)
            : base(key, holder)
        {
            OperationHandle = handle;
            Result = handle.Result;
        }

        public override void Release()
        {
            Addressables.Release(OperationHandle);
        }

        public override void ReleaseInstance()
        {
            Addressables.ReleaseInstance(OperationHandle);
        }
    }

    public class AssetGroupHandlePlus<TObject> : AbstractAssetHandle
    {
        public IList<TObject> Results;
        public AsyncOperationHandle<IList<TObject>> OperationHandle;

        public AssetGroupHandlePlus(object key, AssetHolder holder, AsyncOperationHandle<IList<TObject>> operationHandle) 
            : base(key, holder)
        {
            OperationHandle = operationHandle;
            Results = operationHandle.Result;
        }

        public override void Release()
        {
            Addressables.Release(OperationHandle);
        }

        public override void ReleaseInstance()
        {
            Addressables.ReleaseInstance(OperationHandle);
        }
    }
    
    
}