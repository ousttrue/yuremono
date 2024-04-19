using UnityEngine;
using System.Collections;

public class SpringManager : MonoBehaviour
{
	public SpringBone[] springBones;

	private void LateUpdate()
	{
		for (int i = 0; i < springBones.Length; i++)
		{
			springBones[i].UpdateSpring();
		}
	}

}
