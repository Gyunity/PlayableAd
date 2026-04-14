using PlayableAd.NPC;
using PlayableAd.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.NPC
{

    [RequireComponent(typeof(Animator))]

    public class CriminalNPCAnimator : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private NPCNavigator nPCNavigator;
        [Header("설정")]
        [SerializeField] private float speedDampTime = 0.1f;

        private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            UpdateSpeed();
        }

        private void UpdateSpeed()
        {
            if (nPCNavigator == null) return;

            _animator.SetFloat(
                SpeedHash,
                nPCNavigator.Speed,
                speedDampTime,
                Time.deltaTime);
        }
    }
}