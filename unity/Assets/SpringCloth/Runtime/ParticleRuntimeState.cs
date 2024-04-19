using UnityEngine;

namespace SpringCloth
{
    [SerializeField]
    public struct ParticleRuntimeState
    {
        [SerializeField]
        public Vector3 PreviousPosition;
        [SerializeField]
        public Vector3 CurrentPosition;

        public ParticleRuntimeState(Vector3 prev, Vector3 current)
        {
            PreviousPosition = prev;
            CurrentPosition = current;
        }

        public ParticleRuntimeState(Vector3 position)
        {
            CurrentPosition = PreviousPosition = position;
        }

        public Vector3 PositionDelta => CurrentPosition - PreviousPosition;

        public Vector3 Verlet(Vector3 acceleration, float sqrDt)
        {
            return CurrentPosition + CurrentPosition - PreviousPosition + acceleration * sqrDt;
        }
    }
}