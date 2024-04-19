using UnityEngine;
using System.Collections;

public class RandomWind : MonoBehaviour
{
	private SpringBone[] springBones;

	private bool isChecked = true;

	// Use this for initialization
	void Start()
	{
		springBones = GetComponent<SpringManager>().springBones;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 force = Vector3.zero;
		if (isChecked)
		{
			force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f, 0, 0);
		}

		for (int i = 0; i < springBones.Length; i++)
		{
			springBones[i]._springForce = force;
		}
	}

	void OnGUI()
	{
		Rect rect1 = new Rect(10, 10, 400, 30);
		isChecked = GUI.Toggle(rect1, isChecked, "Wind!");
	}

}
