using System;
using UnityEngine;

namespace SpringCloth
{
    [Serializable]
    public struct ParticleInitState
    {
        [SerializeField]
        public Vector3 LocalPosition;

        [SerializeField]
        public Vector3 BoneAxis;

        [SerializeField]
        public Quaternion ParentLocalRotation;

        [SerializeField]
        public float SpringLength;

        [SerializeField]
        public float Radius;


        public ParticleInitState(Transform t, float radius)
        {
            LocalPosition = t.localPosition;
            SpringLength = LocalPosition.magnitude;
            BoneAxis = LocalPosition.normalized;
            ParentLocalRotation = t.parent.localRotation;
            Radius = radius;
        }

        public override string ToString()
        {
            return $"create Particle: {LocalPosition}, {BoneAxis}, {SpringLength}, {ParentLocalRotation}";
        }
    }

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

    [Serializable]
    public class SpringParam
    {
        [SerializeField, Range(0, 12)]
        public float Stiffness = 1.0f;

        [SerializeField, Range(0, 1)]
        public float DragRatio = 0.1f;

        /// <summary>
        /// 重力の場合 mass を乗算しておくべき？
        /// </summary>
        [SerializeField]
        public Vector3 ExternalForce = new Vector3(0, -0.1f, 0);
    }
}