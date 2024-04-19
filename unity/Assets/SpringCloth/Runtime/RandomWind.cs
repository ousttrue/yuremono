using UnityEngine;
using System.Collections;


namespace SpringCloth
{
	public class RandomWind : MonoBehaviour
	{
		private Himo springBones;

		private bool isChecked = true;

		// Use this for initialization
		void Start()
		{
			springBones = GetComponent<Himo>();
		}

		// Update is called once per frame
		void Update()
		{
			Vector3 force = Vector3.zero;
			if (isChecked)
			{
				force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f, 0, 0);
			}
			springBones.Param.ExternalForce = force;
		}

		void OnGUI()
		{
			Rect rect1 = new Rect(10, 10, 400, 30);
			isChecked = GUI.Toggle(rect1, isChecked, "Wind!");
		}
	}
}