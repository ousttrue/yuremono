using System;
using System.Collections.Generic;
using UnityEngine;


namespace SpringCloth
{
    public class Strand
    {
        public List<StrandParticle> Particles = new List<StrandParticle>();

        /// <summary>
        ///  TODO: 初期化
        ///  - Editor は Reset
        ///  - Runtime は Start に Reset で作ったデータが無かったら
        /// </summary>
        /// <param name="transform"></param>
        public Strand(Transform transform, float radius)
        {
            InitRecursive(transform, 0.0f, radius);
        }

        void InitRecursive(Transform t, float mass, float radius)
        {
            var p = t.gameObject.AddComponent<StrandParticle>();
            p.Setup(radius, mass);

            Particles.Add(p);

            foreach (Transform child in t)
            {
                InitRecursive(child, 1.0f, radius);
            }
        }
    }
}