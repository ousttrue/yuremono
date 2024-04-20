using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpringCloth
{
    public class SpringClothSystem : MonoBehaviour
    {
        [SerializeField, Range(0, 1)]
        public float Stiffness = 0.1f;

        [SerializeField, Range(0, 1)]
        public float DragRatio = 0.1f;

        /// <summary>
        /// 重力の場合 mass を乗算しておくべき？
        /// </summary>
        [SerializeField]
        public Vector3 ExternalForce = new Vector3(0, -0.1f, 0);

        public List<Transform> _roots = new List<Transform>();

        List<Strand> _springs = new List<Strand>();

        public List<ParticleCollider> _colliders = new List<ParticleCollider>();

        public bool Cloth = false;

        List<SpringConstraint> _constraints = new List<SpringConstraint>();

        public void Start()
        {
            foreach (var root in _roots)
            {
                _springs.Add(new Strand(root));
            }

            Debug.Log($"Springs: {_springs.Count}");

            for (int i = 1; i < _springs.Count; ++i)
            {
                var s0 = _springs[i - 1];
                var s1 = _springs[i];
                for (int j = 0; j < s0.Particles.Count; ++j)
                {
                    _constraints.Add(new SpringConstraint(s0.Particles[j], s1.Particles[j]));
                }
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
            foreach (var spring in _springs)
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
            foreach (var spring in _springs)
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
                    c.Resolve(Time.deltaTime, 1.0f);
                }
            }

            foreach (var s in _springs)
            {
                foreach (var p in s.Particles)
                {
                    var newPos = p.ApplyVerlet(DragRatio);
                    if (Cloth)
                    {
                        foreach (var c in _constraints)
                        {
                            newPos = c.Collision(newPos,  p.Constraint);
                        }
                    }
                    else
                    {
                        newPos = p.Collision(newPos, _colliders, p.Constraint);
                    }
                    p.ApplyRotationFromPosition(newPos);
                }
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