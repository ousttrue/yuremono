using UnityEngine;


namespace SpringCloth
{
    public class ParticleCollider : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        public float Radius = 0.1f;

        public bool TryCollide(in Vector3 p, float radius, out Vector3 resolved)
        {
            if (Vector3.Distance(p, transform.position) > (radius + Radius))
            {
                resolved = default;
                return false;
            }
            Vector3 normal = (p - transform.position).normalized;
            resolved = transform.position + normal * (radius + Radius);
            return true;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            if (transform.parent)
            {
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}