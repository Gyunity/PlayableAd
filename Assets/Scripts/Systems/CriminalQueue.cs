using PlayableAd.Core;
using PlayableAd.NPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// SalesZone 앞 범죄자 대기 줄을 관리합니다.
    /// 맨 앞 범죄자만 판매 위치로 이동하고
    /// 한 명이 떠나면 다음 범죄자가 앞으로 이동합니다.
    /// </summary>
    public class CriminalQueue : MonoBehaviour
    {
        [Header("대기 줄 설정")]
        [SerializeField] private List<Transform> queueSlots;   // 대기 위치들
        [SerializeField] private float moveInterval = 0.3f; // 줄 이동 딜레이

        [Header("범죄자 프리팹")]
        [SerializeField] private GameObject criminalPrefab;
        [SerializeField] private int initialCount = 2;   // 시작 시 범죄자 수

        [Header("씬 참조 (CriminalNPC에 주입)")]
        [SerializeField] private Transform salesPoint;
        [SerializeField] private Transform prisonEntry;
        [SerializeField] private SalesZone salesZone;
        [SerializeField] private CoinZone coinZone;
        [Header("수용소 자리")]
        [SerializeField] private List<Transform> prisonSeats;
        private int _nextSeatIndex = 0;

        private readonly List<CriminalNPC> _queue = new();
        private bool _isProcessing = false;


        private void OnEnable()
        {
            GameEventBus.OnPrisonFull += HandlePrisonFull;
        }

        private void OnDisable()
        {
            GameEventBus.OnPrisonFull -= HandlePrisonFull;
        }

        private void Start()
        {
            // 초기 범죄자 스폰
            for (int i = 0; i < Mathf.Min(initialCount, queueSlots.Count); i++)
                SpawnCriminal(i);

            StartCoroutine(StartFirstCriminalNextFrame());

        }
        private IEnumerator StartFirstCriminalNextFrame()
        {
            yield return null; // NavMesh 배치 대기
            if (_queue.Count > 0 && !PrisonManager.Instance.IsFull)
                _queue[0].ProceedToSales();
        }
        private void SpawnCriminal(int slotIndex)
        {
            if (slotIndex >= queueSlots.Count) return;

            var go = Instantiate(criminalPrefab,
                          queueSlots[slotIndex].position,
                          queueSlots[slotIndex].rotation);
            var npc = go.GetComponent<CriminalNPC>();
            if (npc == null) return;

            Transform seat = _nextSeatIndex < prisonSeats.Count
                ? prisonSeats[_nextSeatIndex++]  
                : null;

            Debug.Log($"[CriminalQueue] 범죄자 스폰 → 자리: {seat?.name ?? "없음"} / 다음 인덱스: {_nextSeatIndex}");

            npc.Init(this, salesPoint, prisonEntry, seat, salesZone, coinZone);
            _queue.Add(npc);
        }

        /// <summary>범죄자가 판매 완료 후 수용소로 떠날 때 호출</summary>
        public void OnCriminalDeparting(CriminalNPC criminal)
        {
            _queue.Remove(criminal);

            // 줄 앞으로 이동 + 새 범죄자 추가
            StartCoroutine(AdvanceQueue());
        }

        private IEnumerator AdvanceQueue()
        {
            yield return new WaitForSeconds(moveInterval);

            // 남은 범죄자들을 한 칸씩 앞으로
            for (int i = 0; i < _queue.Count; i++)
            {
                if (i >= queueSlots.Count) break;
                var npc = _queue[i];
                npc.GetComponent<NPCNavigator>()?.MoveTo(queueSlots[i].position);
            }

            yield return new WaitForSeconds(moveInterval);

            // 새 범죄자 맨 뒤에 추가
            if (!PrisonManager.Instance.IsFull)
            {
                int newSlot = _queue.Count;
                if (newSlot < queueSlots.Count)
                    SpawnCriminal(newSlot);
            }

            yield return new WaitForSeconds(moveInterval);

            // 맨 앞 범죄자 판매 시작
            if (_queue.Count > 0 && !PrisonManager.Instance.IsFull)
                _queue[0].ProceedToSales();
        }

        private void HandlePrisonFull()
        {
            // 수용소 만석 — 모든 범죄자 정지
            foreach (var criminal in _queue)
                criminal.StopWork();

            Debug.Log("[CriminalQueue] 수용소 만석 — 모든 범죄자 정지");
        }
        public void OnCriminalEntered()
        {
            Debug.Log($"[CriminalQueue] 수감자 입장 완료 / 현재 인덱스: {_nextSeatIndex}");

        }

    }
}
