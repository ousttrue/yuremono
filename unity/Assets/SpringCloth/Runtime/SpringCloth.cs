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

        public class SpringConstraint
        {
            Particle _p0;

            Particle _p1;

            float _rest;

            public SpringConstraint(Particle p0, Particle p1)
            {
                _p0 = p0;
                _p1 = p1;
                _rest = Vector3.Distance(p0.transform.position, p1.transform.position);
                Debug.Log($"SpringConstraint: {p0.transform}, {p1.transform}");
            }

            public void Draw()
            {
                Gizmos.DrawLine(_p0.transform.position, _p1.transform.position);
            }

            /// <summary>
            ///  フックの法則
            /// </summary>
            /// <returns></returns>
            public (Particle, Particle, Vector3) Resolve(float delta, float hookean)
            {
                // protected _execute(step: number, k: number, shrink: number, stretch: number) {
                // バネの力（スカラー）
                var d = Vector3.Distance(this._p0.transform.position, this._p1.transform.position); // 質点間の距離
                var f = (d - this._rest) * hookean; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
                // f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

                // 変位
                var dx = (this._p1.transform.position - this._p0.transform.position).normalized * f; // * 0.5f * delta * delta;

                // 位置更新（二つの質点を互いに移動させる）
                // const dx_p1 = new THREE.Vector3().copy(dx);
                // dx_p1.multiplyScalar(this._p1._weight / (this._p1._weight + this._p2._weight));
                // this._p1._pos.add(dx_p1);
                // const dx_p2 = new THREE.Vector3().copy(dx);
                // dx_p2.multiplyScalar(this._p2._weight / (this._p1._weight + this._p2._weight));
                // this._p2._pos.sub(dx_p2);

                return (_p0, _p1, dx);
            }
        }

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