using UnityEngine;


namespace StrandCloth
{
    public class ParticleCollider : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        public float Radius = 0.05f;

        public Transform Tail;

        public Ray? HeadTailRay => Tail == null ? null : new Ray { origin = transform.position, direction = (Tail.position - transform.position) };

        public float CapsuleLength => Tail == null ? 0 : Vector3.Distance(Tail.position, transform.position);

        public bool SolidGizmo = false;

        public void Reset()
        {
            if (transform.childCount > 0)
            {
                Tail = transform.GetChild(0);
            }
        }

        /// <summary>
        /// collide sphere a and sphere b.
        /// move sphere b to resolved if collide.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="ra"></param>
        /// <param name="b"></param>
        /// <param name="ba"></param>
        /// <param name="resolved"></param>
        /// <returns></returns>
        static bool TryCollideSphereAndSphere(
            in Vector3 a, float ra,
            in Vector3 b, float rb,
            out Vector3 resolved
            )
        {
            if (Vector3.Distance(a, b) > (ra + rb))
            {
                resolved = default;
                return false;
            }
            Vector3 normal = (b - a).normalized;
            resolved = a + normal * (ra + rb);
            return true;
        }

        /// <summary>
        /// collide capsule and sphere b.
        /// move sphere b to resolved if collide.
        /// </summary>
        /// <param name="capsuleHead"></param>
        /// <param name="capsuleTail"></param>
        /// <param name="capsuleRadius"></param>
        /// <param name="b"></param>
        /// <param name="rb"></param>
        static bool TryCollideCapsuleAndSphere(
            in Vector3 capsuleHead,
            in Vector3 capsuleTail,
            float capsuleRadius,
            in Vector3 b,
            float rb,
            out Vector3 resolved
            )
        {
            var P = (capsuleTail - capsuleHead).normalized;
            var Q = b - capsuleHead;
            var dot = Vector3.Dot(P, Q);
            if (dot <= 0)
            {
                // head側半球の球判定
                return TryCollideSphereAndSphere(capsuleHead, capsuleRadius, b, rb, out resolved);
            }

            var t = dot / P.magnitude;
            if (t >= 1.0f)
            {
                // tail側半球の球判定
                return TryCollideSphereAndSphere(capsuleTail, capsuleRadius, b, rb, out resolved);
            }

            // head-tail上の m_transform.position との最近点
            var p = capsuleHead + P * t;
            return TryCollideSphereAndSphere(p, capsuleRadius, b, rb, out resolved);
        }

        public bool TryCollide(in Vector3 p, float radius, out Vector3 resolved)
        {
            if (Tail != null)
            {
                return TryCollideCapsuleAndSphere(transform.position, Tail.position, this.Radius, p, radius, out resolved);
            }
            else
            {
                return TryCollideSphereAndSphere(transform.position, this.Radius, p, radius, out resolved);
            }
        }

        public static void DrawCapsuleGizmo(Vector3 start, Vector3 end, float radius)
        {
            var tail = end - start;
            var distance = (end - start).magnitude;
            Gizmos.matrix = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.forward, tail), Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.DrawWireSphere(Vector3.forward * distance, radius);
            var capsuleEnd = Vector3.forward * distance;
            var offsets = new Vector3[] { new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Gizmos.DrawLine(offsets[i] * radius, capsuleEnd + offsets[i] * radius);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }

        public void OnDrawGizmos()
        {
            if (SolidGizmo)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(transform.position, Radius);
            }

            Gizmos.color = Color.yellow;
            if (transform.parent)
            {
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
            if (Tail != null)
            {
                DrawCapsuleGizmo(transform.position, Tail.position, Radius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, Radius);
            }
        }
    }
}