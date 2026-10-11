using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace PlayableAd.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCNavigator : MonoBehaviour
    {
        [SerializeField]
        private float arrivalThreshold = 0.5f;

        private NavMeshAgent _agent;
        private Action _onArrived;
        private bool _isMoving;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            Speed = 0f;

        }

        private void Update()
        {
            if (!_isMoving) return;
            if (_agent.pathPending) return;
            if (_agent.remainingDistance > arrivalThreshold) return;

            _isMoving = false;
            var callback = _onArrived;
            _onArrived = null;
            callback?.Invoke();
        }

        public void MoveTo(Vector3 destination, Action onArrived = null)
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                Debug.LogWarning($"[NPCNavigator] {name} NavMesh 위에 없음");
                onArrived?.Invoke();
                return;
            }
            Speed = 5;
            _onArrived = onArrived;
            _isMoving = true;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            _isMoving = false;
            _onArrived = null;
            _agent.ResetPath();

            Speed = 0f;
        }

        public float Speed
        {
            get => _agent.speed;
            set => _agent.speed = value;
        }
    }

}