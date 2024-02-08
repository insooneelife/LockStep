using UnityEngine;
using System.Collections.Generic;
using System;

public class CameraLogic : MonoBehaviour 
{
	// For singleton ..
	public static CameraLogic instance = null;

	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}
}
