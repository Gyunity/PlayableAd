using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.Player;
using PlayableAd.UI;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("최대 수량")]
        [SerializeField] private int maxOreCount = 100;

        [Header("MAX 표시")]
        [SerializeField] private MaxIndicator maxIndicator;
        [SerializeField] private Transform maxFollowTarget;

        [Header("이미지색상")]
        [SerializeField]
        private Image border;

        private readonly List<GameObject> _depositedOres = new();
        private readonly Queue<System.Action> _pendingNPCOres = new();
        private bool _isDepositing = false;
        private int _spawnIndex = 0;

        public int DepositedCount => _depositedOres.Count;
        public bool HasOres => _depositedOres.Count > 0;
        public bool IsFull => _depositedOres.Count >= maxOreCount;

        public event System.Action<int> OnOresDeposited;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            border.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

            if (IsFull)
            {
                Debug.Log("[OreDepositZone] 광물 MAX — 반납 불가");
                return;
            }

            var inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            int oreCount = ResourceManager.Instance.GetAmount(ResourceType.Ore);
            if (oreCount <= 0) return;

            int canDeposit = Mathf.Min(oreCount, maxOreCount - _depositedOres.Count);
            if (canDeposit <= 0) return;

            StartCoroutine(DepositRoutine(inventory, oreCount));
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            border.color = Color.white;
        }
        private IEnumerator DepositRoutine(PlayerInventory inventory, int oreCount)
        {
            _isDepositing = true;

            ResourceManager.Instance.TryRemove(ResourceType.Ore, oreCount);

            var oreObjects = inventory.TakeOresForDeposit();

            for (int i = 0; i < oreObjects.Count; i++)
            {
                if (IsFull)
                {
                    // 넘치는 광물은 풀 반환
                    for (int j = i; j < oreObjects.Count; j++)
                        if (oreObjects[j] != null)
                            ObjectPool.Instance.Return(oreObjects[j]);
                    break;
                }

                var go = oreObjects[i];
                var targetPos = CalculatePosition(_spawnIndex);

                if (go == null) continue;

                go.transform.SetParent(null);

                if (go.TryGetComponent<ItemMover>(out var mover))
                {
                    var captured = go;
                    mover.MoveTo(targetPos, onComplete: () =>
                    {
                        go.transform.rotation = depositRoot.rotation;
                        // 도착 후 수집 불가 처리 + 리스트 추가
                        if (captured.TryGetComponent<OreItem>(out var ore))
                            ore.SetCollectable(false);
                        _depositedOres.Add(captured);
                        PlayableAd.Utils.CoroutineRunner.Instance.Run(
                    ScalePopUtility.ScalePop(go));
                    });
                }
                else
                {
                    go.transform.position = targetPos;
                    go.transform.rotation = depositRoot.rotation;

                    if (go.TryGetComponent<OreItem>(out var ore))
                        ore.SetCollectable(false);
                    _depositedOres.Add(go);
                    PlayableAd.Utils.CoroutineRunner.Instance.Run(
                    ScalePopUtility.ScalePop(go));
                }
                SoundManager.Instance.PlayOreDeposit();

                _spawnIndex++;
                yield return new WaitForSeconds(0.08f); 
            }

            // 이동 완료 대기 (마지막 오브젝트 이동 시간)
            yield return new WaitForSeconds(0.3f);

            OnOresDeposited?.Invoke(oreCount);

            if (IsFull) maxIndicator?.Show();
            OnOresDeposited?.Invoke(oreCount);
            GameEventBus.Publish_OreDeposited(oreCount);
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

            if (!IsFull) maxIndicator?.Hide();

            return ore;
        }

        public void ClearDepositedOres()
        {
            foreach (var go in _depositedOres)
                if (go != null) ObjectPool.Instance.Return(go);

            _depositedOres.Clear();
            _spawnIndex = 0;
            maxIndicator?.Hide();
        }
        public void AddOreDirectly()
        {
            if (_isDepositing)
            {
                _pendingNPCOres.Enqueue(() => ProcessNPCOre());
                return;
            }

            ProcessNPCOre();
        }
        private IEnumerator ProcessPendingNPCOres()
        {
            while (_pendingNPCOres.Count > 0)
            {
                if (IsFull)
                {
                    _pendingNPCOres.Clear();
                    break;
                }

                var action = _pendingNPCOres.Dequeue();
                action?.Invoke();
                yield return new WaitForSeconds(0.05f);
            }
        }
        private void ProcessNPCOre()
        {
            if (IsFull)
            {
                maxIndicator?.Show();
                return;
            }

            SpawnDepositedOre(_spawnIndex);
            _spawnIndex++;

            if (IsFull) maxIndicator?.Show();

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