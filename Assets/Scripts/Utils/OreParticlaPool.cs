using UnityEngine;
using PlayableAd.Utils;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 광물 채굴 파티클을 오브젝트 풀로 관리합니다.
    /// </summary>
    public class OreParticlePool : Singleton<OreParticlePool>
    {
        [SerializeField] private ParticleSystem particlePrefab;
        [SerializeField] private int poolSize = 10;

        private ParticleSystem[] _pool;
        private int _index = 0;

        protected override void OnAwake()
        {
            _pool = new ParticleSystem[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var ps = Instantiate(particlePrefab, transform);
                ps.gameObject.SetActive(false);
                _pool[i] = ps;
            }
        }

        public void PlayAt(Vector3 position)
        {
            var ps = _pool[_index];
            _index = (_index + 1) % poolSize;

            ps.gameObject.SetActive(true);
            ps.transform.position = position;
            ps.Stop();
            ps.Play();

            // 재생 완료 후 비활성화
            StartCoroutine(DeactivateAfter(ps, ps.main.duration + ps.main.startLifetime.constantMax));
        }

        private System.Collections.IEnumerator DeactivateAfter(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.gameObject.SetActive(false);
        }
    }
}