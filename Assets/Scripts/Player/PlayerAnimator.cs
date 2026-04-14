
using UnityEngine;

namespace PlayableAd.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerMiner playerMiner;

        [Header("설정")]
        [SerializeField] private float speedDampTime = 0.1f;

        private Animator _animator;
        private bool _wasMining = false;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.SetLayerWeight(1, 0);
        }

        private void Update()
        {
            UpdateSpeed();
            UpdateMining();
        }

        private void UpdateSpeed()
        {
            if (playerController == null) return;

            _animator.SetFloat(
                SpeedHash,
                playerController.CurrentSpeed,
                speedDampTime,
                Time.deltaTime);
        }

        private void UpdateMining()
        {
            if (playerMiner == null) return;
            bool isCurrentlyMining = playerMiner.IsInOreZone;


            if (playerMiner.WeaponLevel == 0 && isCurrentlyMining != _wasMining)
            {

                float targetWeight = isCurrentlyMining ? 1f : 0f;

                _animator.SetLayerWeight(1, targetWeight);

                _wasMining = isCurrentlyMining;
            }
        }
     
    }
}