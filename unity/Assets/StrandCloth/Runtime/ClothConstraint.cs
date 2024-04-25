using System.Collections.Generic;
using UnityEngine;

namespace StrandCloth
{
    /// <summary>
    /// ４つの質点を参照する。
    /// d--c
    /// |  |
    /// a--b
    /// </summary>
    public class ClothConstraint
    {
        StrandParticle _a;
        StrandParticle _b;
        StrandParticle _c;
        StrandParticle _d;

        SpringConstraint _ab;
        SpringConstraint _ac;
        SpringConstraint _bd;

        public ClothConstraint(StrandParticle a, StrandParticle b, StrandParticle c, StrandParticle d)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _ab = new SpringConstraint(a, b);
            _ac = new SpringConstraint(a, c);
            _bd = new SpringConstraint(b, d);
        }

        CapsuleInfo? _capsule;

        Triangle _triangle;

        /// <summary>
        /// 三角形 a-b-(d+c/2) と衝突 
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="posMap"></param>
        public void Collide(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            _capsule = default;

            var a = posMap[_a];
            var b = posMap[_b];
            var c = (posMap[_c] + posMap[_d]) * 0.5f;
            _triangle = new Triangle(a, b, c);

            if (collider.Tail != null)
            {
                ColliderCapsule(collider, posMap);
            }
            else
            {
                ColliderSphere(collider.transform.position, collider.Radius, posMap);
            }
        }

        void ColliderCapsule(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            var capsule = new CapsuleInfo(_triangle, collider);
            if (!capsule.Intersected)
            {
                _capsule = default;
                return;
            }
            _capsule = capsule;

            if (capsule.Triangle.TryIntersect(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, out var t))
            {
                ColliderSphere(Vector3.Lerp(capsule.MinClamp, capsule.MaxClamp, t), collider.Radius, posMap);
            }
        }


        bool ColliderSphere(in Vector3 collider, float radius, Dictionary<StrandParticle, Vector3> posMap)
        {
            var p = _triangle.Plane.ClosestPointOnPlane(collider);
            var distance = Vector3.Distance(p, collider);
            if (distance > radius)
            {
                return false;
            }

            if (!_triangle.SameSide(p))
            {
                return false;
            }

            // 4点の移動量
            var delta = (p - collider).normalized * (radius - distance);
            // Debug.Log(delta);
            posMap[_a] = _a.Constraint(posMap[_a] + delta);
            posMap[_b] = _b.Constraint(posMap[_b] + delta);
            posMap[_c] = _c.Constraint(posMap[_c] + delta);
            posMap[_d] = _d.Constraint(posMap[_d] + delta);
            return true;
        }

        public void ResolveConstraint(float hookean)
        {
            // 1 だと暴れた
            _ab.Resolve(0.3f);
            _ac.Resolve(0.3f);
            _bd.Resolve(0.3f);
        }

        public void DrawGizmo()
        {
            Gizmos.color = Color.green;
            _triangle.DrawGizmo();

            if (_capsule.HasValue)
            {
                _capsule.Value.DrawGizmo();
            }
        }
    }
}