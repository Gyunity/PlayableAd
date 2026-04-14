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
        [SerializeField] private float handcuffColumnSpacing = 0.35f;
        [SerializeField] private int handcuffMaxPerColumn = 999;
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
            _navigator.Speed = 0;
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
        // 수갑 수령
        // -------------------------------------------------------

        public void ReceiveHandcuff(GameObject handcuffGo)
        {
            int currentIndex = _handcuffs.Count;
            _handcuffs.Add(handcuffGo);

            var worldTargetPos = GetNextHandcuffPosition();

            if (handcuffGo.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(worldTargetPos, onComplete: () =>
                {
                    if (handcuffGo == null || !handcuffGo.activeSelf) return;

                    handcuffGo.transform.SetParent(transform);
                    handcuffGo.transform.localPosition = GetHandcuffLocalPosition(currentIndex);
                    handcuffGo.transform.localRotation = Quaternion.identity;
                });
            else
            {
                handcuffGo.transform.SetParent(transform);
                handcuffGo.transform.localPosition = GetHandcuffLocalPosition(currentIndex);
                handcuffGo.transform.localRotation = Quaternion.identity;
            }

            _handcuffCount++;

            if (_handcuffCount >= maxHandcuff)
            {
                _factory.StopPickupForWorker();
                TransitionTo(NPCState.Selling);
            }
        }

        private Vector3 GetNextHandcuffPosition()
        {
            int colIndex = _handcuffCount / handcuffMaxPerColumn;
            int rowIndex = _handcuffCount % handcuffMaxPerColumn;

            var basePos = transform.position
                        + transform.forward * (handcuffColumnSpacing * (colIndex + 1));

            return new Vector3(
                basePos.x,
                transform.position.y + handcuffStackY * rowIndex,
                basePos.z);
        }

        private Vector3 GetHandcuffLocalPosition(int index)
        {
            int colIndex = index / handcuffMaxPerColumn;
            int rowIndex = index % handcuffMaxPerColumn;

            return new Vector3(
                0f,
                handcuffStackY * rowIndex,
                handcuffColumnSpacing * (colIndex + 1));
        }

        public List<GameObject> TakeAllHandcuffs()
        {
            var list = new List<GameObject>(_handcuffs);
            list.Reverse(); 

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
                case NPCState.Idle:
                    _navigator.Speed = 0;

                    break;
                case NPCState.MovingToFactory:
                    MoveToFactory();
                    _navigator.Speed = 5;

                    break;
                case NPCState.Selling:
                    MoveToSales();
                    _navigator.Speed = 5;
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
        // 공장 이동
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
                _navigator.Speed = 0;
                _factory.StartPickupForWorker(this);
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
                Debug.Log("[WorkerNPC] 판매존 도착 → 수갑 내려놓기");
                _navigator.Speed = 0;

                var handcuffs = TakeAllHandcuffs();
                StartCoroutine(DepositAndWait(handcuffs));
            });
        }

        private IEnumerator DepositAndWait(List<GameObject> handcuffs)
        {
            yield return StartCoroutine(_salesZone.DepositHandcuffs(handcuffs));

            yield return new WaitUntil(() =>
                _salesZone.DepositedCount == 0 ||
                PrisonManager.Instance.IsFull);

            Debug.Log("[WorkerNPC] 판매 완료 → 공장으로 복귀");
            TransitionTo(NPCState.MovingToFactory);
        }
    }
}