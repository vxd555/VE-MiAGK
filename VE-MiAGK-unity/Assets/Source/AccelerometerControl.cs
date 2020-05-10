using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccelerometerControl: MonoBehaviour
{
	public Text accText = null;

    void Update()
    {
		Vector3 vector = new Vector3(
								  Mathf.Round(Input.acceleration.x * 1000) / 10,
								  Mathf.Round(Input.acceleration.y * 1000) / 10,
								  Mathf.Round(Input.acceleration.z * 1000) / 10);

		accText.text = $"x: {vector.x}\ny: {vector.y}\nz: {vector.z}";
    }
}
