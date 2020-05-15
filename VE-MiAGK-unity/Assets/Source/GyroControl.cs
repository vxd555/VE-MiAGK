using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroControl : MonoBehaviour
{
	private bool gyroEnabled;
	private Gyroscope gyro;

	public Transform cameraContainer;
	private Quaternion rot;

    void Start()
    {
		gyroEnabled = EnableGyro();
		cameraContainer = transform.parent;
		//rot = new Quaternion(1, 0, 0, 0);
	}

    private bool EnableGyro()
	{
		if(SystemInfo.supportsGyroscope)
		{
			gyro = Input.gyro;
			gyro.enabled = true;

			rot = new Quaternion(0, 0, 1, 0);

			return true;
		}
		return false;
	}

	public void Update()
	{
		if(gyroEnabled)
		{
			//transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, 0);
			cameraContainer.Rotate(0, -Input.gyro.rotationRateUnbiased.y, 0);
			transform.Rotate(-Input.gyro.rotationRateUnbiased.x, 0, 0);
			//transform.rotation = gyro.attitude * rot;
			/*transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
													transform.eulerAngles.y, 
													transform.eulerAngles.z - 90f);*/
		}

		//transform.rotation = rot;
	}
}
