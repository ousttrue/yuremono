using System.Collections.Generic;
using UnityEngine;


namespace SpringCloth
{
    public class Himo : MonoBehaviour
    {
        [SerializeField]
        public SpringParam Param;

        public List<Particle> Particles = new List<Particle>();

        public bool Initialized => Particles.Count > 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (Transform child in transform)
            {
                InitRecursive(child);
            }
        }

        /**
        * TODO: 初期化
        * - Editor は Reset
        * - Runtime は Start に Reset で作ったデータが無かったら
        */
        void InitRecursive(Transform t)
        {
            var p = t.gameObject.AddComponent<Particle>();
            p.Setup(0.05f);
            
            Particles.Add(p);

            foreach (Transform child in t)
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
    }
}
