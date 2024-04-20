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
        public Strand(Transform transform)
        {
            InitRecursive(transform, 0.0f);
        }

        void InitRecursive(Transform t, float mass)
        {
            var p = t.gameObject.AddComponent<StrandParticle>();
            p.Setup(0.05f, mass);

            Particles.Add(p);

            foreach (Transform child in t)
            {
                InitRecursive(child, 1.0f);
            }
        }
    }
}