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

        Triangle _triangle0;
        float _trinagle0Collision;
        Triangle _triangle1;
        float _triangle1Collision;

        /// <summary>
        /// 三角形と衝突 
        /// d-c
        /// |/|
        /// a-b
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="posMap"></param>
        public void Collide(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            _capsule = default;

            var a = posMap[_a];
            var b = posMap[_b];
            var c = posMap[_c];
            var d = posMap[_d];

            _triangle0 = new Triangle(a, b, c);
            _trinagle0Collision -= 0.1f;
            if (_trinagle0Collision < 0)
            {
                _trinagle0Collision = 0;
            }

            _triangle1 = new Triangle(c, d, a);
            _triangle1Collision -= 0.1f;
            if (_triangle1Collision < 0)
            {
                _triangle1Collision = 0;
            }

            if (Collide(collider, posMap, _triangle0))
            {
                _trinagle0Collision = 1.0f;
                return;
            }
            if (Collide(collider, posMap, _triangle1))
            {
                _triangle1Collision = 1.0f;
                return;
            }
        }

        public bool Collide(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap, in Triangle triangle)
        {
            if (collider.Tail != null)
            {
                return ColliderCapsule(collider, posMap, triangle);
            }
            else
            {
                return ColliderSphere(collider.transform.position, collider.Radius, posMap, triangle);
            }
        }

        bool ColliderCapsule(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap, in Triangle triangle)
        {
            var capsule = new CapsuleInfo(triangle, collider);
            if (!capsule.Intersected)
            {
                _capsule = default;
                return false;
            }
            _capsule = capsule;

            if (capsule.Triangle.TryIntersectSegment(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, out var intersection))
            {
                return ColliderSphere(Vector3.Lerp(capsule.MinClamp, capsule.MaxClamp, intersection.t0), collider.Radius, posMap, triangle);
                // var cap = Vector3.Lerp(capsule.MinClamp, capsule.MaxClamp, intersection.t1);
                // var tri = _triangle.Plane.ClosestPointOnPlane(cap);
                // var distance = _triangle.Plane.GetDistanceToPoint(cap);
                // var delta = (tri - cap).normalized * (collider.Radius - distance);

                // ResolveDelta(delta, posMap);
            }

            return false;
        }


        bool ColliderSphere(in Vector3 collider, float radius, Dictionary<StrandParticle, Vector3> posMap, in Triangle triangle)
        {
            var p = triangle.Plane.ClosestPointOnPlane(collider);
            var distance = Vector3.Distance(p, collider);
            if (distance > radius)
            {
                return false;
            }

            if (!triangle.IsSameSide(p))
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

        static Color getColor(float n)
        {
            return new Color(Mathf.Lerp(0.3f, 1.0f, n), 0.3f, 0.3f);
        }

        public void DrawGizmo()
        {
            Gizmos.color = getColor(_trinagle0Collision);
            _triangle0.DrawGizmo();

            Gizmos.color = getColor(_triangle1Collision);
            _triangle1.DrawGizmo();

            if (_capsule.HasValue)
            {
                _capsule.Value.DrawGizmo();
            }
        }
    }
}