using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayableAd.Player
{
    /// <summary>
    /// 플레이어 등 뒤 스택 슬롯 위치를 계산합니다.
    /// 돈 컬럼 → 광석 컬럼 순서로 플레이어 뒤쪽에 배치됩니다.
    ///
    /// 배치 구조 (플레이어 뒤 방향 기준):
    ///   [광석N] ← [광석1] [돈N] ← [돈1] [Player]
    ///   
    /// Y축 스택: 같은 컬럼 내에서 아이템이 위로 쌓입니다.
    /// 컬럼 순서: 플레이어에서 멀어질수록 뒤에 새 컬럼이 생성됩니다.
    /// </summary>
    public class StackSlotManager : MonoBehaviour
    {
        [Header("스택 기준점")]
        [SerializeField]
        private Transform playerRoot; // 플레이어 Transform

        [Header("컬럼 간격 (플레이어 뒤 방향)")]
        [SerializeField]
        private float columnSpacing = 0.45f; // 컬럼 사이 Z 간격

        [Header("Y축 스택 간격")]
        [SerializeField]
        private float coinStackY = 0.12f;  // 돈 아이템 높이 간격
        [SerializeField]
        private float oreStackY = 0.18f;  // 광석 아이템 높이 간격

        [Header("컬럼당 최대 높이 (개수)")]
        [SerializeField] private int maxPerColumn = 10; // 10개마다 새 컬럼

        private int _coinCount;
        private int _oreCount;

        // 바닥 기준 높이 (플레이어와 같은 Y)
        private float BaseY => playerRoot.position.y;

        /// <summary>현재 돈 개수 기준 다음 돈 아이템이 놓일 월드 위치 반환</summary>
        public Vector3 GetNextCoinPosition()
        {
            int colIndex = _coinCount / maxPerColumn;          // 몇 번째 컬럼
            int rowIndex = _coinCount % maxPerColumn;          // 컬럼 내 몇 번째
            _coinCount++;
            return CalculatePosition(colIndex, rowIndex, coinStackY);
        }

        /// <summary>현재 광석 개수 기준 다음 광석 아이템이 놓일 월드 위치 반환</summary>
        public Vector3 GetNextOrePosition()
        {
            // 광석 컬럼은 돈 컬럼 뒤에서 시작
            int coinColumns = Mathf.Max(1, Mathf.CeilToInt((float)_coinCount / maxPerColumn));
            int colOffset = coinColumns; // 돈 컬럼 수만큼 뒤에서 시작

            int colIndex = _oreCount / maxPerColumn;
            int rowIndex = _oreCount % maxPerColumn;
            _oreCount++;
            return CalculatePosition(colOffset + colIndex, rowIndex, oreStackY);
        }

        /// <summary>돈/광석 제거 시 카운트 감소 (스택 재정렬은 RefreshStack 호출)</summary>
        public void RemoveCoin(int count = 1) => _coinCount = Mathf.Max(0, _coinCount - count);
        public void RemoveOre(int count = 1) => _oreCount = Mathf.Max(0, _oreCount - count);

        public void ClearCoins() => _coinCount = 0;
        public void ClearOres() => _oreCount = 0;

        public int CoinCount => _coinCount;
        public int OreCount => _oreCount;

        // -------------------------------------------------------
        // 내부 계산
        // -------------------------------------------------------

        private Vector3 CalculatePosition(int colIndex, int rowIndex, float stackYSpacing)
        {
            var backDir = -playerRoot.forward;
            var basePos = playerRoot.position
                        + backDir * (columnSpacing * (colIndex + 1)); // 컬럼 위치

            return new Vector3(
                basePos.x,
                BaseY + stackYSpacing * rowIndex, // Y축 누적
                basePos.z
            );
        }
    }
}