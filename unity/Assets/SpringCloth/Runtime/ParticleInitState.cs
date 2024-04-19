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
}