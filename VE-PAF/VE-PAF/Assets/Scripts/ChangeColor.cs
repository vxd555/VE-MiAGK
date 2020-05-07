using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public void SetRed()
	{
		GetComponent<Renderer>().material.color = Color.red;
	}
	public void SetBlue()
	{
		GetComponent<Renderer>().material.color = Color.blue;
	}
	public void SetBlack()
	{
		GetComponent<Renderer>().material.color = Color.black;
	}
}
