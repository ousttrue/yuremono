using UnityEngine;

public class RandomWind : MonoBehaviour
{
	private SpringManager springBones;

	private bool isChecked = true;

	void Start()
	{
		springBones = GetComponent<SpringManager>();
	}

	void Update()
	{
		Vector3 force = Vector3.zero;
		if (isChecked)
		{
			force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f, 0, 0);
		}

		springBones._springForce = force;
	}

	void OnGUI()
	{
		Rect rect1 = new Rect(10, 10, 400, 30);
		isChecked = GUI.Toggle(rect1, isChecked, "Wind!");
	}
}