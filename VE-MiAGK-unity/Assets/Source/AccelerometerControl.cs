using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AccelerometerControl: MonoBehaviour
{
	public UnityEvent onStepRight = new UnityEvent();
	public UnityEvent onStepLeft = new UnityEvent();
	public UnityEvent onJump = new UnityEvent();
	public UnityEvent onDodge = new UnityEvent();

	public Text accText = null;

	void Update()
    {
		if(Input.acceleration.z > 0.6f) onJump.Invoke();
		else if(Input.acceleration.z < -0.6f) onDodge.Invoke();
		if(Input.acceleration.x < -0.15f) onStepLeft.Invoke();
		else if(Input.acceleration.x > 0.15f) onStepRight.Invoke();

		Vector3 vector = new Vector3(
								  Mathf.Round(Input.acceleration.x * 1000) / 10,
								  Mathf.Round(Input.acceleration.y * 1000) / 10,
								  Mathf.Round(Input.acceleration.z * 1000) / 10);

		accText.text = $"x: {vector.x}\ny: {vector.y}\nz: {vector.z}";
    }
}
