using Cysharp.Threading.Tasks;
using NFramework;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class PoolManager : SingletonMono<PoolManager>
    {
        [Serializable]
        public class PoolData
        {
            public PooledObject prefab;
            public int initPoolSize;
        }

        private Dictionary<PooledObject, Pool> _poolDic = new Dictionary<PooledObject, Pool>();

        [SerializeField] private List<PoolData> _poolDatas = new List<PoolData>();

#if UNITY_EDITOR
        [Button]
        private void SetupPools()
        {
            transform.DestroyAllChildren();
            foreach (var data in _poolDatas)
            {
                var pool = Pool.CreatePool(false, true, data.initPoolSize, data.prefab);
                pool.transform.SetParent(transform);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public async UniTask Init()
        {
            foreach (var pool in GetComponentsInChildren<Pool>())
            {
                _poolDic.Add(pool.ObjectToPool, pool);
                pool.InitializePool();
                await UniTask.Yield();
            }
        }

        public Pool GetPool(PooledObject key)
        {
            if (_poolDic.ContainsKey(key))
            {
                return _poolDic[key];
            }
            else
            {
                var pool = Pool.CreatePool(false, true, 1, key);
                pool.transform.SetParent(transform);
                _poolDic.Add(pool.ObjectToPool, pool);
                pool.InitializePool();
                return pool;
            }
        }

        public void ReturnAllToPool()
        {
            foreach (var pair in _poolDic)
            {
                pair.Value.ReturnAllToPool();
            }
        }
    }
}
