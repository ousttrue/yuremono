using System.Collections.Generic;
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

            public Vector3 MinOnPlane;
            public float MinDistance;
            public float MinDistanceClmap;
            public Vector3 MinOnPlaneClamp;

            public Vector3 MaxOnPlane;
            public float MaxDistance;
            public float MaxDistanceClamp;
            public Vector3 MaxOnPlaneClamp;

            /// <summary>
            /// min and max is tail and head.
            /// </summary>
            public bool Reverse;

            public bool Intersected;

            public CapsuleInfo(in Triangle t, ParticleCollider collider)
            {
                Collider = collider;
                Triangle = t;
                var headDistance = t.Plane.GetDistanceToPoint(collider.transform.position);
                var tailDistance = t.Plane.GetDistanceToPoint(collider.Tail.position);
                if (headDistance <= tailDistance)
                {
                    Reverse = false;
                    MinDistance = headDistance;
                    MaxDistance = tailDistance;
                    MinOnPlane = t.Plane.ClosestPointOnPlane(collider.transform.position);
                    MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.Tail.position);
                }
                else
                {
                    Reverse = true;
                    MaxDistance = headDistance;
                    MinDistance = tailDistance;
                    MaxOnPlane = t.Plane.ClosestPointOnPlane(collider.transform.position);
                    MinOnPlane = t.Plane.ClosestPointOnPlane(collider.Tail.position);
                }

                // Intersect
                Intersected = true;
                MinDistanceClmap = MinDistance;
                MinOnPlaneClamp = MinOnPlane;
                MaxDistanceClamp = MaxDistance;
                MaxOnPlaneClamp = MaxOnPlane;
                if (MinDistance < -Collider.Radius)
                {
                    if (MaxDistance < -Collider.Radius)
                    {
                        Intersected = false;
                    }
                    else
                    {
                        // clamp Min
                        MinDistanceClmap = -Collider.Radius;
                    }
                }
                else if (MaxDistance > Collider.Radius)
                {
                    if (MinDistance > Collider.Radius)
                    {
                        Intersected = false;
                    }
                    else
                    {
                        // clamp Max
                        MaxDistanceClamp = Collider.Radius;
                    }
                }

                // 
                MaxOnPlaneClamp = MinOnPlane + (MaxOnPlane - MinOnPlane) * (MaxDistanceClamp - MinDistance) / (MaxDistance - MinDistance);
                MinOnPlaneClamp = MaxOnPlane + (MinOnPlane - MaxOnPlane) * (MinDistanceClmap - MaxDistance) / (MinDistance - MaxDistance);
            }

            public void DrawGizmo()
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Reverse ? Collider.Tail.position : Collider.transform.position, MinOnPlane);
                Gizmos.DrawWireSphere(MinOnPlane, 0.01f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Reverse ? Collider.transform.position : Collider.Tail.position, MaxOnPlane);
                Gizmos.DrawWireSphere(MaxOnPlane, 0.01f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(MinOnPlaneClamp, MaxOnPlaneClamp);

                Gizmos.color = Color.green;
                Triangle.DrawGizmo();
            }
        }
        CapsuleInfo? _capsule;

        int _xn;
        Vector3? _xa;
        Vector3? _xb;
        Vector3? _xc;

        void ColliderCapsule(ParticleCollider collider, Dictionary<StrandParticle, Vector3> posMap)
        {
            _xa = default;
            _xb = default;
            _xc = default;
            _xn = 0;

            var a = posMap[_a];
            var b = posMap[_b];
            var c = (posMap[_c] + posMap[_d]) * 0.5f;
            var t = new Triangle(a, b, c);
            // var ray = collider.HeadTailRay.Value;
            var capsule = new CapsuleInfo(t, collider);
            if (!capsule.Intersected)
            {
                _capsule = default;
                return;
            }

            _capsule = capsule;

            _xa = IntersectSegments(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, a, b);
            if (_xa.HasValue)
            {
                ++_xn;
            }
            _xb = IntersectSegments(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, b, c);
            if (_xb.HasValue)
            {
                ++_xn;
            }
            var xc = IntersectSegments(capsule.MinOnPlaneClamp, capsule.MaxOnPlaneClamp, c, a);
            if (xc.HasValue)
            {
                ++_xn;
            }

            if (_xn >= 2)
            {
                //
            }
            else if (_xn == 1)
            {

            }
            else
            {
                // 内か外か

            }
        }

        static Vector3? IntersectSegments(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d)
        {
            var deno = Vector3.Cross(b - a, d - c).magnitude;
            // point error = { INF, INF };
            if (deno == 0.0)
            {
                // 線分が平行
                return default;
            }
            var s = Vector3.Cross(c - a, d - c).magnitude / deno;
            var t = Vector3.Cross(b - a, a - c).magnitude / deno;
            if (s < 0.0 || 1.0 < s || t < 0.0 || 1.0 < t)
            {
                // 線分が交差していない
                return default;
            }

            return new Vector3(
                a.x + s * (b - a).x,
                a.y + s * (b - a).y,
                a.z + s * (b - a).z
             );
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
            Gizmos.color = Color.red;
            if (_xa.HasValue)
            {
                Gizmos.DrawSphere(_xa.Value, 0.01f);
            }
            if (_xb.HasValue)
            {
                Gizmos.DrawSphere(_xb.Value, 0.01f);
            }
            if (_xc.HasValue)
            {
                Gizmos.DrawSphere(_xc.Value, 0.01f);
            }

            if (_capsule.HasValue && _xn > 0)
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