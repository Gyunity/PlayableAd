using PlayableAd.Core;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 스테이지 전환 시 해당 단계에 필요한 존을 활성화합니다.
    /// 모든 존은 시작 시 비활성 상태입니다.
    /// </summary>
    public class ItemSpawnSystem : MonoBehaviour
    {
        [Header("단계별 존")]
        [SerializeField] private GameObject oreDepositZone;    // 항상 활성
        [SerializeField] private GameObject handcuffFactory;   // 항상 활성
        [SerializeField] private GameObject salesZone;         // Stage3 활성화
        [SerializeField] private GameObject weaponShopZone;    // Stage4 활성화
        [SerializeField] private GameObject minerShopZone;     // Stage5 활성화
        [SerializeField] private GameObject workerShopZone;    // Stage6 활성화
        [SerializeField] private GameObject weaponShop2Zone;   // Stage7 활성화
        [SerializeField] private GameObject prisonExpandZone;  // Stage8 활성화
        [SerializeField] private GameObject endPanel; //종료 패널
        [Header("확장벽")]
        [SerializeField] private GameObject[] expandWall;
        [SerializeField] private GameObject exitWall;
        [Header("다음 단계 안내 화살표")]
        [SerializeField] private GameObject arrowIndicator;

        private void Start()
        {
            // 초기 상태: 채굴/공장만 활성, 나머지 비활성
            oreDepositZone?.SetActive(true);
            handcuffFactory?.SetActive(true);
            salesZone?.SetActive(true);
            weaponShopZone?.SetActive(false);
            minerShopZone?.SetActive(false);
            workerShopZone?.SetActive(false);
            weaponShop2Zone?.SetActive(false);
            prisonExpandZone?.SetActive(false);
            
            for(int i = 0; i < expandWall.Length; i++)
                expandWall[i].SetActive(false);


            // 화살표는 공장 방향 표시
            MoveArrow(handcuffFactory);
        }

        private void OnEnable()
        {
            GameEventBus.OnStageChanged += HandleStageChanged;
        }

        private void OnDisable()
        {
            GameEventBus.OnStageChanged -= HandleStageChanged;
        }

        private void HandleStageChanged(GameStage stage)
        {
            switch (stage)
            {
                case GameStage.Stage3_Sales:
                    salesZone?.SetActive(true);
                    MoveArrow(salesZone);
                    break;

                case GameStage.Stage4_WeaponUpgrade:
                    weaponShopZone?.SetActive(true);
                    MoveArrow(weaponShopZone);
                    break;

                case GameStage.Stage5_BuyMiner:
                    minerShopZone?.SetActive(true);
                    MoveArrow(minerShopZone);
                    break;

                case GameStage.Stage6_BuyWorker:
                    weaponShop2Zone?.SetActive(true);
                    workerShopZone?.SetActive(true);
                    MoveArrow(workerShopZone);
                    break;

                case GameStage.Stage7_WeaponUpgrade2:
                    MoveArrow(weaponShop2Zone);
                    break;

                case GameStage.Stage8_PrisonExpand:
                    prisonExpandZone?.SetActive(true);
                    MoveArrow(prisonExpandZone);
                    break;

                case GameStage.Stage9_End:

                    exitWall.SetActive(false);

                    for (int i = 0; i < expandWall.Length; i++)
                        expandWall[i].SetActive(true);

                    break;

                case GameStage.Stage10_UI:
                    endPanel?.SetActive(true);
                    break;
            }
        }

        private void MoveArrow(GameObject target)
        {
            if (arrowIndicator == null || target == null) return;

            arrowIndicator.SetActive(true);
            arrowIndicator.transform.position =
                target.transform.position + Vector3.up * 2f;
        }
    }
}