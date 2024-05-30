using System;
using System.Collections.Generic;
using System.Linq;
using SphereTriangle;
using UnityEngine;


namespace RotateParticle
{
    [DisallowMultipleComponent]
    public class RotateParticleSystem : MonoBehaviour
    {
        // TODO: param to _strandGroups
        [SerializeField]
        public SimulationEnv Env = new();

        [SerializeField]
        public List<StrandGroup> _strandGroups = new List<StrandGroup>();

        [SerializeField]
        public List<ColliderGroup> _colliderGroups = new();

        [Range(0, 1)]
        public float _clothFactor = 0.5f;

        // runtime
        bool _initialized = false;
        List<Strand> _strands = new List<Strand>();
        public ParticleList _list = new();
        public List<(SpringConstraint, ClothRectCollision)> _clothRects = new();

        public PositionList _newPos;
        Vector3[] _restPositions;

        static Color[] Colors = new Color[]
        {
            Color.yellow,
            Color.green,
            Color.magenta,
        };

        Color GetGizmoColor(ColliderGroup g)
        {
            for (int i = 0; i < _colliderGroups.Count; ++i)
            {
                if (_colliderGroups[i] == g)
                {
                    return Colors[i];
                }
            }

            return Color.gray;
        }

        public void AddColliderIfNotExists(string groupName, Transform head, Transform tail, float radius)
        {
            ColliderGroup group = default;
            foreach (var g in _colliderGroups)
            {
                if (g.Name == groupName)
                {
                    group = g;
                    break;
                }
            }
            if (group == null)
            {
                group = new ColliderGroup { Name = groupName };
                _colliderGroups.Add(group);
            }

            foreach (var collider in group.Colliders)
            {
                if (collider.transform == head)
                {
                    return;
                }
            }

            var c = GetOrAddComponent<SphereCapsuleCollider>(head.gameObject);
            c.Tail = tail;
            c.Radius = radius;
            c.GizmoColor = GetGizmoColor(group);
            group.Colliders.Add(c);
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

        HashSet<int> _clothUsedParticles = new();

        public void InitializeCloth(
            StrandGroup g,
            ParticleList list,
            List<Strand> strands,
            List<(SpringConstraint, ClothRectCollision)> clothRects)
        {
            if (g.Connection == StrandConnectionType.Cloth || g.Connection == StrandConnectionType.ClothLoop)
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
                        _clothUsedParticles.Add(a.Init.Index);
                        _clothUsedParticles.Add(b.Init.Index);
                        _clothUsedParticles.Add(c.Init.Index);
                        _clothUsedParticles.Add(d.Init.Index);
                        if (i % 2 == 1)
                        {
                            // 互い違いに
                            // abcd to badc
                            (a, b) = (b, a);
                            (c, d) = (d, c);
                        }
                        clothRects.Add((
                            new SpringConstraint(
                                list._particles[a.Init.Index],
                                list._particles[b.Init.Index]),
                            new ClothRectCollision(
                                a.Init.Index, b.Init.Index, c.Init.Index, d.Init.Index)));
                    }
                }
                if (strands.Count >= 3)
                {
                    if (g.Connection == StrandConnectionType.ClothLoop)
                    {
                        var i = strands.Count;
                        var s0 = strands.Last();
                        var s1 = strands.First();
                        for (int j = 1; j < s0.Particles.Count && j < s1.Particles.Count; ++j)
                        {
                            var a = s0.Particles[j];
                            var b = s1.Particles[j];
                            var c = s1.Particles[j - 1];
                            var d = s0.Particles[j - 1];
                            _clothUsedParticles.Add(a.Init.Index);
                            _clothUsedParticles.Add(b.Init.Index);
                            _clothUsedParticles.Add(c.Init.Index);
                            _clothUsedParticles.Add(d.Init.Index);
                            if (i % 2 == 1)
                            {
                                // 互い違いに
                                // abcd to badc
                                (a, b) = (b, a);
                                (c, d) = (d, c);
                            }
                            clothRects.Add((
                                new SpringConstraint(
                                    list._particles[a.Init.Index],
                                    list._particles[b.Init.Index]),
                                new ClothRectCollision(
                                    a.Init.Index, b.Init.Index, c.Init.Index, d.Init.Index
                                )
                            ));
                        }
                    }
                }
            }
        }

        public void Initialize()
        {
            _initialized = true;
            foreach (var g in _strandGroups)
            {
                if (g.Roots?.Count == 0)
                {
                    continue;
                }
                var strands = new List<Strand>();
                foreach (var root in g.Roots)
                {
                    var strand = _list.MakeParticleStrand(Env, root, g.DefaultStrandRaius, g.CollisionMask);
                    strands.Add(strand);
                }

                InitializeCloth(g, _list, strands, _clothRects);
                _strands.AddRange(strands);
            }

            _newPos = new(_list._particles.Count);
            _list.EndInitialize(_newPos.Init);
            _restPositions = new Vector3[_list._particles.Count];
            _newPos.EndInitialize();

            foreach (var (s, c) in _clothRects)
            {
                c.InitializeColliderSide(_newPos, _colliderGroups);
            }
        }

        void Start()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// すべての Particle を Init 状態にする。
        /// Verlet の Prev を現在地に更新する(速度0)。
        /// </summary>
        public void ResetParticle()
        {
            foreach (var strand in _strands)
            {
                strand.Reset(_list._particleTransforms);
            }
        }

        public void Update()
        {
            if (Time.deltaTime == 0)
            {
                return;
            }
            Process(Time.deltaTime);
        }

        public void Process(float deltaTime)
        {
            using var profile = new ProfileSample("RotateParticle");

            using (new ProfileSample("UpdateRoot"))
            {
                //
                // input
                //
                // 各strandのrootの移動と回転を外部から入力する。
                // それらを元に各 joint の方向を元に戻した場合の戻り位置を計算する
                foreach (var strand in _strands)
                {
                    strand.UpdateRoot(_list._particleTransforms, _newPos, _restPositions);
                }
            }

            using (new ProfileSample("Verlet"))
            {
                //
                // particle simulation
                //
                // verlet 積分
                var time = new FrameTime(deltaTime);
                _list.BeginFrame(Env, time, _restPositions);
                foreach (var (spring, collision) in _clothRects)
                {
                    // cloth constraint
                    spring.Resolve(time, _clothFactor);
                }
                _list.Verlet(Env, time, _newPos.Init);
                // 長さで拘束
                foreach (var strand in _strands)
                {
                    strand.ForceLength(_list._particleTransforms, _newPos);
                }
            }

            // collision
            using (new ProfileSample("Collision"))
            {
                for (int i = 0; i < _colliderGroups.Count; ++i)
                {
                    var g = _colliderGroups[i];


                    foreach (var (spring, rect) in _clothRects)
                    {
                        using var prof = new ProfileSample("Collision: Cloth");
                        // 頂点 abcd は同じ CollisionMask
                        if (_list._particles[rect._a].Init.CollisionMask.HasFlag((CollisionGroupMask)(i + 1)))
                        {
                            // cloth
                            rect.Collide(_newPos, g.Colliders);
                        }
                    }

                    for (int j = 0; j < _list._particles.Count; ++j)
                    {
                        using var prof = new ProfileSample("Collision: Strand");
                        if (_clothUsedParticles.Contains(j))
                        {
                            // 布で処理された
                            continue;
                        }

                        var particle = _list._particles[j];
                        if (particle.Init.Mass == 0)
                        {
                            continue;
                        }

                        // 紐の当たり判定
                        if (particle.Init.CollisionMask.HasFlag((CollisionGroupMask)(i + 1)))
                        {
                            var p = _newPos.Get(j);
                            foreach (var c in g.Colliders)
                            {
                                // strand
                                if (c != null && c.TryCollide(p, particle.Init.Radius, out var resolved))
                                {
                                    _newPos.CollisionMove(particle.Init.Index, resolved, c.Radius);
                                }
                            }
                        }
                    }
                }
            }

            using (new ProfileSample("Apply"))
            {
                for (int i = 0; i < _newPos.CollisionCount.Length; ++i)
                {
                    if (_newPos.CollisionCount[i] > 0)
                    {
                        _list._particles[i].HasCollide = true;
                    }
                }
                var result = _newPos.Resolve();
                //
                // apply result
                //
                // apply positions and
                // calc rotation from positions recursive
                foreach (var strand in _strands)
                {
                    strand.Apply(_list._particleTransforms, result);
                }
            }
        }

        void OnDrawGizmos()
        {
            _list.DrawGizmos();

            foreach (var (spring, rect) in _clothRects)
            {
                rect.DrawGizmos();
            }

            if (_newPos != null)
            {
                _newPos.DrawGizmos();
            }
        }
    }
}