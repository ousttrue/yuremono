using System;
using System.Collections.Generic;
using SphereTriangle;
using UnityEngine;


namespace RotateParticle
{
    [Serializable]
    public class RotateParticle
    {
        public ParticleInitState Init;
        public ParticleRuntimeState State;

        public readonly List<RotateParticle> Children = new();
        public readonly RotateParticle Parent;

        // 現フレームの力積算
        public Vector3 Force = Vector3.zero;

        public RotateParticle(int index, RotateParticle parent, SimulationEnv env, Transform transform, float radius, float mass, CollisionGroupMask collisionMask)
        {
            Init = new ParticleInitState(index, transform, radius, mass, collisionMask);
            State = new ParticleRuntimeState(env, transform);
            Parent = parent;
        }

        public void BeginFrame(SimulationEnv env, float sqDt, in Vector3 rest)
        {
            // integrate forces
            Force = Vector3.zero;

            // 曲げ
            Force += (rest - State.Current) * env.Stiffness / sqDt;

            Force += (env.External / sqDt);
        }

        public Vector3 Verlet(SimulationEnv env, float sqDt)
        {
            return State.Current + (State.Current - State.Prev) * (1 - env.DragForce) + Force * sqDt;
        }

        /// <summary>
        /// get ParentParent.rotatio * Parent.Init.LocalRotation
        /// </summary>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public Quaternion RestRotation(IReadOnlyList<Transform> transforms)
        {
            if (Parent == null)
            {
                return Quaternion.identity;
            }

            var parent = transforms[Parent.Init.Index];
            if (Parent.Parent == null)
            {
                var pt = parent.parent;
                if (pt == null)
                {
                    return Parent.Init.LocalRotation;
                }

                return pt.rotation * Parent.Init.LocalRotation;
            }

            var parentparent = transforms[Parent.Parent.Init.Index];
            var restRotation = parentparent.rotation * Parent.Init.LocalRotation;
            return restRotation;
        }

        public void OnDrawGizmos(Transform transform)
        {
            if (Init.Radius == 0)
            {
                return;
            }

            Gizmos.color = Init.Mass == 0 ? Color.red : Color.gray;
            if (transform.parent != null && Init.Mass > 0)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, Init.Radius);

            var r = Init.Radius * 2;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.left * r, Vector3.right * r);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.down * r, Vector3.up * r);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.back * r, Vector3.forward * r);
        }
    }
}