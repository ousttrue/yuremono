using UnityEngine;

public class SpringCollider : MonoBehaviour
{
	//半径
	public float radius = 0.05f;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, radius);
	}

	//衝突判定
	public bool TryCollide(Vector3 p, float radius, out Vector3 resolved)
	{
		if (Vector3.Distance(p, this.transform.position) > (radius + this.radius))
		{
			resolved = default;
			return false;
		}

		Vector3 normal = (p - this.transform.position).normalized;
		resolved = this.transform.position + (normal * (radius + this.radius));
		return true;
	}
}