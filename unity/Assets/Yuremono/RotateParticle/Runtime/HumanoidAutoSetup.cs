using System.Collections.Generic;
using System.Linq;
using SphereTriangle;
using UnityEngine;


namespace RotateParticle
{
    public class HumanoidAutoSetup : MonoBehaviour
    {
        static (string group, HumanBodyBones head, HumanBodyBones tail, float radius)[] Capsules = new[]
        {
            ("Leg", HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, 0.06f),
            ("Leg", HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, 0.05f),
            ("Leg", HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, 0.06f),
            ("Leg", HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, 0.05f),

            ("Arm", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, 0.03f),
            ("Arm", HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, 0.03f),
            ("Arm", HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, 0.02f),
            ("Arm", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, 0.03f),
            ("Arm", HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, 0.03f),
            ("Arm", HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal, 0.02f),
        };

        T GetOrAddComponent<T>() where T : Component
        {
            var t = GetComponent<T>();
            if (t != null)
            {
                return t;
            }
            return gameObject.AddComponent<T>();
        }

        bool TryAddGroup(CollisionGroupMask mask, string name, Animator animator, HumanBodyBones humanBone, string[] targets, StrandConnectionType type,
            out StrandGroup group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                Debug.LogWarning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<Transform> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (var target in targets)
                {
                    if (child.name.ToLower().Contains(target.ToLower()))
                    {
                        transforms.Add(child);
                        break;
                    }
                }
            }
            if (transforms.Count == 0)
            {
                Debug.LogWarning($"{string.Join(',', targets)} not found");
                group = default;
                return false;
            }

            group = new StrandGroup
            {
                Name = name,
                CollisionMask = mask,
                DefaultStrandRaius = 0.02f,
                Connection = type
            };
            group.Roots.AddRange(transforms);

            // sort
            group.Roots.Sort(new TransformSort(bone.position));
            return true;
        }

        bool TryAddGroupChildChild(CollisionGroupMask mask, string name, Animator animator, HumanBodyBones humanBone,
            string[] targets, string[] excludes,
            StrandConnectionType type,
            bool sort,
            out StrandGroup group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                Debug.LogWarning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<Transform> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (Transform childchild in child)
                {
                    if (excludes.Any(x => childchild.name.ToLower().Contains(x.ToLower())))
                    {
                        continue;
                    }

                    foreach (var target in targets)
                    {
                        if (childchild.name.ToLower().Contains(target.ToLower()))
                        {
                            transforms.Add(childchild);
                            break;
                        }
                    }
                }
            }
            if (transforms.Count == 0)
            {
                Debug.LogWarning($"{string.Join(',', targets)} not found");
                group = default;
                return false;
            }

            group = new StrandGroup
            {
                Name = name,
                CollisionMask = mask,
                DefaultStrandRaius = 0.02f,
                Connection = type
            };
            group.Roots.AddRange(transforms);

            // sort
            if (sort)
            {
                group.Roots.Sort(new TransformSort(bone.position));
            }
            return true;
        }

        public void Reset()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("no animator");
                return;
            }
            var avatar = animator.avatar;
            if (!avatar.isHuman)
            {
                Debug.LogWarning("not humanoid");
                return;
            }

            var system = GetOrAddComponent<RotateParticleSystem>();
            system.Env.DragForce = 0.6f;
            system.Env.Stiffness = 0.07f;

            foreach (var (group, head, tail, radius) in Capsules)
            {
                system.AddColliderIfNotExists(group,
                    animator.GetBoneTransform(head), animator.GetBoneTransform(tail), radius);
            }

            // skirt
            {
                if (TryAddGroup(CollisionGroupMask.Group01, "Skirt", animator, HumanBodyBones.Hips, new[] { "skirt", "ｽｶｰﾄ", "スカート" }, StrandConnectionType.ClothLoop,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroup(CollisionGroupMask.Group02, "髪", animator, HumanBodyBones.Head, new[] { "髪", "hair" }, StrandConnectionType.Strand,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroup(CollisionGroupMask.Group01, "裾", animator, HumanBodyBones.Hips, new[] { "裾" }, StrandConnectionType.Cloth,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroupChildChild(CollisionGroupMask.Group02, "左袖", animator, HumanBodyBones.LeftUpperArm, new[] { "袖" }, new[] { "ひじ袖" }, StrandConnectionType.ClothLoop,
                    false,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroupChildChild(CollisionGroupMask.Group02, "左ひじ袖", animator, HumanBodyBones.LeftLowerArm, new[] { "袖" }, new string[] { }, StrandConnectionType.ClothLoop,
                    false,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroupChildChild(CollisionGroupMask.Group02, "右袖", animator, HumanBodyBones.RightUpperArm, new[] { "袖" }, new[] { "ひじ袖" }, StrandConnectionType.ClothLoop,
                    false,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroupChildChild(CollisionGroupMask.Group02, "右ひじ袖", animator, HumanBodyBones.RightLowerArm, new[] { "袖" }, new string[] { }, StrandConnectionType.ClothLoop,
                    false,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
            {
                if (TryAddGroup(CollisionGroupMask.Group01 | CollisionGroupMask.Group02, "マント", animator, HumanBodyBones.Chest, new[] { "マント" }, StrandConnectionType.Cloth,
                    out var g))
                {
                    system._strandGroups.Add(g);
                }
            }
        }
    }
}