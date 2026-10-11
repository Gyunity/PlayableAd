
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


namespace PlayableAd.Utils
{

    /// <summary>
    /// 오브젝트 풀 관리 매니저
    /// </summary>
    public class ObjectPool : Singleton<ObjectPool>
    {
        [System.Serializable]
        public class PoolEntry
        {
            public string key;
            public GameObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 50;
        }

        [SerializeField]
        private List<PoolEntry> poolEntries;

        private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, string> _instanceKeyMap = new();

        protected override void OnAwake()
        {
            foreach (var entry in poolEntries)
                CreatePool(entry);
        }

        private void CreatePool(PoolEntry entry)
        {
            var prefab = entry.prefab;
            var key = entry.key;

            _pools[key] = new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var go = Instantiate(prefab);
                    _instanceKeyMap[go] = key;
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    go.transform.SetParent(null);
                    go.SetActive(false);
                }, actionOnDestroy: go => _instanceKeyMap.Remove(go),
                defaultCapacity: entry.defaultCapacity,
                maxSize: entry.maxSize
                );
        }


        public GameObject Get(string key, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"[ObjectPool] '{key}' 풀이 없습니다.");
                return null;
            }

            var go = pool.Get();
            go.transform.SetPositionAndRotation(position, rotation);
            return go;
        }

        public void Return(GameObject go)
        {
            if (!_instanceKeyMap.TryGetValue(go, out var key))
            {
                Debug.LogWarning("[ObjectPool] 풀에서 생성되지 않은 오브젝트입니다. Destroy 처리합니다.");
                Destroy(go);
                return;
            }

            _pools[key].Release(go);
        }



    }
}