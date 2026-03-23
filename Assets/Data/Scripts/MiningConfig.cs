using UnityEngine;

namespace PlayableAd.Data
{
    [CreateAssetMenu(fileName = "MiningConfig_Default", menuName = "PlayableAd/Mining Config")]
    public class MiningConfig : ScriptableObject
    {
        [Header("채굴 기본 설정")]
        [Tooltip("채굴 간격 (초)")]
        public float miningInterval = 1.5f;

        [Tooltip("채굴 범위 (앞 방향 반지름)")]
        public float miningRange = 1.5f;

        [Tooltip("광물 최대 보유량")]
        public int maxOreCount = 10;

        [Header("업그레이드 1단계 수치")]
        public float weapon1Interval = 1f;   
        public float weapon1Range = 3f;   
        public int weapon1MaxOre = 20;     

        [Header("업그레이드 2단계 수치")]
        public float weapon2Interval = 0.2f;
        public float weapon2Range = 5f;   
        public int weapon2MaxOre = 50;   

    }

}