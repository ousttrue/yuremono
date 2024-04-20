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

        /// <summary>
        /// 親初期回転だった場合の方向ベクトルに stiffness を乗算した力を作る。
        /// 大きくなればなるほど、親が初期回転に戻ろうとする比率が増える。
        /// </summary>
        /// <param name="parentRotation"></param>
        /// <param name="_init"></param>
        /// <param name="stiffness"></param>
        /// <returns></returns>
        public static Vector3 StiffnessOriginal(Quaternion parentRotation, in ParticleInitState _init, float stiffness)
        {
            return parentRotation * (_init.BoneAxis * stiffness);
        }

        public static Vector3 StiffnessHookean(float delta, float stiffness, in ParticleRuntimeState _runtime, Vector3 targetPosition)
        {
            return (targetPosition - _runtime.CurrentPosition) * stiffness * delta * delta;
        }

        public static (Quaternion parentRotation, Vector3 newPos) _ApplyForce(
            in ParticleInitState _init,
            in ParticleRuntimeState _runtime,
            Vector3 parentPosition,
            Quaternion parentRotation,
            float dragRatio,
            Vector3 acceleration,
            IReadOnlyList<ParticleCollider> colliders
            )
        {
            //verlet
            var newPos = _runtime.Verlet(dragRatio, acceleration);

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
            float stiffness,
            float dragRatio,
            IReadOnlyList<ParticleCollider> colliders
            )
        {
            var force = StiffnessOriginal(parentRotation, _init, stiffness);
            return _ApplyForce(
                _init, _runtime,
                parentPosition, parentRotation,
                dragRatio, force, colliders);
        }

        public void Simulation(float stiffness, float dragRatio,
                    IReadOnlyList<ParticleCollider> colliders
        )
        {
            transform.parent.localRotation = _init.ParentLocalRotation;
            var (r, newPos) = _Simulation(_init, _runtime,
                transform.parent.position,
                transform.parent.rotation,
                stiffness, dragRatio, colliders);
            transform.parent.rotation = r;
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
        }

        public void AddStiffnessForce(float delta, float stiffness)
        {
            // TODO!
            transform.parent.localRotation = _init.ParentLocalRotation;
            // var q = Quaternion.identity;
            // if (transform.parent.parent != null)
            // {
            //     q = transform.parent.parent.rotation;
            // }
            // q *= _init.ParentLocalRotation;
            var f = StiffnessOriginal(transform.parent.rotation, _init, stiffness);
            // var f = StiffnessHookean(delta, stiffness, _runtime, transform.parent.position + q * _init.LocalPosition);
            _force += f;
        }

        public void ApplyForce(float dragRatio, List<ParticleCollider> colliders)
        {
            var q = Quaternion.identity;
            if (transform.parent.parent != null)
            {
                q = transform.parent.parent.rotation;
            }
            q *= _init.ParentLocalRotation;

            var (r, newPos) = _ApplyForce(_init, _runtime,
                transform.parent.position,
                q,
                dragRatio,
                _force, colliders);
            transform.parent.rotation = r;
            _runtime = new ParticleRuntimeState(_runtime.CurrentPosition, newPos);
            _force = Vector3.zero;
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