using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StrandCloth
{
    public class SpringClothSystem : MonoBehaviour
    {
        [SerializeField, Range(0, 1)]
        public float Stiffness = 0.01f;

        [SerializeField, Range(0, 1)]
        public float DragRatio = 0.4f;

        /// <summary>
        /// 重力の場合 mass を乗算しておくべき？
        /// </summary>
        [SerializeField]
        public Vector3 ExternalForce = new Vector3(0, -0.001f, 0);

        public enum ConnectionType
        {
            Cloth,
            ClothLoop,
            Strand,
        }

        [Serializable]
        public class StrandGroup
        {
            public string Name;
            public ConnectionType Connection;
            public List<Transform> _roots = new List<Transform>();
        }

        [SerializeField]
        public List<StrandGroup> _groups = new List<StrandGroup>();

        List<Strand> _strands = new List<Strand>();

        public List<ParticleCollider> _colliders = new List<ParticleCollider>();

        public bool Cloth = false;

        List<ClothConstraint> _constraints = new List<ClothConstraint>();

        [Range(0.001f, 0.5f)]
        public float _defaultStrandRaius = 0.01f;

        public void Start()
        {
            var strands = new List<Strand>();
            foreach (var g in _groups)
            {
                strands.Clear();
                foreach (var root in g._roots)
                {
                    strands.Add(new Strand(root, _defaultStrandRaius));
                }

                if (g.Connection == ConnectionType.Cloth || g.Connection == ConnectionType.ClothLoop)
                {
                    for (int i = 1; i < strands.Count; ++i)
                    {
                        var s0 = strands[i - 1];
                        var s1 = strands[i];
                        for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                        {
                            _constraints.Add(new ClothConstraint(s0.Particles[j], s1.Particles[j], s1.Particles[j - 1], s0.Particles[j - 1]));
                        }
                    }
                    if (strands.Count >= 3)
                    {
                        if (g.Connection == ConnectionType.ClothLoop)
                        {
                            var s0 = strands.Last();
                            var s1 = strands.First();
                            for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                            {
                                _constraints.Add(new ClothConstraint(s0.Particles[j], s1.Particles[j], s1.Particles[j - 1], s0.Particles[j - 1]));
                            }
                        }
                    }
                }
                _strands.AddRange(strands);
            }
        }

        public void Update()
        {
            // EachSolver();
            PhaseSolver();
        }

        void EachSolver()
        {
            var stepForce = ExternalForce;
            foreach (var spring in _strands)
            {
                foreach (var p in spring.Particles)
                {
                    p.AddStiffnessForce(Time.deltaTime, Stiffness);
                    p.AddForce(stepForce);
                    var newPos = p.ApplyVerlet(DragRatio);
                    newPos = p.Collision(newPos, _colliders, p.Constraint);
                    p.ApplyRotationFromPosition(newPos);
                }
            }
        }

        void PhaseSolver()
        {
            // Force を積算する
            foreach (var spring in _strands)
            {
                foreach (var p in spring.Particles)
                {
                    // 剛性
                    p.AddStiffnessForce(Time.deltaTime, Stiffness);
                    // 重力や風
                    p.AddForce(ExternalForce);
                }
            }

            if (Cloth)
            {
                foreach (var c in _constraints)
                {
                    c.ResolveConstraint(1.0f);
                }
            }

            var posMap = new Dictionary<StrandParticle, Vector3>();
            foreach (var s in _strands)
            {
                foreach (var p in s.Particles)
                {
                    var newPos = p.ApplyVerlet(DragRatio);
                    posMap.Add(p, newPos);
                }
            }

            if (_constraints.Count > 0 && Cloth)
            {
                foreach (var collider in _colliders)
                {
                    foreach (var c in _constraints)
                    {
                        c.Collide(collider, posMap);
                    }
                }
            }
            else
            {
                var keys = posMap.Keys.ToArray();
                foreach (var p in keys)
                {
                    posMap[p] = p.Collision(posMap[p], _colliders, p.Constraint);
                }
            }

            foreach (var (p, newPos) in posMap)
            {
                p.ApplyRotationFromPosition(newPos);
            }
        }

        public void OnDrawGizmos()
        {
            if (Cloth)
            {
                foreach (var c in _constraints)
                {
                    c.DrawGizmo();
                }
            }
        }
    }
}