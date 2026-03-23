using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.Ststems;
using PlayableAd.Systems;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.NPC
{
    /// <summary>
    /// 자동 채굴 NPC.
    /// 근처 광석 탐색(예약) → 이동 → 채굴 → OreDepositZone 이동 → 반복
    /// </summary>
    [RequireComponent(typeof(NPCNavigator))]

    public class MinerNPC : NPCStateMachine, IWorker
    {
        [Header("채굴 설정")]
        [SerializeField]
        private float miningInterval = 0.5f;
        [SerializeField]
        private float searchRange = 10f;
        [SerializeField]
        private LayerMask oreLayer;


        public NPCRole Role => NPCRole.Miner;
        public bool IsWorking => CurrentState != NPCState.Idle;

        private NPCNavigator _navigator;
        private OreItem _reservedOre;
        private float _miningTimer;
        private OreDepositZone _depositZone;

        private void Awake()
        {
            _navigator = GetComponent<NPCNavigator>();
        }

        public void AssignTarget(Transform target)
        {

        }

        public void StartWork()
        {
            TransitionTo(NPCState.MovingToTarget);
        }
        public void StopWork()
        {
            ReleaseReservation();
            _navigator.Stop();
            TransitionTo(NPCState.Idle);
        }
        /// <summary>ShopZone에서 스폰 시 호출</summary>
        public void Init(OreDepositZone depositZone)
        {
            if (depositZone == null)
            {
                return;
            }
            _depositZone = depositZone;

        }

        public void StartWorkDelayed()
        {
            StartCoroutine(StartWorkNextFrame());
        }

        private IEnumerator StartWorkNextFrame()
        {
            yield return null; 
            StartWork();
        }
        // -------------------------------------------------------
        // 상태 머신
        // -------------------------------------------------------


        protected override void OnStateEnter(NPCState state)
        {

            switch (state) 
            {
                case NPCState.MovingToTarget:
                    FindAndMoveToOre();
                    break;
            }
        }

        protected override void OnStateUpdate(NPCState state)
        {
            if (state == NPCState.Working)
                UpdateMining();
        }

        // -------------------------------------------------------
        // 광석 탐색 & 예약
        // -------------------------------------------------------

        private void FindAndMoveToOre()
        {

            var hits = Physics.OverlapSphere(transform.position, searchRange, oreLayer);


            OreItem bestOre = null;
            float bestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<OreItem>(out var ore))
                {
                    continue;
                }
                if (!ore.IsCollectable)
                {
                    continue;
                }
                if (OreReservationSystem.IsReserved(ore))
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, ore.transform.position);
                if (dist >= bestDist) continue;

                bestDist = dist;
                bestOre = ore;
            }

            if (bestOre == null)
            {
                Invoke(nameof(RetryFind), 1f);
                return;
            }

            // 예약 성공 시 이동
            if (!OreReservationSystem.TryReserve(bestOre, this))
            {

                Invoke(nameof(RetryFind), 0.5f);
                return;
            }

            _reservedOre = bestOre;
            _navigator.MoveTo(
                _reservedOre.transform.position,
                onArrived: () => {

                    TransitionTo(NPCState.Working);
        });
        }

        private void RetryFind() => FindAndMoveToOre();

        // -------------------------------------------------------
        // 채굴
        // -------------------------------------------------------

        private void UpdateMining()
        {
            // 광석이 사라진 경우 재탐색
            if (_reservedOre == null || !_reservedOre.IsCollectable)
            {
                ReleaseReservation();
                TransitionTo(NPCState.MovingToTarget);
                return;
            }

            _miningTimer += Time.deltaTime;
            if (_miningTimer < miningInterval) return;

            _miningTimer = 0f;

            if (!TryHarvestOre(_reservedOre)) return;

            ReleaseReservation();
            TransitionTo(NPCState.MovingToTarget);
        }
        private bool TryHarvestOre(OreItem ore)
        {
            if (!ore.IsCollectable) return false;

            // ResourceManager 수량 추가 없이 시각적으로만 처리
            ore.HarvestByNPC();

            // ItemMover로 DepositZone을 향해 날아가게 함
            if (ore.TryGetComponent<ItemMover>(out var mover))
            {
                mover.MoveTo(
                    _depositZone.transform.position,
                    onComplete: () =>
                    {
                        // 도착 시 DepositZone에 광석 추가
                        _depositZone.AddOreDirectly();

                        // 광석 오브젝트 풀 반환
                        ObjectPool.Instance.Return(ore.gameObject);
                    });
            }
            else
            {
                // ItemMover 없으면 즉시 처리
                _depositZone.AddOreDirectly();
                ObjectPool.Instance.Return(ore.gameObject);
            }

            return true;
        }
        private void ReleaseReservation()
        {
            OreReservationSystem.Release(_reservedOre);
            _reservedOre = null;
            _miningTimer = 0f;

        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, searchRange);
        }
#endif
    }

}