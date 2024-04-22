using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using UnityEngine;

namespace SpringCloth
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

        static bool SameSide(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var da = Vector3.Dot(p - a, b - a);
            var db = Vector3.Dot(p - b, c - b);
            var dc = Vector3.Dot(p - c, a - c);

            if (da > 0)
            {
                if (db > 0)
                {
                    if (dc > 0)
                    {
                        return true;
                    }
                }
            }
            else if (da < 0)
            {
                if (db < 0)
                {
                    if (dc < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 三角形 a-b-(d+c/2) と衝突 
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="posMap"></param>
        public void Collide(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            _capsule = default;

            if (collider.Tail != null)
            {
                ColliderCapsule(collider, posMap);
            }
            else
            {
                ColliderSphere(collider.transform.position, collider.Radius, posMap);
            }
        }

        struct Triangle
        {
            public Plane Plane;
            public Vector3[] Points;

            public Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                Plane = new Plane(a, b, c);
                Points = new Vector3[] { a, b, c };
            }

            public void DrawGizmo()
            {
                Gizmos.DrawLineStrip(Points, true);
            }
        }

        struct CapsuleInfo
        {
            public ParticleCollider Collider;
            public Triangle Triangle;
            public Vector3 HeadOnPlane;
            public float HeadDIstance;
            public Vector3 TailOnPlane;
            public float TailDistance;
            public CapsuleInfo(in Triangle t, ParticleCollider collider)
            {
                Collider = collider;
                Triangle = t;
                HeadOnPlane = t.Plane.ClosestPointOnPlane(collider.transform.position);
                TailOnPlane = t.Plane.ClosestPointOnPlane(collider.Tail.position);
                HeadDIstance = t.Plane.GetDistanceToPoint(collider.transform.position);
                TailDistance = t.Plane.GetDistanceToPoint(collider.Tail.position);
            }

            public bool Intersect()
            {
                if (HeadDIstance < -Collider.Radius)
                {
                    if (TailDistance < -Collider.Radius)
                    {
                        return false;
                    }
                }
                else if (HeadDIstance > Collider.Radius)
                {
                    if (TailDistance > Collider.Radius)
                    {
                        return false;
                    }
                }

                return true;
            }

            public void DrawGizmo()
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(Collider.transform.position, HeadOnPlane);
                Gizmos.DrawWireSphere(HeadOnPlane, 0.01f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Collider.Tail.position, TailOnPlane);
                Gizmos.DrawWireSphere(TailOnPlane, 0.01f);

                Triangle.DrawGizmo();
            }
        }
        CapsuleInfo? _capsule;

        void ColliderCapsule(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            var a = posMap[_a];
            var b = posMap[_b];
            var c = (posMap[_c] + posMap[_d]) * 0.5f;
            var t = new Triangle(a, b, c);
            // var ray = collider.HeadTailRay.Value;
            var capsule = new CapsuleInfo(t, collider);
            if (!capsule.Intersect())
            {
                _capsule = default;
            }
            else
            {
                _capsule = capsule;
            }
        }

        bool MoveSphere(in Vector3 p, in Vector3 delta, Dictionary<StrandParticle, Vector3> posMap)
        {
            var a = posMap[_a];
            var b = posMap[_b];
            var c = (posMap[_c] + posMap[_d]) * 0.5f;
            var plane = new Plane(a, b, c);

            // var p = plane.ClosestPointOnPlane(collider);

            if (!SameSide(p, a, b, c))
            {
                return false;
            }

            // 4点の移動量
            // var delta = (p - collider).normalized * (radius - distance);
            // Debug.Log(delta);
            posMap[_a] = _a.Constraint(posMap[_a] + delta);
            posMap[_b] = _b.Constraint(posMap[_b] + delta);
            posMap[_c] = _c.Constraint(posMap[_c] + delta);
            posMap[_d] = _d.Constraint(posMap[_d] + delta);
            // posMap[_a] = posMap[_a] + delta;
            // posMap[_b] = posMap[_b] + delta;
            // posMap[_c] = posMap[_c] + delta;
            // posMap[_d] = posMap[_d] + delta;
            return true;
        }

        bool ColliderSphere(in Vector3 collider, float radius, Dictionary<StrandParticle, Vector3> posMap)
        {
            var a = posMap[_a];
            var b = posMap[_b];
            var c = (posMap[_c] + posMap[_d]) * 0.5f;
            var plane = new Plane(a, b, c);

            var p = plane.ClosestPointOnPlane(collider);
            var distance = Vector3.Distance(p, collider);
            if (distance > radius)
            {
                return false;
            }

            if (!SameSide(p, a, b, c))
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
            // posMap[_a] = posMap[_a] + delta;
            // posMap[_b] = posMap[_b] + delta;
            // posMap[_c] = posMap[_c] + delta;
            // posMap[_d] = posMap[_d] + delta;
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
            if (_capsule.HasValue)
            {
                _capsule.Value.DrawGizmo();
            }

            // Gizmos.color = Color.white;
            // _ab.DrawGizmo();
            // _ac.DrawGizmo();
            // _bd.DrawGizmo();
        }
    }
}