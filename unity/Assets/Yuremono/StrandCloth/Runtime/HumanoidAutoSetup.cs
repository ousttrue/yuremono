using System.Collections;
using System.Collections.Generic;
using SphereTriangle;
using UnityEngine;


namespace StrandCloth
{
    public class HumanoidAutoSetup : MonoBehaviour
    {
        static (HumanBodyBones head, HumanBodyBones tail, float radius)[] Capsules = new[]
        {
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, 0.06f), (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, 0.06f),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, 0.05f), (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, 0.05f),
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

        void Reset()
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

            var system = GetOrAddComponent<StrandClothSystem>();
            foreach (var (head, tail, radius) in Capsules)
            {
                system.AddColliderIfNotExists(
                    animator.GetBoneTransform(head), animator.GetBoneTransform(tail), radius);
            }

            // skirt
            var skirt = new StrandGroup();
            skirt.Name = "Skirt";
            skirt.DefaultStrandRaius = 0.02f;
            skirt.Connection = StrandConnectionType.ClothLoop;
            system._groups.Add(skirt);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hips != null)
            {
                foreach (Transform child in hips)
                {
                    foreach (Transform t in child)
                    {
                        foreach (var target in new[] { "ｽｶｰﾄ", "スカート", })
                        {
                            if (t.name.Contains(target))
                            {
                                skirt.Roots.Add(t);
                            }
                        }
                    }
                    // sort
                    skirt.Roots.Sort(new TransformSort(child.position));
                }
            }

            // hair
            var hair = new StrandGroup();
            hair.Name = "Hair";
            hair.DefaultStrandRaius = 0.02f;
            system._groups.Add(hair);
        }
    }
}