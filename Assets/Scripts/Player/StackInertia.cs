using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Player
{
    public class StackInertia : MonoBehaviour
    {
        [Header("관성 설정")]
        [SerializeField] private float inertiaSpeed = 10f;
        [SerializeField] private float stopThreshold = 0.1f;

        [Header("휘어짐 설정")]
        [SerializeField] private float bendStrength = 0.01f;
        [SerializeField] private float curvePower = 3f;
        [SerializeField] private float heightMultiplier = 0.3f;

        [Header("스프링 복귀 설정")]
        [SerializeField] private float springStiffness = 50f;
        [SerializeField] private float springDamping = 4f;

        private Vector3 _prevPosition;
        private float _currentBendX;
        private float _currentBendZ;
        private float _bendVelocityX;
        private float _bendVelocityZ;
        private bool _wasMoving;

        private PlayerInventory _inventory;
        private StackSlotManager _slotManager;

        private void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _slotManager = GetComponent<StackSlotManager>();
            _prevPosition = transform.position;
        }

        private void LateUpdate()
        {
            var worldVelocity = (transform.position - _prevPosition) / Time.deltaTime;
            _prevPosition = transform.position;

            var localVelocity = transform.InverseTransformDirection(worldVelocity);
            localVelocity.y = 0f;

            bool isMoving = localVelocity.magnitude > stopThreshold;

            if (isMoving)
            {
                float targetBendX = localVelocity.z * (bendStrength * 10f);
                float targetBendZ = -localVelocity.x * (bendStrength * 10f);

                _currentBendX = Mathf.Lerp(_currentBendX, targetBendX,
                                    Time.deltaTime * inertiaSpeed);
                _currentBendZ = Mathf.Lerp(_currentBendZ, targetBendZ,
                                    Time.deltaTime * inertiaSpeed);

                _wasMoving = true;
            }
            else
            {
                if (_wasMoving)
                {
                    _bendVelocityX = _currentBendX * springStiffness * 0.3f;
                    _bendVelocityZ = _currentBendZ * springStiffness * 0.3f;
                    _wasMoving = false;
                }

                SpringUpdate(ref _currentBendX, ref _bendVelocityX);
                SpringUpdate(ref _currentBendZ, ref _bendVelocityZ);
            }

            ApplyInertia();
        }

        private void SpringUpdate(ref float current, ref float velocity)
        {
            float springForce = -springStiffness * current;
            float dampingForce = -springDamping * velocity;

            velocity += (springForce + dampingForce) * Time.deltaTime;
            current += velocity * Time.deltaTime;

            if (Mathf.Abs(current) < 0.001f && Mathf.Abs(velocity) < 0.001f)
            {
                current = 0f;
                velocity = 0f;
            }
        }

        private void ApplyInertia()
        {
            if (_inventory == null) return;

            ApplyToStack(_inventory.GetOreStack(), isOre: true);
            ApplyToStack(_inventory.GetCoinStack(), isOre: false);
        }

        private void ApplyToStack(List<GameObject> stack, bool isOre)
        {
            if (stack == null || stack.Count == 0) return;

            int count = stack.Count;

            for (int i = 0; i < count; i++)
            {
                var go = stack[i];
                if (go == null || !go.activeSelf) continue;

                // 곡선 분포
                float t = count > 1 ? (float)i / (count - 1) : 0f;
                float curveFactor = Mathf.Pow(t, curvePower)
                                  * (1f + heightMultiplier * count);

                go.transform.localRotation = Quaternion.identity;

                var baseLocalPos = isOre
                    ? _slotManager.GetLocalPositionForOre(i)
                    : _slotManager.GetLocalPositionForCoin(i);

                float bendZ = _currentBendX * curveFactor;
                float bendX = _currentBendZ * curveFactor;

                go.transform.localPosition = baseLocalPos
                    + new Vector3(-bendX, 0f, -bendZ);
            }
        }
    }
}