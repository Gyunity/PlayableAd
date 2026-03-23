using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.Player;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 플레이어가 진입하면 보유 광물을 전부 내려놓습니다.
    /// 내려놓은 광물은 두 줄로 배치됩니다.
    /// </summary>
    public class OreDepositZone : MonoBehaviour
    {
        [Header("광물 배치 설정")]
        [SerializeField] private Transform depositRoot;
        [SerializeField] private float columnSpacing = 0.4f;
        [SerializeField] private float rowSpacing = 0.4f;
        [SerializeField] private float stackYSpacing = 0.4f;

        private readonly List<GameObject> _depositedOres = new();

        private bool _isDepositing = false;
        private readonly Queue<System.Action> _pendingNPCOres = new();

        public int DepositedCount => _depositedOres.Count;
        public bool HasOres => _depositedOres.Count > 0;

        public event System.Action<int> OnOresDeposited;
        private int _spawnIndex = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            int oreCount = ResourceManager.Instance.GetAmount(ResourceType.Ore);
            if (oreCount <= 0) return;

            StartCoroutine(DepositRoutine(inventory, oreCount));
        }

        private IEnumerator DepositRoutine(PlayerInventory inventory, int oreCount)
        {
            _isDepositing = true;

            ResourceManager.Instance.TryRemove(ResourceType.Ore, oreCount);
            inventory.ClearOreStack();

            for (int i = 0; i < oreCount; i++)
            {
                SpawnDepositedOre(_spawnIndex);
                _spawnIndex++;
                yield return new WaitForSeconds(0.05f);
            }

            OnOresDeposited?.Invoke(oreCount);
            _isDepositing = false;
            StartCoroutine(ProcessPendingNPCOres());

        }

        private void SpawnDepositedOre(int index)
        {
            var pos = CalculatePosition(index);
            var go = ObjectPool.Instance.Get("Ore", pos, Quaternion.identity);
            if (go == null) return;

            if (go.TryGetComponent<ICollectable>(out var ore))
                ore.SetCollectable(false);

            _depositedOres.Add(go);
        }

        private Vector3 CalculatePosition(int index)
        {
          

            int col = index % 1;
            int row = index / 1;
            int height = row / 2;
            int depth = row % 2;

            float xOffset = (col == 0 ? -1f : 1f) * (columnSpacing * 0.5f);
            float zOffset = depth * rowSpacing;
            float yOffset = height * stackYSpacing;

            return depositRoot.position
                 + depositRoot.right * xOffset  // 두 줄 X 간격
                 + depositRoot.forward * zOffset  // 앞뒤 Z 간격
                 + Vector3.up * yOffset;  // Y축 높이
        }

        public GameObject TakeOneOre()
        {
            if (_depositedOres.Count == 0) return null;

            var ore = _depositedOres[^1];
            _depositedOres.RemoveAt(_depositedOres.Count - 1);
            _spawnIndex = Mathf.Max(0, _spawnIndex - 1);
            return ore;
        }

        public void ClearDepositedOres()
        {
            foreach (var go in _depositedOres)
                if (go != null) ObjectPool.Instance.Return(go);

            _depositedOres.Clear();
            _spawnIndex = 0;
        }
        public void AddOreDirectly()
        {
            if (_isDepositing)
            {
                _pendingNPCOres.Enqueue(() => ProcessNPCOre());
                Debug.Log($"[OreDepositZone] NPC 광물 대기 중 (큐: {_pendingNPCOres.Count})");
                return;
            }

            ProcessNPCOre();
        }
        private IEnumerator ProcessPendingNPCOres()
        {
            while (_pendingNPCOres.Count > 0)
            {
                var action = _pendingNPCOres.Dequeue();
                action?.Invoke();
                yield return new WaitForSeconds(0.05f); // 자연스러운 딜레이
            }
        }
        private void ProcessNPCOre()
        {
            SpawnDepositedOre(_spawnIndex);
            _spawnIndex++;
            OnOresDeposited?.Invoke(1);
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (depositRoot == null) return;

            Gizmos.color = Color.green;
            for (int i = 0; i < 10; i++)
            {
                var pos = CalculatePosition(i);
                Gizmos.DrawWireCube(pos, Vector3.one * 0.2f);
            }
        }
#endif

    }
}