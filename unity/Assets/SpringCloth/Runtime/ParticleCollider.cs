using UnityEngine;


namespace SpringCloth
{
    public class ParticleCollider : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        public float Radius = 0.1f;

        public bool Hit(in Vector3 p, float radius)
        {
            return Vector3.Distance(p, transform.position) <= (radius + Radius);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            if (transform.parent)
            {
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
            Gizmos.DrawSphere(transform.position, Radius);
        }

        public Vector3? Collide(in Vector3 p, float radius)
        {
            if(!Hit(p, radius)){
                return default;
            }
            Vector3 normal = (p - transform.position).normalized;
            return transform.position + (normal * (radius + Radius));
        }
    }
}