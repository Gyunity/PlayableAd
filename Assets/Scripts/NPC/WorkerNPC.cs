using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.Systems;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.NPC
{
    [RequireComponent(typeof(NPCNavigator))]
    public class WorkerNPC : NPCStateMachine, IWorker
    {
        [Header("수갑 설정")]
        [SerializeField] private float handcuffStackY = 0.15f;
        [SerializeField] private int maxHandcuff = 10;

        public NPCRole Role => NPCRole.FactoryWorker;
        public bool IsWorking => CurrentState != NPCState.Idle;

        public bool CanPickupHandcuff =>
            _handcuffCount < maxHandcuff && !PrisonManager.Instance.IsFull;

        public int HandcuffCount => _handcuffCount;


        private NPCNavigator _navigator;
        private readonly List<GameObject> _handcuffs = new();
        private int _handcuffCount;
        private HandcuffFactory _factory;
        private SalesZone _salesZone;

        private void Awake()
        {
            _navigator = GetComponent<NPCNavigator>();
        }

        public void AssignTarget(Transform target) { }

        public void StartWork() => TransitionTo(NPCState.MovingToFactory);

        public void StopWork()
        {
            _factory?.StopPickupForWorker();
            _navigator.Stop();
            TransitionTo(NPCState.Idle);
        }

        public void Init(HandcuffFactory factory, SalesZone salesZone)
        {
            _factory = factory;
            _salesZone = salesZone;
            Debug.Log($"[WorkerNPC] Init 완료 / factory: {factory.name} / salesZone: {salesZone.name}");
        }

        // -------------------------------------------------------
        // 수갑 수령 — HandcuffFactory.PickupRoutine에서 호출
        // -------------------------------------------------------

        public void ReceiveHandcuff(GameObject handcuffGo)
        {
            _handcuffs.Add(handcuffGo);

            var targetPos = GetNextHandcuffPosition();


            if (handcuffGo.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos, onComplete: () =>
                {

                    handcuffGo.transform.SetParent(transform);
                    handcuffGo.transform.localRotation = Quaternion.identity; 
                });
            else
            {
                handcuffGo.transform.position = targetPos;
                handcuffGo.transform.SetParent(transform);
                handcuffGo.transform.localRotation = Quaternion.identity;

            }
            _handcuffCount++;

            // 10개 채워지면 판매존으로 이동
            if (_handcuffCount >= maxHandcuff)
            {
                _factory.StopPickupForWorker(); // 픽업 중단
                TransitionTo(NPCState.Selling);
            }
        }
        private Vector3 GetNextHandcuffPosition()
        {
            int colIndex = _handcuffCount;
            int rowIndex = _handcuffCount;

            // NPC 앞 방향으로 컬럼 배치
            var basePos = transform.position
                        + transform.forward;

            return new Vector3(
                basePos.x,
                transform.position.y + handcuffStackY * rowIndex,
                basePos.z);
        }
        public List<GameObject> TakeAllHandcuffs()
        {
            var list = new List<GameObject>(_handcuffs);

            foreach (var go in _handcuffs)
                if (go != null) go.transform.SetParent(null);

            _handcuffs.Clear();
            _handcuffCount = 0;

            return list;
        }

     
        // -------------------------------------------------------
        // 상태 머신
        // -------------------------------------------------------

        protected override void OnStateEnter(NPCState state)
        {
            Debug.Log($"[WorkerNPC] 상태 전환 → {state}");

            switch (state)
            {
                case NPCState.MovingToFactory:
                    MoveToFactory();
                    break;

                case NPCState.Selling:
                    MoveTo​Sales();
                    break;
            }
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
        // 공장 이동 — 도착 시 직접 픽업 요청
        // -------------------------------------------------------

        private void MoveToFactory()
        {
            if (PrisonManager.Instance.IsFull)
            {
                TransitionTo(NPCState.Idle);
                return;
            }

            Vector3 destination;
            if (_factory.TryGetComponent<Collider>(out var col))
                destination = col.bounds.center;
            else
                destination = _factory.transform.position;

            Debug.Log($"[WorkerNPC] 공장으로 이동: {destination}");

            _navigator.MoveTo(destination, onArrived: () =>
            {
                Debug.Log("[WorkerNPC] 공장 도착 → 픽업 요청");

                _factory.StartPickupForWorker(this);

                // 수갑 10개 찰 때까지 대기
                StartCoroutine(WaitUntilFull());
            });
        }

        private IEnumerator WaitUntilFull()
        {
            while (_handcuffCount < maxHandcuff)
            {
                if (PrisonManager.Instance.IsFull)
                {
                    _factory.StopPickupForWorker();
                    TransitionTo(NPCState.Idle);
                    yield break;
                }
                yield return new WaitForSeconds(0.2f);
            }
            // ReceiveHandcuff에서 이미 Selling으로 전환됨
        }

        // -------------------------------------------------------
        // 판매존 이동
        // -------------------------------------------------------

        private void MoveToSales()
        {
            Vector3 destination;
            if (_salesZone.TryGetComponent<Collider>(out var col))
                destination = col.bounds.center;
            else
                destination = _salesZone.transform.position;

            Debug.Log($"[WorkerNPC] 판매존으로 이동: {destination}");

            _navigator.MoveTo(destination, onArrived: () =>
            {
                Debug.Log("[WorkerNPC] 판매존 도착 → 판매 시작");

                _salesZone.StartSellForWorker(this);

                StartCoroutine(SellRoutine());
            });
        }


        private IEnumerator SellRoutine()
        {
            yield return new WaitUntil(() => !_salesZone.IsSelling);

            yield return null;
            yield return new WaitUntil(() => !_salesZone.IsSelling);

            Debug.Log("[WorkerNPC] 판매 완료 → 공장으로 복귀");
            TransitionTo(NPCState.MovingToFactory);
        }

    
    }
}