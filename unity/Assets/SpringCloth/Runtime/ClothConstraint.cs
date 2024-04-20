using System;
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
        SpringConstraint ab;
        SpringConstraint ac;
        SpringConstraint bd;

        public ClothConstraint(StrandParticle a, StrandParticle b, StrandParticle c, StrandParticle d)
        {
            ab = new SpringConstraint(a, b);
            ac = new SpringConstraint(a, c);
            bd = new SpringConstraint(b, d);
        }

        // TODO: 三角形 a-b-(d+c/2) と衝突
        public Vector3 Collision(Vector3 newPos, Func<Vector3, Vector3> constraint)
        {
            return newPos;
        }

        public void ResolveConstraint(float hookean)
        {
            // 1 だと暴れた
            ab.Resolve(0.3f);
            ac.Resolve(0.3f);
            bd.Resolve(0.3f);
        }

        public void DrawGizmo()
        {
            ab.DrawGizmo();
            ac.DrawGizmo();
            bd.DrawGizmo();
        }
    }
}