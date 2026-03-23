using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Player
{
    [RequireComponent(typeof(StackSlotManager))]
    public class PlayerInventory : MonoBehaviour
    {
        [Header("아이템 프리팹 키 (ObjectPool)")]
        [SerializeField] private string coinPoolKey = "Coin";
        [SerializeField] private string orePoolKey = "Ore";

        [Header("판매 후 돈 스폰 설정")]
        [SerializeField] private int coinSpawnPer = 10;

        private StackSlotManager _slotManager;

        [SerializeField] private List<GameObject> _coinStack = new();
        [SerializeField] private List<GameObject> _oreStack = new();

        [Header("수갑 스택 설정 (플레이어 앞)")]
        [SerializeField] private Transform handcuffStackRoot;
        [SerializeField] private float handcuffStackY = 0.15f;
        [SerializeField] private float handcuffColumnSpacing = 0.4f;
        [SerializeField] private int handcuffMaxPerColumn = 5;

        private readonly List<GameObject> _handcuffStack = new();
        private int _handcuffCount = 0;

        private void Awake()
        {
            _slotManager = GetComponent<StackSlotManager>();
        }

        private void OnEnable()
        {
            GameEventBus.OnOreHarvested += HandleOreHarvested;
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            GameEventBus.OnOreHarvested -= HandleOreHarvested;
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        // -------------------------------------------------------
        // 광석 스택
        // -------------------------------------------------------

        private void HandleOreHarvested(int amount)
        {
            for (int i = 0; i < amount; i++)
                SpawnOreVisual();
        }

        private void SpawnOreVisual()
        {
            var targetPos = _slotManager.GetNextOrePosition();
            var go = ObjectPool.Instance.Get(
                                orePoolKey,
                                transform.position,
                                transform.rotation); 

            if (go == null) return;

            if (go.TryGetComponent<OreItem>(out var ore))
                ore.SetCollectable(false);

            _oreStack.Add(go);

            if (go.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos, onComplete: () =>
                {
                    if (go != null && go.activeSelf)
                    {
                        go.transform.SetParent(transform, true);
                        go.transform.localRotation = Quaternion.identity; 
                    }
                });
            else
            {
                go.transform.position = targetPos;
                go.transform.SetParent(transform, true);
                go.transform.localRotation = Quaternion.identity;
            }
        }



        

        public void ClearOreStack()
        {
            foreach (var go in _oreStack)
            {
                if (go == null || !go.activeSelf) continue;
                go.transform.SetParent(null);
                ObjectPool.Instance.Return(go);
            }
            _oreStack.Clear();
            _slotManager.ClearOres();
        }

        public void RefreshOreStack()
        {
            _slotManager.ClearOres();
            foreach (var go in _oreStack)
            {
                var targetPos = _slotManager.GetNextOrePosition();
                if (go.TryGetComponent<ItemMover>(out var mover))
                    mover.MoveTo(targetPos);
                else
                    go.transform.localPosition = targetPos - transform.position;
            }
        }

        // -------------------------------------------------------
        // 동전 스택
        // -------------------------------------------------------

        private void HandleCurrencyChanged(int balance)
        {
            int targetCount = balance / coinSpawnPer;
            while (_coinStack.Count < targetCount) SpawnCoinVisual();
            while (_coinStack.Count > targetCount) RemoveLastCoin();
        }

        private void SpawnCoinVisual()
        {
            var targetPos = _slotManager.GetNextCoinPosition();
            var go = ObjectPool.Instance.Get(
                                coinPoolKey,
                                transform.position,
                                transform.rotation); 

            if (go == null) return;

            _coinStack.Add(go);

            if (go.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos, onComplete: () =>
                {
                    if (go != null && go.activeSelf)
                    {
                        go.transform.SetParent(transform, true);
                        go.transform.localRotation = Quaternion.identity; 
                    }
                });
            else
            {
                go.transform.position = targetPos;
                go.transform.SetParent(transform, true);
                go.transform.localRotation = Quaternion.identity;
            }
        }

        private void RemoveLastCoin()
        {
            if (_coinStack.Count == 0) return;

            var go = _coinStack[^1];
            _coinStack.RemoveAt(_coinStack.Count - 1);
            _slotManager.RemoveCoin();

            if (go == null || !go.activeSelf) return;
            go.transform.SetParent(null);
            ObjectPool.Instance.Return(go);
        }

        public void RefreshCoinStack()
        {
            _slotManager.ClearCoins();
            foreach (var go in _coinStack)
            {
                var targetPos = _slotManager.GetNextCoinPosition();
                if (go.TryGetComponent<ItemMover>(out var mover))
                    mover.MoveTo(targetPos);
                else
                    go.transform.localPosition = targetPos - transform.position;
            }
        }

        // -------------------------------------------------------
        // 수갑 스택 (플레이어 앞)
        // -------------------------------------------------------

        public void ReceiveHandcuff(GameObject handcuffGo)
        {
            var targetPos = GetNextHandcuffPosition();

            if (handcuffGo.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos, onComplete: () =>
                {
                    handcuffGo.transform.SetParent(transform, true);
                    handcuffGo.transform.localRotation = Quaternion.identity; 
                    _handcuffStack.Add(handcuffGo);
                });
            else
            {
                handcuffGo.transform.position = targetPos;
                handcuffGo.transform.SetParent(transform, true);
                handcuffGo.transform.localRotation = Quaternion.identity;
                _handcuffStack.Add(handcuffGo);
            }
        }

        public Vector3 GetNextHandcuffPosition()
        {
            int colIndex = _handcuffCount / handcuffMaxPerColumn;
            int rowIndex = _handcuffCount % handcuffMaxPerColumn;
            _handcuffCount++;

            // 항상 플레이어 forward 방향 사용
            var basePos = handcuffStackRoot.position
                        + transform.forward * (handcuffColumnSpacing * (colIndex + 1));

            return new Vector3(
                basePos.x,
                handcuffStackRoot.position.y + handcuffStackY * rowIndex,
                basePos.z);
        }

        public void RegisterHandcuff(GameObject go)
        {
            _handcuffStack.Add(go);
        }

        public void RemoveHandcuffs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_handcuffStack.Count == 0) break;

                var go = _handcuffStack[^1];
                _handcuffStack.RemoveAt(_handcuffStack.Count - 1);
                _handcuffCount = Mathf.Max(0, _handcuffCount - 1);

                if (go == null || !go.activeSelf) continue;
                go.transform.SetParent(null);
                ObjectPool.Instance.Return(go);
            }
        }
        public List<GameObject> TakeAllHandcuffs()
        {
            var list = new List<GameObject>(_handcuffStack);

            // 스택 초기화 (오브젝트는 SalesZone이 관리)
            foreach (var go in _handcuffStack)
                if (go != null) go.transform.SetParent(null);

            _handcuffStack.Clear();
            _handcuffCount = 0;

            return list;
        }
        // -------------------------------------------------------
        // 전체 초기화
        // -------------------------------------------------------

        public void ClearAll()
        {
            ClearOreStack();

            foreach (var go in _coinStack)
            {
                if (go == null || !go.activeSelf) continue;
                go.transform.SetParent(null);
                ObjectPool.Instance.Return(go);
            }
            _coinStack.Clear();
            _slotManager.ClearCoins();

            RemoveHandcuffs(_handcuffStack.Count);
        }

        public int OreVisualCount => _oreStack.Count;
        public int CoinVisualCount => _coinStack.Count;
        public int HandcuffVisualCount => _handcuffStack.Count;
    }
}