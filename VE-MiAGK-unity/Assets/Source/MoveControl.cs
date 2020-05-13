using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveControl : MonoBehaviour
{
	public GyroControl gyroControl;
	public AccelerometerControl accelerometerControl;
	public Path path;

	
	public int currentPath = 0;

	bool end = false;
	public bool isJump = false;
	public bool isDodge = false;
	int step = -1;

	//parametry przemieszczania gracza
	[Header("movement")]
	public float forceCurrent = 0; //obecna siła popychania
	public float forceAdd = 0.25f; //siła nadawana przy pojedyńczym kroku;
	public float forceFalling = 0.05f; //siła o jaką spada
	public float forceMax = 0.6f; //siła nadawana przy

	[Header("Text")]
	public Text info;


	void Start()
    {
		accelerometerControl.onJump.AddListener(Jump);
		accelerometerControl.onDodge.AddListener(Dodge);
		accelerometerControl.onOutJump.AddListener(JumpOut);
		accelerometerControl.onOutDodge.AddListener(DodgeOut);
		accelerometerControl.onStepLeft.AddListener(StepLeft);
		accelerometerControl.onStepRight.AddListener(StepRight);
	}

	void Update()
	{
		if(end) return;

		if(currentPath >= path.path.Count - 1) //zakończneie gry na ostatnim fragmencie
		{
			End();
			return;
		}

		if(Vector3.Distance(transform.position, path.path[currentPath + 1].transform.position) < 0.9f) //przesówanie się miedzy fragmentami drogi
		{
			currentPath += 1;
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
		if(path.path[currentPath + 1].pathType != PathType.jump) return;
		if(isJump || isDodge) return;
		isJump = true;
		info.text = "jump";
	}
	void JumpOut()
	{
		if(!isJump) return;
		isJump = false;
		info.text = "out jump";
	}


	void Dodge()
	{
		if(path.path[currentPath + 1].pathType != PathType.dodge) return;
		if(isJump || isDodge) return;
		isDodge = true;
		info.text = "dodge";
	}

	void DodgeOut()
	{
		if(!isDodge) return;
		isDodge = false;
		info.text = "out dodge";
	}

	void StepLeft()
	{
		if(path.path[currentPath + 1].pathType != PathType.walk) return;
		if(step == 1 || isJump || isDodge) return;
		step = 1;
		Move();
		info.text = "move left";
	}

	void StepRight()
	{
		if(path.path[currentPath + 1].pathType != PathType.walk) return;
		if(step == 0 || isJump || isDodge) return;
		step = 0;
		Move();
		info.text = "move right";
	}

	void Move()
	{
		forceCurrent = Mathf.Clamp(forceCurrent + forceAdd, 0f, 1f);
	}
}
