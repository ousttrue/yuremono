using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpringCloth
{
    [DisallowMultipleComponent]
    public class StrandParticle : MonoBehaviour
    {
        /// <summary>
        /// 0 の場合は動かない(root用)
        /// 
        /// TODO: acceleration = force/mass
        /// </summary>
        public float Mass = 1.0f;

        public float SphereRadius = 0.05f;
        public Color GizmoColor = Color.cyan;

        Vector3 _force;

        public void AddForce(Vector3 f)
        {
            _force += f;
        }

        /// <summary>
        /// 初期化時に固定される不変の情報
        /// </summary>
        [SerializeField]
        ParticleInitState _init;

        /// <summary>
        /// 毎フレーム変化する情報
        /// </summary>
        [SerializeField]
        ParticleRuntimeState _runtime;

        public void Setup(float radius, float mass, Transform simulationSpace = null)
        {
            Mass = mass;
            if (mass == 0)
            {
                GizmoColor = Color.red;
            }

            // gizmo
            SphereRadius = radius;

            _init = new ParticleInitState(transform, radius);
            Debug.Log(_init, transform);
            if (simulationSpace)
            {
                // TODO
                throw new System.NotImplementedException();
            }
            else
            {
                // use world
                _runtime = new ParticleRuntimeState(transform.position);
            }
        }

        public void AddStiffnessForce(float delta, float stiffness)
        {
            if(Mass==0)
            {
                return;
            }
            var restRotation = transform.parent.parent.rotation * _init.ParentLocalRotation;
            var restPosition = transform.parent.position + restRotation * _init.BoneAxis;
            var f = Stiffness(restPosition, _runtime.CurrentPosition, stiffness);
            _force += f;
        }

        static Vector3 Stiffness(Vector3 restPosition, Vector3 currTipPos, float stiffness)
        {
            return (restPosition - currTipPos) * stiffness;
        }

        public Vector3 ApplyVerlet(float dragRatio)
        {
            var newPos = _runtime.Verlet(dragRatio, _force);
            return _Constraint(newPos, transform.parent.position, _init.SpringLength);
        }

        public void ApplyRotationFromPosition(Vector3 newPos)
        {
            if (Mass == 0)
            {
                return;
            }
            var restRotation = transform.parent.parent.rotation * _init.ParentLocalRotation;
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
            _force = Vector3.zero;

            // 親の回転として結果を適用する(位置から回転を作る)
            var r = CalcRotation(restRotation, _init.BoneAxis, newPos - transform.parent.position);
            transform.parent.rotation = r;
        }

        public Vector3 Collision(Vector3 newPos, IReadOnlyList<ParticleCollider> colliders, Func<Vector3, Vector3> constraint)
        {
            return _Collision(newPos, _init, colliders, constraint);
        }

        public static Vector3 _Collision(Vector3 p, in ParticleInitState _init, IReadOnlyList<ParticleCollider> colliders, Func<Vector3, Vector3> constraint)
        {
            foreach (var c in colliders)
            {
                if (c != null && c.TryCollide(p, _init.Radius, out var resolved))
                {
                    p = constraint(resolved);
                }
            }
            return p;
        }

        //長さを元に戻す
        public Vector3 Constraint(Vector3 to)
        {
            return _Constraint(to, transform.parent.position, _init.SpringLength);
        }

        public static Vector3 _Constraint(Vector3 to, Vector3 from, float len)
        {
            return from + (to - from).normalized * len;
        }

        //回転を適用；
        static Quaternion CalcRotation(Quaternion restRotation, Vector3 boneAxis, Vector3 to)
        {
            Quaternion aimRotation = Quaternion.FromToRotation(restRotation * boneAxis, to);
            return aimRotation * restRotation;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            if (transform.parent)
            {
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
            Gizmos.DrawWireSphere(transform.position, SphereRadius);
        }
    }
}