﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class MovementSystem : MonoBehaviour
{
	public GyroControl gyroControl;
	public AccelerometerControl accelerometerControl;
	public CinemachineDollyCart cinemachine;
	
	bool end = false;

	[SerializeField]
	bool isJump = false; //czy masz podniesioną głowę czy nie
	bool isFly = false; //czy jesteś w powietrzu po skoku
	bool isDodge = false; //czy masz obniżoną głowę
	bool isSlide = false; //czy jesteś w ślizgu po uniku
	int step = -1;

	//parametry przemieszczania gracza
	[Header("movement")]
	public float forceCurrent = 0; //obecna siła popychania
	public float forceAdd = 0.25f; //siła nadawana przy pojedyńczym kroku;
	public float forceFalling = 0.05f; //siła o jaką spada
	public float forceMax = 0.6f; //siła nadawana przy
	public float forceJump = 0.3f; //siła nadawana przy

	[Header("Fields")]
	public List<CinemachineSmoothPath> fall = new List<CinemachineSmoothPath>();
	public List<Transform> dodge = new List<Transform>();

	[Header("Information")]
	public Image            jumpIndicator;
	public Image            dodgeIndicator;
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

	// Update is called once per frame
	void Update()
    {
		if(end) return;

		if(CheckJump())
		{
			StartCoroutine(JumpTime());
		}
		cinemachine.m_Speed = forceCurrent;

		if(Input.GetKeyDown(KeyCode.RightArrow)) StepRight();
		if(Input.GetKeyDown(KeyCode.LeftArrow)) StepLeft();
		if(Input.GetKeyDown(KeyCode.UpArrow)) Jump();
		if(Input.GetKeyDown(KeyCode.DownArrow)) Dodge();
	}

	void FixedUpdate()
	{
		if(end) return;

		if(isFly || isSlide) forceCurrent = forceJump;
		else
		{
			forceCurrent -= forceFalling;
			if(forceCurrent < 0) forceCurrent = 0;
		}
	}

	void End() //zakończenie gry
	{
		end = true;
		cinemachine.m_Speed = 0;
		info.text = "end game";
	}

	bool CheckJump()
	{
		foreach(var i in fall)
		{
			if(Vector3.Distance(transform.position, i.transform.position) < 0.9f) return true;
		}
		return false;
	}
	bool CheckDodge()
	{
		foreach(var i in dodge)
		{
			if(Vector3.Distance(transform.position, i.transform.position) < 0.9f) return true;
		}
		return false;
	}

	void Jump() //rozpoczęcie skoku
	{
		if(CheckJump()) return;
		if(isJump || isDodge) return;
		isJump = true;
		isFly = true;
		info.text = "jump";
	}
	void JumpOut()
	{
		if(!isJump) return;
		isJump = false;
		info.text = "out jump";
	}

	void Dodge() //rozpoczęcie wślizgu
	{
		if(CheckDodge()) return;
		if(isJump || isDodge) return;
		isDodge = true;
		isSlide = true;
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
		if(step == 1) return;
		step = 1;
		Move();
		info.text = "move left";
	}

	void StepRight()
	{
		if(step == 0) return;
		step = 0;
		Move();
		info.text = "move right";
	}

	void Move()
	{
		forceCurrent = Mathf.Clamp(forceCurrent + forceAdd, 0f, forceMax);
	}

	IEnumerator JumpTime()
	{
		jumpIndicator.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		jumpIndicator.gameObject.SetActive(false);

		if(isFly)
		{
			StartCoroutine(EndFlyTime());
		}
		else
		{
			End();
			cinemachine.m_Path = fall[0];
			cinemachine.m_Position = 0f;
			cinemachine.m_Speed = forceJump;
		}
	}

	IEnumerator EndFlyTime()
	{
		yield return new WaitForSeconds(2f);
		isFly = false;
	}
}
