using PlayableAd.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Ststems
{
    /// <summary>
    /// MinerNPC 간 동일 광석 중복 채굴 방지.
    /// 광석 채굴 전 Reserve() 호출, 완료 후 Release() 호출.
    /// </summary>
    public static class OreReservationSystem
    {
        private static readonly Dictionary<OreItem, object> _reservations = new();
        
        public static bool IsReserved(OreItem ore)
            => ore != null && _reservations.ContainsKey(ore);

        public static bool TryReserve(OreItem ore, object reserver)
        {
            if (ore == null) return false;

            // 이미 다른 NPC가 예약했으면 false 반환
            if (_reservations.ContainsKey(ore)) return false;

            _reservations[ore] = reserver;
            return true;
        }

        public static void Release (OreItem ore)
        {
            if (ore != null)
                _reservations.Remove(ore);
        }
    }
}