using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Systems;
using PlayableAd.Utils;
using System;
using UnityEngine;

namespace PlayableAd.Items
{
    [RequireComponent(typeof(Collider))]
    public class OreItem : MonoBehaviour, ICollectable
    {
        //ICollectable
        public bool IsCollectable { get; private set; } = true;
        public ResourceType ResourceType => ResourceType.Ore;
        public int CollectAmount => 1;


        public Action OnCollectedCallback { get; set; }

        public bool Collect(GameObject collector)
        {
            if (!IsCollectable) return false;

            IsCollectable = false;

            bool isMax = ResourceManager.Instance.GetAmount(ResourceType.Ore)
                      >= ResourceManager.Instance.GetMax(ResourceType.Ore);

            if (!isMax)
            {
                ResourceManager.Instance.TryAdd(ResourceType, CollectAmount);
                OnCollected();
            }
            else
            {
                OnCollectedVisualOnly();
            }

            return true;
        }
        public void HarvestByNPC()
        {
            if (!IsCollectable) return;

            IsCollectable = false;

            // OreZone 리스폰 콜백 실행 (채굴 후 리스폰 타이머 시작)
            OnCollectedCallback?.Invoke();
            OnCollectedCallback = null;
        }
        public void OnCollected()
        {
            if (TryGetComponent<ItemMover>(out var mover))
                mover.StopMove();

            OreParticlePool.Instance.PlayAt(transform.position);
            GameEventBus.Publish_OreHarvested(CollectAmount);

            OnCollectedCallback?.Invoke();
            OnCollectedCallback = null;
            ObjectPool.Instance.Return(gameObject);
        }
        /// <summary>MAX 상태 — 파티클만 재생, 수량 추가 없음</summary>
        private void OnCollectedVisualOnly()
        {
            if (TryGetComponent<ItemMover>(out var mover))
                mover.StopMove();

            OreParticlePool.Instance.PlayAt(transform.position);
            GameEventBus.Publish_OreHarvested(0);
            OnCollectedCallback?.Invoke();
            OnCollectedCallback = null;
            ObjectPool.Instance.Return(gameObject);
        }
        public void ResetCollectable()
        {
            IsCollectable = true;
            OnCollectedCallback = null;

        }
        public void SetCollectable(bool value)
        {
            IsCollectable = value;
        }
    }
}