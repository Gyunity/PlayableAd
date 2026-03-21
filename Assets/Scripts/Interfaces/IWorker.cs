
using UnityEngine;

namespace PlayableAd.Interface
{

    public interface IWorker
    {
        NPCRole Role { get; }
        bool IsWorking { get; }

        void AssignTarget(Transform target);
        void StartWork();
        void StopWork();
    }

}