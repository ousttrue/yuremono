using UnityEngine;
using System.Collections;


namespace SpringCloth
{
	public class RandomWind : MonoBehaviour
	{
		private SpringCloth springBones;

		private bool isChecked = true;

		[SerializeField, Range(1, 1000)]
		public float Power = 10.0f;

		// Use this for initialization
		void Start()
		{
			springBones = GetComponent<SpringCloth>();
		}

		// Update is called once per frame
		void Update()
		{
			Vector3 force = Vector3.zero;
			if (isChecked)
			{
				force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * Power, 0, 0);
			}

			springBones.ExternalForce = force;
		}

		void OnGUI()
		{
			Rect rect1 = new Rect(10, 10, 400, 30);
			isChecked = GUI.Toggle(rect1, isChecked, "Wind!");
		}
	}
}