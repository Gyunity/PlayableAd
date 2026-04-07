using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Systems;
using PlayableAd.Utils;
using System.Collections;
using TMPro;
using UnityEngine;

namespace PlayableAd.NPC
{
    /// <summary>
    /// 범죄자 NPC.
    /// 대기 → 판매 위치 이동 → 수갑 구매 → 수용소 이동 → 입장
    /// </summary>
    /// 
    [RequireComponent(typeof(NPCNavigator))]
    public class CriminalNPC : MonoBehaviour
    {
        public enum CriminalState
        {
            Waiting,        // 대기 줄에서 대기
            MovingToSales,  // 판매 위치로 이동
            Buying,         // 수갑 구매 중 (수갑 부족 시 대기)
            MovingToPrison, // 수용소로 이동
            InPrison        // 수용소 입장 완료
        }


        [Header("씬 참조")]
        private SalesZone _salesZone;
        private CoinZone _coinZone;

        [Header("구매 설정")]
        [SerializeField] private int requiredHandcuffs = 4;  // 필요 수갑 수
        [SerializeField] private int price = 100; // 지불 금액

        [Header("수갑 부족 표시")]
        [SerializeField] private GameObject remainingDisplay;  // 부족량 표시 오브젝트
        [SerializeField] private TextMeshPro remainingText;    // 부족량 텍스트

        [Header("씬 참조 (CriminalQueue에서 주입)")]
        private CriminalQueue _queue;
        private Transform _salesPoint;
        private Transform _prisonEntry;
        private Transform _prisonSeat;
        public NPCRole Role => NPCRole.Prisoner;
        public bool IsWorking => _criminalState != CriminalState.Waiting;

        private NPCNavigator _navigator;
        private CriminalState _criminalState = CriminalState.Waiting;
        private int _purchasedCount = 0; // 현재까지 구매한 수갑 수
        private Camera _cam;

        private void Awake()
        {
            _navigator = GetComponent<NPCNavigator>();
            _cam = Camera.main;
            remainingDisplay?.SetActive(false);
        }
        public void Init(CriminalQueue queue, Transform salesPoint,
                   Transform prisonEntry, Transform prisonSeat,
                   SalesZone salesZone, CoinZone coinZone) 
        {
            _queue = queue;
            _salesPoint = salesPoint;
            _prisonEntry = prisonEntry;
            _prisonSeat = prisonSeat;
            _salesZone = salesZone;
            _coinZone = coinZone;
        }
        public void AssignTarget(Transform target) { }

        public void StartWork()
        {
            _criminalState = CriminalState.Waiting;
        }

        public void StopWork()
        {
            _navigator.Stop();
            StopAllCoroutines();
        }

        // -------------------------------------------------------
        // 대기 줄에서 차례가 됐을 때 호출
        // -------------------------------------------------------

        public void ProceedToSales()
        {
            if (PrisonManager.Instance.IsFull) return;

            _criminalState = CriminalState.MovingToSales;
            _navigator.MoveTo(_salesPoint.position, onArrived: () =>
            {
                _criminalState = CriminalState.Buying;
                ShowRemaining(requiredHandcuffs - _purchasedCount);
                StartCoroutine(BuyRoutine());
            });
        }

        // -------------------------------------------------------
        // 수갑 구매 루틴
        // -------------------------------------------------------

        private IEnumerator BuyRoutine()
        {
            while (_purchasedCount < requiredHandcuffs)
            {
                if (PrisonManager.Instance.IsFull)
                {
                    StopBuying();
                    yield break;
                }

                if (_salesZone.DepositedCount <= 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }

                _salesZone.RemoveOneDepositedHandcuff();

                var container = ServiceLocator.Get<IResourceContainer>();
                container.TryRemove(ResourceType.Handcuff, 1);

                _purchasedCount++;
                UpdateRemaining(requiredHandcuffs - _purchasedCount);

                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(0.3f);
            HideRemaining();
            CompletePurchase();
        }
        
        private void CompletePurchase()
        {
            // 돈 지불
            _coinZone.AddPendingCoins(price);

            GameEventBus.Publish_ItemSold(requiredHandcuffs);
            GameEventBus.Publish_CustomerServed(0);

            _queue.OnCriminalDeparting(this);

            _criminalState = CriminalState.MovingToPrison;
            _navigator.MoveTo(_prisonEntry.position, onArrived: () =>
            {
                StartCoroutine(EnterPrison());
            });
        }

        private IEnumerator EnterPrison()
        {
            // 수용소 자리로 이동
            if (_prisonSeat != null)
                _navigator.MoveTo(_prisonSeat.position, onArrived: OnEnteredPrison);
            else
                OnEnteredPrison();

            yield return null;
        }

        private void OnEnteredPrison()
        {
            _criminalState = CriminalState.InPrison;
            _queue.OnCriminalEntered();
            PrisonManager.Instance.AddPrisoner();

            _navigator.Stop();

            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
                agent.enabled = false;

        }

        private void StopBuying()
        {
            HideRemaining();
            StopAllCoroutines();
            // 제자리 대기
        }

        // -------------------------------------------------------
        // 부족량 표시
        // -------------------------------------------------------

        private void ShowRemaining(int count)
        {
            if (remainingDisplay == null) return;
            remainingDisplay.SetActive(true);
            UpdateRemaining(count);
        }

        private void UpdateRemaining(int count)
        {
            if (remainingText != null)
                remainingText.text = $"{count}";
        }

        private void HideRemaining()
        {
            remainingDisplay?.SetActive(false);
        }

        private void LateUpdate()
        {
            // 부족량 표시가 항상 카메라를 바라보도록
            if (remainingDisplay != null && remainingDisplay.activeSelf && _cam != null)
            {
                remainingDisplay.transform.position =
                    transform.position + Vector3.up * 2f;
                remainingDisplay.transform.LookAt(
                    remainingDisplay.transform.position + _cam.transform.forward);
            }
        }
    }
}
