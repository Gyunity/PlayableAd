using PlayableAd.Items;
using PlayableAd.Player;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 광물 스폰/리스폰만 담당합니다.
    /// 플레이어 진입 감지 없음 — PlayerMiner가 Physics로 직접 탐색합니다.
    /// </summary>
    public class OreZone : MonoBehaviour
    {
        [SerializeField] 
        private List<Transform> oreSlots;
        [SerializeField] 
        private float respawnDelay = 2f;
        [Header("플레이어 무기 비주얼")]
        [SerializeField] private WeaponVisual weaponVisual;

        private readonly Dictionary<Transform, OreItem> _slotOreMap = new();


        private void Start()
        {
            foreach (var slot in oreSlots)
                SpawnAt(slot);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // 무기 비주얼 자동 탐색 (인스펙터 미연결 시)
            if (weaponVisual == null)
                weaponVisual = other.GetComponentInChildren<WeaponVisual>();

            weaponVisual?.ShowWeapon();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            weaponVisual?.HideWeapon();
        }
        private void SpawnAt(Transform slot)
        {
            var go = ObjectPool.Instance.Get("Ore", slot.position, Quaternion.identity);
            if (go == null) return;

            var ore = go.GetComponent<OreItem>();
            if (ore == null) return;

            ore.ResetCollectable();
            //수집됐을 때 콜백 — 해당 슬롯 기준으로 개별 타이머 시작
            ore.OnCollectedCallback = () => StartCoroutine(RespawnAfterDelay(slot));

            _slotOreMap[slot] = ore;
        }

        private IEnumerator RespawnAfterDelay(Transform slot)
        {
            _slotOreMap[slot] = null;

            yield return new WaitForSeconds(respawnDelay);
            SpawnAt(slot);
        }
        /// <summary>PlayerMiner / MiningSystem이 범위 내 광물 탐색 시 사용</summary>
        public List<OreItem> GetCollectablesInRange(Vector3 center, float range)
        {
            var result = new List<OreItem>();

            foreach (var ore in _slotOreMap.Values)
            {
                if (ore == null) continue;
                if (!ore.IsCollectable) continue;
                if (!ore.gameObject.activeSelf) continue;
                if (Vector3.Distance(ore.transform.position, center) > range) continue;

                result.Add(ore);
            }

            return result;
        }
    }
}