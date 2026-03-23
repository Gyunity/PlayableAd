using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using UnityEngine;
using Cinemachine;
using PlayableAd.Core;

namespace PlayableAd.UI
{
    /// <summary>
    /// 이벤트 발생 시 해당 Virtual Camera로 전환 후
    /// 일정 시간 뒤 플레이어 카메라로 복귀합니다.
    /// </summary>
    public class CameraDirector : MonoBehaviour
    {
        [Header("Virtual Cameras")]
        [SerializeField] private CinemachineVirtualCamera vcamPlayer;       // 기본 플레이어 카메라
        [SerializeField] private CinemachineVirtualCamera vcamWeaponShop;   // 무기존 연출
        [SerializeField] private CinemachineVirtualCamera vcamPrison;       // 수용소 연출
        [SerializeField] private CinemachineVirtualCamera vcamPrisonExpand; // 확장 수용소 연출

        [Header("연출 설정")]
        [SerializeField] private float cutsceneDuration = 2.5f; // 연출 지속 시간

        private const int PRIORITY_HIGH = 20;
        private const int PRIORITY_NORMAL = 10;
        private const int PRIORITY_OFF = 0;

        private bool _isPlaying = false;

        private void OnEnable()
        {
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
            GameEventBus.OnPrisonFull += HandlePrisonFull;
            GameEventBus.OnPrisonExpanded += HandlePrisonExpanded;
        }

        private void OnDisable()
        {
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEventBus.OnPrisonFull -= HandlePrisonFull;
            GameEventBus.OnPrisonExpanded -= HandlePrisonExpanded;
        }

        private void Start()
        {
            // 초기 우선순위 설정
            vcamPlayer.Priority = PRIORITY_NORMAL;
            vcamWeaponShop.Priority = PRIORITY_OFF;
            vcamPrison.Priority = PRIORITY_OFF;
            vcamPrisonExpand.Priority = PRIORITY_OFF;
        }

        // -------------------------------------------------------
        // 이벤트 핸들러
        // -------------------------------------------------------

        private bool _weaponShopShown = false;

        private void HandleCurrencyChanged(int balance)
        {
            // ✅ 처음 돈을 얻었을 때 1회만 실행
            if (_weaponShopShown) return;
            if (balance <= 0) return;

            _weaponShopShown = true;
            PlayCutscene(vcamWeaponShop);
        }

        private void HandlePrisonFull()
        {
            PlayCutscene(vcamPrison);
        }

        private void HandlePrisonExpanded()
        {
            PlayCutscene(vcamPrisonExpand);
        }

        // -------------------------------------------------------
        // 연출 실행
        // -------------------------------------------------------

        private void PlayCutscene(CinemachineVirtualCamera targetVcam)
        {
            if (_isPlaying) return;
            StartCoroutine(CutsceneRoutine(targetVcam));
        }

        private IEnumerator CutsceneRoutine(CinemachineVirtualCamera targetVcam)
        {
            _isPlaying = true;

            // 플레이어 카메라 낮추고 연출 카메라 올리기
            vcamPlayer.Priority = PRIORITY_OFF;
            targetVcam.Priority = PRIORITY_HIGH;

            yield return new WaitForSeconds(cutsceneDuration);

            // 플레이어 카메라로 복귀
            targetVcam.Priority = PRIORITY_OFF;
            vcamPlayer.Priority = PRIORITY_NORMAL;

            // 블렌딩 완료까지 잠시 대기
            yield return new WaitForSeconds(0.5f);

            _isPlaying = false;
        }
    }
}
