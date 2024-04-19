using System;
using System.Collections.Generic;
using UnityEngine;


namespace SpringCloth
{
    [Serializable]
    public class StrandParam
    {
        [SerializeField, Range(0, 12)]
        public float Stiffness = 1.0f;

        [SerializeField, Range(0, 1)]
        public float DragRatio = 0.1f;

        /// <summary>
        /// 重力の場合 mass を乗算しておくべき？
        /// </summary>
        [SerializeField]
        public Vector3 ExternalForce = new Vector3(0, -0.1f, 0);
    }

    public class Strand
    {
        public List<StrandParticle> Particles = new List<StrandParticle>();

        // Start is called before the first frame update
        public Strand(Transform transform)
        {
            foreach (Transform child in transform)
            {
                InitRecursive(child);
            }
        }

        // // Update is called once per frame
        // void Update()
        // {
        //     foreach (var p in Particles)
        //     {
        //         p.Simulation(Time.deltaTime, Param, m_colliders);
        //     }
        // }

        /**
        * TODO: 初期化
        * - Editor は Reset
        * - Runtime は Start に Reset で作ったデータが無かったら
        */
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
