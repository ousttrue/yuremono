using System.Collections.Generic;
using UnityEngine;

namespace SpringCloth
{
    [DisallowMultipleComponent]
    public class Particle : MonoBehaviour
    {
        public float SphereRadius = 0.05f;
        public Color GizmoColor = Color.magenta;

        public Vector3 Force;

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

        static Vector3 NormalizedPosition(Vector3 p, float len, Vector3 dst)
        {
            //長さを元に戻して次の場所を求める
            return p + (dst - p).normalized * len;
        }

        static Quaternion UpdateParentRotation(in Vector3 parentPosition, in Quaternion parentRotation, in Vector3 boneAxis, in Vector3 currentPos)
        {
            // to world
            Vector3 aimVector = parentRotation * boneAxis;
            Quaternion aimRotation = Quaternion.FromToRotation(aimVector, currentPos - parentPosition);
            return aimRotation * parentRotation;
        }

        public static Vector3 _CalcForce(
            in ParticleInitState _init,
            in ParticleRuntimeState _runtime,
            Quaternion parentRotation,
            float delta,
            in SpringParam param
            )
        {
            float sqrDt = delta * delta;
            // 親の回転から力を作る
            var force = parentRotation * (_init.BoneAxis * param.Stiffness) / sqrDt;

            // drag
            force -= _runtime.PositionDelta * param.DragRatio / sqrDt;

            // 重力
            force += param.ExternalForce / sqrDt;

            return force;
        }

        public static (Quaternion parentRotation, Vector3 newPos) _ApplyForce(
            in ParticleInitState _init,
            in ParticleRuntimeState _runtime,
            Vector3 parentPosition,
            Quaternion parentRotation,
            float delta,
            Vector3 force,
            IReadOnlyList<ParticleCollider> colliders
            )
        {
            float sqrDt = delta * delta;

            //verlet
            // TODO: mass による除算
            var newPos = _runtime.Verlet(force, sqrDt);

            // update
            var resolved = NormalizedPosition(parentPosition, _init.SpringLength, newPos);

            // 衝突判定
            foreach (var c in colliders)
            {
                if (c != null)
                {
                    var collision = c.Collide(resolved, _init.Radius);
                    if (collision.HasValue)
                    {
                        // resolved = NormalizedPosition(parentPosition, _init.SpringLength, collision.Value);
                        resolved = collision.Value;
                    }
                }
            }

            // 親の回転として結果を適用する(位置から回転を作る)
            return (UpdateParentRotation(parentPosition, parentRotation, _init.BoneAxis, _runtime.CurrentPosition), resolved);

        }

        public static (Quaternion parentRotation, Vector3 newPos) _Simulation(
            in ParticleInitState _init,
            in ParticleRuntimeState _runtime,
            Vector3 parentPosition,
            Quaternion parentRotation,
            float delta,
            in SpringParam param,
            IReadOnlyList<ParticleCollider> colliders
            )
        {
            var force = _CalcForce(_init, _runtime, parentRotation, delta, param);
            return _ApplyForce(_init, _runtime, parentPosition, parentRotation, delta, force, colliders);
        }

        public void Simulation(float delta, in SpringParam param,
                    IReadOnlyList<ParticleCollider> colliders
        )
        {
            transform.parent.localRotation = _init.ParentLocalRotation;
            var (r, newPos) = _Simulation(_init, _runtime,
                transform.parent.position,
                transform.parent.rotation,
                delta, param, colliders);
            transform.parent.rotation = r;
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
        }

        public void CalcForce(float delta, in SpringParam param, bool add)
        {
            transform.parent.localRotation = _init.ParentLocalRotation;
            var f = _CalcForce(_init, _runtime, transform.parent.rotation, delta, param);
            if (add)
            {
                Force += f;
            }
            else
            {
                Force = f;
            }
        }

        public void ApplyForce(float delta, List<ParticleCollider> colliders)
        {
            var (r, newPos) = _ApplyForce(_init, _runtime,
                transform.parent.position,
                transform.parent.rotation,
                delta, Force, colliders);
            transform.parent.rotation = r;
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
        }

        void OnDrawGizmos()
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