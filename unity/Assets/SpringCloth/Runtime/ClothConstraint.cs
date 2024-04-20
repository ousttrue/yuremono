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
            // var a = _a.transform.position;
            // var b = _b.transform.position;
            // var c = (_c.transform.position + _d.transform.position) * 0.5f;
            // var plane = new Plane(a, b, c);
            // // var t = new Triangle()
            // var p = plane.ClosestPointOnPlane(newPos);
            // var distance = Vector3.Distance(p, newPos);
            // if (distance > radius)
            // {
            //     return;
            // }

            // if (!SameSide(p, a, b, c))
            // {
            //     return;
            // }

            // // 4点の移動量
            // var delta = (p - newPos).normalized * (radius - distance);
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
            _ab.DrawGizmo();
            _ac.DrawGizmo();
            _bd.DrawGizmo();
        }
    }
}