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
            foreach (Transform child in transform)
            {
                InitRecursive(child);
            }
        }

        void InitRecursive(Transform t)
        {
            var p = t.gameObject.AddComponent<StrandParticle>();
            p.Setup(0.05f);

            Particles.Add(p);

            foreach (Transform child in t)
            {
                InitRecursive(child);
            }
        }

    }
}
