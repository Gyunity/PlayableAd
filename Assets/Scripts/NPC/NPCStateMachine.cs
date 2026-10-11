
using UnityEngine;

namespace PlayableAd.NPC
{
    public enum NPCState
    {
        Idle,
        MovingToTarget,   // 목표 이동 중
        Working,          // 채굴 중 (MinerNPC)
        MovingToFactory,  // 공장 이동 중 (WorkerNPC)
        Selling           // 판매존 이동/판매 중 (WorkerNPC)
    }

    public abstract class NPCStateMachine : MonoBehaviour
    {
      public NPCState CurrentState { get; private set; } = NPCState.Idle;
        protected virtual void Update()
        {
            OnStateUpdate(CurrentState);
        }

        protected void TransitionTo(NPCState newState)
        {
            OnStateExit(CurrentState);
            CurrentState = newState;
            OnStateEnter(newState);
        }

        protected virtual void OnStateEnter(NPCState state) { }
        protected virtual void OnStateExit(NPCState state) { }
        protected virtual void OnStateUpdate(NPCState state) { }
        
    }

}