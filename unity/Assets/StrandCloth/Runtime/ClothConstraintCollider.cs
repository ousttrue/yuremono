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
    public class ClothConstraintCollider
    {
        StrandParticle _a;
        StrandParticle _b;
        StrandParticle _c;
        StrandParticle _d;

        SpringConstraint _ab;
        SpringConstraint _ac;
        SpringConstraint _bd;

        public ClothConstraintCollider(StrandParticle a, StrandParticle b, StrandParticle c, StrandParticle d)
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
        /// 三角形 a-b-(c+d/2) と衝突 
        ///   c
        ///  /\
        /// /  \
        ///a----b
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

            if (capsule.Triangle.TryIntersectSegment(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, out var intersection))
            {
                ColliderSphere(Vector3.Lerp(capsule.MinClamp, capsule.MaxClamp, intersection.t0), collider.Radius, posMap);
                // var cap = Vector3.Lerp(capsule.MinClamp, capsule.MaxClamp, intersection.t1);
                // var tri = _triangle.Plane.ClosestPointOnPlane(cap);
                // var distance = _triangle.Plane.GetDistanceToPoint(cap);
                // var delta = (tri - cap).normalized * (collider.Radius - distance);

                // ResolveDelta(delta, posMap);
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

            if (!_triangle.IsSameSide(p))
            {
                return false;
            }


            // 4点の移動量
            var delta = (p - collider).normalized * (radius - distance);
            ResolveDelta(delta, posMap);
            return true;
        }

        void ResolveDelta(in Vector3 delta, Dictionary<StrandParticle, Vector3> posMap)
        {
            // Debug.Log(delta);\
            if (_a.Mass > 0)
            {
                posMap[_a] = posMap[_a] + delta;
            }
            if (_b.Mass > 0)
            {
                posMap[_b] = posMap[_b] + delta;
            }
            // if (_c.Mass > 0)
            // {
            //     posMap[_c] = posMap[_c] + delta;
            // }
            // if (_d.Mass > 0)
            // {
            //     posMap[_d] = posMap[_d] + delta;
            // }
        }

        public void ResolveConstraint(float factor)
        {
            _ab.Resolve(factor);
            // _ac.Resolve(factor);
            // _bd.Resolve(factor);
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