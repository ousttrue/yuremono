﻿using UnityEngine;
using System.Collections;


namespace StrandCloth
{
	public class RandomWind : MonoBehaviour
	{
		private StrandClothSystem springBones;

		private bool isChecked = true;

		[Range(0, 1)]
		public float Power = 1;

		// Use this for initialization
		void Start()
		{
			springBones = GetComponent<StrandClothSystem>();
		}

		// Update is called once per frame
		void Update()
		{
			Vector3 force = Vector3.zero;
			if (isChecked)
			{
				force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f * Power, 0, 0);
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