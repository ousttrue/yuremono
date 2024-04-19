using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpringCloth
{
    public class SpringCloth : MonoBehaviour
    {
        public List<Himo> _springs = new List<Himo>();

        public List<ParticleCollider> m_colliders = new List<ParticleCollider>();

        [SerializeField, Range(1, 500000)]
        public float Hookean = 1000.0f;

        public List<SpringConstraint> _constraints = new List<SpringConstraint>();

        bool _initialized = false;

        void Start()
        {
            if (_springs.Count == 0)
            {
                Debug.LogWarning("no springs");
                this.enabled = false;
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_initialized)
            {
                if (_springs.All(x => x.Initialized))
                {
                    Debug.Log($"Springs: {_springs.Count}");

                    // 横向きの拘束を追加
                    for (int i = 1; i < _springs.Count; ++i)
                    {
                        var s0 = _springs[i - 1];
                        var s1 = _springs[i];
                        for (int j = 0; j < s0.Particles.Count; ++j)
                        {
                            _constraints.Add(new SpringConstraint(s0.Particles[j], s1.Particles[j]));
                        }
                    }

                    _initialized = true;
                }
            }

            if (!_initialized)
            {
                return;
            }

            // Hookean
            foreach (var c in _constraints)
            {
                var (p0, p1, f) = c.Resolve(Time.deltaTime, Hookean);
                // TODO: mass で分配
                Debug.Log($"{p0.transform} <=> {p1.transform} = {f}");
                p0.Force = f * 0.5f;
                p1.Force = -f * 0.5f;
            }

            foreach (var spring in _springs)
            {
                foreach (var p in spring.Particles)
                {
                    p.CalcForce(Time.deltaTime, spring.Param, true);
                }
            }

            foreach (var s in _springs)
            {
                foreach (var p in s.Particles)
                {
                    p.ApplyForce(Time.deltaTime, m_colliders);
                }
            }
        }

        void OnDrawGizmos()
        {
            foreach (var c in _constraints)
            {
                c.Draw();
            }
        }
    }
}