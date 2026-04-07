using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.UI;
using PlayableAd.Utils;
using System.Collections;
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
        private bool _collectingFromCoinZone = false;

        private StackSlotManager _slotManager;

        [SerializeField] private List<GameObject> _coinStack = new();
        [SerializeField] private List<GameObject> _oreStack = new();

        [Header("수갑 스택 설정 (플레이어 앞)")]
        [SerializeField] private Transform handcuffStackRoot;
        [SerializeField] private float handcuffStackY = 0.15f;
        [SerializeField] private float handcuffColumnSpacing = 0.4f;
        [SerializeField] private int handcuffMaxPerColumn = 5;

        [Header("MAX 표시")]
        [SerializeField] private MaxIndicator oreMaxIndicator;
        [Header("수갑 고정 방향")]
        [SerializeField] private Vector3 fixedHandcuffDirection = Vector3.forward; // 월드 +Z 고정

        public List<GameObject> GetOreStack() => _oreStack;
        public List<GameObject> GetCoinStack() => _coinStack;
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

            var current = ResourceManager.Instance.GetAmount(ResourceType.Ore);
            var max = ResourceManager.Instance.GetMax(ResourceType.Ore);
            if (current >= max || amount == 0)
                oreMaxIndicator?.Show();
        }

        
        private void SpawnOreVisual()
        {
            var worldTargetPos = _slotManager.GetNextOrePosition();
            var go = ObjectPool.Instance.Get(
                                     orePoolKey, transform.position, Quaternion.identity);
            if (go == null) return;

            if (go.TryGetComponent<OreItem>(out var ore))
                ore.SetCollectable(false);

            _oreStack.Add(go);
            int index = _oreStack.Count - 1;

            go.transform.position = worldTargetPos;
            go.transform.SetParent(transform);
            go.transform.localPosition = _slotManager.GetLocalPositionForOre(index);
            go.transform.localRotation = Quaternion.identity;

            StartCoroutine(ScalePopRoutine(go));
        }
        private IEnumerator ScalePopRoutine(GameObject go)
        {
            if (go == null) yield break;

            float duration = 0.25f;
            float elapsed = 0f;
            var startScale = Vector3.one * 1.5f;
            var endScale = Vector3.one * 1.0f;

            go.transform.localScale = startScale;

            while (elapsed < duration)
            {
                if (go == null || !go.activeSelf) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                go.transform.localScale = Vector3.Lerp(startScale, endScale, easedT);

                yield return null;
            }

            if (go != null && go.activeSelf)
                go.transform.localScale = endScale;
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
            oreMaxIndicator?.Hide();
        }

        public List<GameObject> TakeOresForDeposit()
        {
            var list = new List<GameObject>();

            foreach (var go in _oreStack)
            {
                if (go == null || !go.activeSelf) continue;
                go.transform.SetParent(null);
                list.Add(go);
            }

            list.Reverse();

            _oreStack.Clear();
            _slotManager.ClearOres();
            oreMaxIndicator?.Hide();

            return list;
        }

        // -------------------------------------------------------
        // 동전 스택
        // -------------------------------------------------------

        private void HandleCurrencyChanged(int balance)
        {
            if (_collectingFromCoinZone) return;

            int targetCount = balance / coinSpawnPer;
            while (_coinStack.Count < targetCount) SpawnCoinVisual();
            while (_coinStack.Count > targetCount) RemoveLastCoin();
        }


        private void SpawnCoinVisual()
        {
            var targetPos = _slotManager.GetNextCoinPosition();
            var go = ObjectPool.Instance.Get(
                                coinPoolKey, transform.position, transform.rotation);
            if (go == null) return;

            _coinStack.Add(go);

            int currentIndex = _coinStack.Count - 1;

            if (go.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos, onComplete: () =>
                {
                    if (go == null || !go.activeSelf) return;
                    go.transform.SetParent(transform);
                    go.transform.localPosition = _slotManager.GetLocalPositionForCoin(currentIndex);
                    go.transform.localRotation = Quaternion.identity;

                    StartCoroutine(ScalePopUtility.ScalePop(go));
                });
            else
            {
                go.transform.SetParent(transform);
                go.transform.localPosition = _slotManager.GetLocalPositionForCoin(currentIndex);
                go.transform.localRotation = Quaternion.identity;
                StartCoroutine(ScalePopUtility.ScalePop(go));
            }
        }
        public void SetCollectingFromCoinZone(bool value)
        {
            _collectingFromCoinZone = value;
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
        public void SyncCoinStack()
        {
            int targetCount = CurrencyManager.Instance.Balance / coinSpawnPer;

            while (_coinStack.Count < targetCount) SpawnCoinVisual();
            while (_coinStack.Count > targetCount) RemoveLastCoin();
        }
        // -------------------------------------------------------
        // 수갑 스택 (플레이어 앞)
        // -------------------------------------------------------

        public void ReceiveHandcuff(GameObject handcuffGo)
        {
            var worldTargetPos = GetNextHandcuffPosition();
            int currentIndex = _handcuffStack.Count;

            if (handcuffGo.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(worldTargetPos, onComplete: () =>
                {
                    if (handcuffGo == null || !handcuffGo.activeSelf) return;

                    handcuffGo.transform.SetParent(transform);
                    handcuffGo.transform.localPosition = GetHandcuffLocalPosition(currentIndex);
                    handcuffGo.transform.localRotation = Quaternion.identity;
                    _handcuffStack.Add(handcuffGo);

                    StartCoroutine(ScalePopUtility.ScalePop(handcuffGo));
                    SoundManager.Instance.PlayHandcuffDeposit();

                    if (_handcuffStack.Count == 1)
                        GameEventBus.Publish_HandcuffReceived();
                });
            else
            {
                handcuffGo.transform.SetParent(transform);
                handcuffGo.transform.localPosition = GetHandcuffLocalPosition(currentIndex);
                handcuffGo.transform.localRotation = Quaternion.identity;
                _handcuffStack.Add(handcuffGo);

                StartCoroutine(ScalePopUtility.ScalePop(handcuffGo));
                SoundManager.Instance.PlayHandcuffDeposit();

                if (_handcuffStack.Count == 1)
                    GameEventBus.Publish_HandcuffReceived();
            }
        }

        public Vector3 GetNextHandcuffPosition()
        {
            int colIndex = _handcuffCount / handcuffMaxPerColumn;
            int rowIndex = _handcuffCount % handcuffMaxPerColumn;
            _handcuffCount++;

            var basePos = handcuffStackRoot.position
                        + transform.forward * (handcuffColumnSpacing * (colIndex + 1));

            return new Vector3(
                basePos.x,
                handcuffStackRoot.position.y + handcuffStackY * rowIndex,
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

            list.Reverse();

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