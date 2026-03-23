using PlayableAd.Core;
using PlayableAd.Items;
using PlayableAd.Player;
using PlayableAd.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 판매 수익이 이 존에 쌓입니다.
    /// 플레이어가 진입하면 쌓인 돈을 전부 수령합니다.
    /// </summary>
    public class CoinZone : MonoBehaviour
    {
        [Header("코인 스폰 설정")]
        [SerializeField] private Transform coinStackRoot;   // 코인 쌓일 기준점
        [SerializeField] private float columnSpacing = 0.4f;
        [SerializeField] private float rowSpacing = 0.4f;
        [SerializeField] private float stackYSpacing = 0.12f;
        [SerializeField] private int maxPerColumn = 2;     // 몇 개마다 Y축으로 올라갈지

        [SerializeField] private int coinPer = 100; // 100원당 코인 1개

        private readonly List<GameObject> _coinStack = new();
        private int _pendingAmount = 0; // 아직 수령 안 된 금액

        private void OnEnable()
        {
            GameEventBus.OnItemSold += HandleItemSold;
        }

        private void OnDisable()
        {
            GameEventBus.OnItemSold -= HandleItemSold;
        }

        private void HandleItemSold(int amount)
        {
            // 고객 1명 = 100원 = 코인 1개 시각화
            int revenue = amount / 4 * 100; // 수갑 4개 = 100원
            AddPendingCoins(revenue);
        }

        public void AddPendingCoins(int amount)
        {
            _pendingAmount += amount;

            // 100원마다 코인 1개 스폰
            int targetCount = _pendingAmount / coinPer;
            while (_coinStack.Count < targetCount)
                SpawnCoinVisual(_coinStack.Count);
        }

        private void SpawnCoinVisual(int index)
        {
            var pos = CalculatePosition(index);
            var go = ObjectPool.Instance.Get("Coin", coinStackRoot.position, Quaternion.identity);
            if (go == null) return;

            _coinStack.Add(go);

            if (go.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(pos);
            else
                go.transform.position = pos;
        }

        private Vector3 CalculatePosition(int index)
        {
            int col = index % 2;
            int row = index / 2;
            int height = row / maxPerColumn;
            int depth = row % maxPerColumn;

            float xOffset = (col == 0 ? -1f : 1f) * (columnSpacing * 0.5f);
            float zOffset = depth * rowSpacing;
            float yOffset = height * stackYSpacing;

            return coinStackRoot.position
                 + coinStackRoot.right * xOffset  // 두 줄 X 간격
                 + coinStackRoot.forward * zOffset  // 앞뒤 Z 간격
                 + Vector3.up * yOffset;  // Y축 높이
        }
       
        // -------------------------------------------------------
        // 플레이어 진입 → 돈 수령
        // -------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (_pendingAmount <= 0) return;

            StartCoroutine(CollectRoutine(other.GetComponent<PlayerInventory>()));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            StopAllCoroutines();
        }

        private IEnumerator CollectRoutine(PlayerInventory inventory)
        {
            while (_coinStack.Count > 0 && _pendingAmount > 0)
            {
                var go = _coinStack[^1];
                _coinStack.RemoveAt(_coinStack.Count - 1);

                if (go != null)
                    ObjectPool.Instance.Return(go);

                // 100원 수령
                _pendingAmount -= coinPer;
                CurrencyManager.Instance.Add(coinPer);

                yield return new WaitForSeconds(0.05f);
            }

            _pendingAmount = 0;
        }
    }
}