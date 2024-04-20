using UnityEngine;

public class SpringManager : MonoBehaviour
{
	/// <summary>
	/// バネが戻る力
	/// </summary>
	[Range(0, 1)]
	public float _stiffnessForce = 0.01f;

	/// <summary>
	/// 速度の減衰
	/// </summary>
	[Range(0, 1)]
	public float _dragForce = 0.4f;

	/// <summary>
	/// 重力・風
	/// </summary>
	public Vector3 _springForce = new Vector3(0.0f, -0.05f, 0.0f);

	public SpringCollider[] _colliders;

	public SpringBone[] springBones;

	private void LateUpdate()
	{
		foreach (var sb in springBones)
		{
			sb.UpdateSpring(_stiffnessForce, _springForce, _dragForce, _colliders);
		}
	}
}