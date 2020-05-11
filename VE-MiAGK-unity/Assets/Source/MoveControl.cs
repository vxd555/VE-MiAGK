using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControl : MonoBehaviour
{
	public GyroControl gyroControl;
	public AccelerometerControl accelerometerControl;
	public Path path;

	
	public int currentPath = 0;

	bool end = false;
	bool isJump = false;
	bool isDodge = false;
	int step = -1;

	//parametry przemieszczania gracza
	[Header("movement")]
	public float forceCurrent = 0; //obecna siła popychania
	public float forceAdd = 0.25f; //siła nadawana przy pojedyńczym kroku;
	public float forceFalling = 0.05f; //siła o jaką spada
	public float forceMax = 0.6f; //siła nadawana przy


	void Start()
    {
		accelerometerControl.onJump.AddListener(Jump);
		accelerometerControl.onDodge.AddListener(Dodge);
		accelerometerControl.onStepLeft.AddListener(StepLeft);
		accelerometerControl.onStepRight.AddListener(StepRight);
	}

	void Update()
	{
		if(end) return;

		if(Vector3.Distance(transform.position, path.path[currentPath + 1].transform.position) < 0.9f)
		{
			currentPath += 1;
			if(currentPath >= path.path.Count - 1) End();
		}
		Vector3 direction = (path.path[currentPath + 1].transform.position - path.path[currentPath].transform.position).normalized;
		transform.position += direction * forceCurrent;

		if(Input.GetKeyDown(KeyCode.RightArrow)) StepRight();
		if(Input.GetKeyDown(KeyCode.LeftArrow)) StepLeft();
		if(Input.GetKeyDown(KeyCode.UpArrow)) Jump();
		if(Input.GetKeyDown(KeyCode.DownArrow)) Dodge();
	}

	void FixedUpdate()
    {
		if(end) return;

		forceCurrent -= forceFalling;
		if(forceCurrent < 0) forceCurrent = 0;
	}

	void End()
	{
		end = true;
	}

	void Jump()
	{
		if(isJump || isDodge) return;
		isJump = true;
	}

	void Dodge()
	{
		if(isJump || isDodge) return;
		isDodge = true;
	}

	void StepLeft()
	{
		if(step == 1 || isJump || isDodge) return;
		step = 1;
		Move();
	}

	void StepRight()
	{
		if(step == 0 || isJump || isDodge) return;
		step = 0;
		Move();
	}

	void Move()
	{
		forceCurrent = Mathf.Clamp(forceCurrent + forceAdd, 0f, 1f);
	}
}
