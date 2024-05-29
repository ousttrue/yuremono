using System;
using System.Collections.Generic;
using System.Linq;
using SphereTriangle;
using UnityEngine;


namespace StrandCloth
{
    public class StrandClothSystem : MonoBehaviour
    {
        public int ParticleCount = 0;

        [SerializeField, Range(0, 1)]
        public float Stiffness = 0.01f;

        [SerializeField, Range(0, 1)]
        public float DragRatio = 0.4f;

        /// <summary>
        /// 重力の場合 mass を乗算しておくべき？
        /// </summary>
        [SerializeField]
        public Vector3 ExternalForce = new Vector3(0, -0.001f, 0);

        [SerializeField]
        public List<StrandGroup> _groups = new List<StrandGroup>();

        List<Strand> _strands = new List<Strand>();

        public List<SphereCapsuleCollider> _colliders = new List<SphereCapsuleCollider>();

        public bool Cloth = true;
        [Range(0.0f, 2.0f)]
        public float _clothFactor = 1.0f;

        List<(SpringConstraint, ClothRectCollision)> _constraints = new();

        public void AddColliderIfNotExists(Transform head, Transform tail, float radius)
        {
            foreach (var collider in _colliders)
            {
                if (collider.transform == head)
                {
                    return;
                }
            }

            var c = GetOrAddComponent<SphereCapsuleCollider>(head.gameObject);
            c.Tail = tail;
            c.Radius = radius;
            _colliders.Add(c);
        }

        static T GetOrAddComponent<T>(GameObject o) where T : Component
        {
            var t = o.GetComponent<T>();
            if (t != null)
            {
                return t;
            }
            return o.AddComponent<T>();
        }

        public void Init(StrandConnectionType connection,
            List<Strand> strands,
            List<(SpringConstraint, ClothRectCollision)> constraints)
        {
            if (connection == StrandConnectionType.Cloth || connection == StrandConnectionType.ClothLoop)
            {
                for (int i = 1; i < strands.Count; ++i)
                {
                    var s0 = strands[i - 1];
                    var s1 = strands[i];
                    for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                    {
                        // d x x c
                        //   | |
                        // a x-x b
                        var a = s0.Particles[j];
                        var b = s1.Particles[j];
                        var c = s1.Particles[j - 1];
                        var d = s0.Particles[j - 1];
                        if (i % 2 == 1)
                        {
                            // 互い違いに
                            // abcd to badc
                            (a, b) = (b, a);
                            (c, d) = (d, c);
                        }
                        constraints.Add((
                            new SpringConstraint(
                                a,
                                b),
                            new ClothRectCollision(
                                a.Index, b.Index, c.Index, d.Index
                            )));
                    }
                }
                if (strands.Count >= 3)
                {
                    if (connection == StrandConnectionType.ClothLoop)
                    {
                        var s0 = strands.Last();
                        var s1 = strands.First();
                        var i = strands.Count;
                        for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                        {
                            var a = s0.Particles[j];
                            var b = s1.Particles[j];
                            var c = s1.Particles[j - 1];
                            var d = s0.Particles[j - 1];
                            if (i % 2 == 1)
                            {
                                // 互い違いに
                                // abcd to badc
                                (a, b) = (b, a);
                                (c, d) = (d, c);
                            }
                            constraints.Add((
                                new SpringConstraint(
                                    a,
                                    b),
                                new ClothRectCollision(
                                    a.Index, b.Index, c.Index, d.Index
                                )));
                        }
                    }
                }
            }
        }

        public void Start()
        {
            foreach (var g in _groups)
            {
                var strands = new List<Strand>();
                foreach (var root in g.Roots)
                {
                    strands.Add(new Strand(root, g.DefaultStrandRaius));
                    foreach (var p in strands.Last().Particles)
                    {
                        p.Index = ParticleCount++;
                    }
                }

                Init(g.Connection, strands, _constraints);

                _strands.AddRange(strands);
            }
        }

        public void Update()
        {
            //
            // Force を積算する
            //
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
                // ばね拘束
                foreach (var (s, c) in _constraints)
                {
                    s.Resolve(_clothFactor);
                }
            }

            //
            // 位置仮決め
            //
            var posMap = new PositionList(ParticleCount);
            foreach (var s in _strands)
            {
                if (s.Particles.Count > 0)
                {
                    var current = s.Particles[0].transform.parent.position;
                    foreach (var p in s.Particles)
                    {
                        current = p.ApplyVerlet(DragRatio, current);
                        // Debug.Assert(p.Index == posMap.Count);
                        posMap.Init(p.Index, p.Mass, current);
                    }
                }
            }

            //
            // 衝突解決
            //
            if (_constraints.Count > 0 && Cloth)
            {
                foreach (var (s, c) in _constraints)
                {
                    c.Collide(posMap, _colliders);
                }
            }
            else
            {
                foreach (var s in _strands)
                {
                    foreach (var p in s.Particles)
                    {
                        new NotImplementedException();
                        // posMap.Positions[p.Index] = p.Collision(posMap.Positions[p.Index], _colliders);
                    }
                }
            }

            //
            // 位置から回転を確定させる(親から子に再帰的に実行すること)
            //
            var result = posMap.Resolve();
            foreach (var s in _strands)
            {
                foreach (var p in s.Particles)
                {
                    p.ApplyRotationFromPosition(result[p.Index]);
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (Cloth)
            {
                foreach (var (s, c) in _constraints)
                {
                    c.DrawGizmos();
                }
            }
        }
    }
}