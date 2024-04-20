using System.Collections.Generic;
using UnityEngine;

namespace SpringCloth
{
    [DisallowMultipleComponent]
    public class StrandParticle : MonoBehaviour
    {
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

        public void Setup(float radius, Transform simulationSpace = null)
        {
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
            var restRotation = transform.parent.parent.rotation * _init.ParentLocalRotation;
            var restPosition = transform.parent.position + restRotation * _init.BoneAxis;
            var f = Stiffness(restPosition, _runtime.CurrentPosition, stiffness);
            _force += f;
        }

        static Vector3 Stiffness(Vector3 restPosition, Vector3 currTipPos, float stiffness)
        {
            return (restPosition - currTipPos) * stiffness;
        }

        public void ApplyForce(float dragRatio, List<ParticleCollider> colliders)
        {
            var restRotation = transform.parent.parent.rotation * _init.ParentLocalRotation;
            // var restPosition = transform.parent.position + restRotation * _init.BoneAxis;
            var newPos = _ApplyForce(_init, _runtime,
                transform.parent.position,
                dragRatio,
                _force, colliders);
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
            _force = Vector3.zero;

            // 親の回転として結果を適用する(位置から回転を作る)
            var r = CalcRotation(restRotation, _init.BoneAxis, newPos - transform.parent.position);
            transform.parent.rotation = r;
        }

        public static Vector3 _ApplyForce(
            in ParticleInitState _init,
            in ParticleRuntimeState _runtime,
            Vector3 parentPosition,
            float dragRatio,
            Vector3 acceleration,
            IReadOnlyList<ParticleCollider> colliders
            )
        {
            var newPos = _runtime.Verlet(dragRatio, acceleration);
            newPos = Constraint(newPos, parentPosition, _init.SpringLength);
            foreach (var c in colliders)
            {
                if (c != null && c.TryCollide(newPos, _init.Radius, out var resolved))
                {
                    newPos = Constraint(resolved, parentPosition, _init.SpringLength);
                }
            }
            return newPos;
        }

        //長さを元に戻す
        static Vector3 Constraint(Vector3 to, Vector3 from, float len)
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