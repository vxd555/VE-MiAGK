using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;

public class MovementSystem : MonoBehaviour
{
	public UnityEvent onEndGame = new UnityEvent();
	public GyroControl gyroControl;
	public AccelerometerControl accelerometerControl;
	public CinemachineDollyCart cinemachine;
	public Rigidbody rigidbody = null;
	public UITimer timer;

	bool end = false;
	bool jumpTime = false;
	bool isJumping = false; //czy masz podniesioną głowę czy nie
	bool isInAir = false; //czy jesteś w powietrzu po skoku
	bool dodgeTime = false;
	bool isDodging = false; //czy masz obniżoną głowę
	bool isSliding = false; //czy jesteś w ślizgu po uniku
	int	step = -1;
	

	//parametry przemieszczania gracza
	[Header("movement")]
	public float forceCurrent = 0; //obecna siła popychania
	public float forceAdd = 0.25f; //siła nadawana przy pojedyńczym kroku;
	public float forceFalling = 0.05f; //siła o jaką spada
	public float forceMax = 0.6f; //siła nadawana przy
	public float forceJump = 0.3f; //siła nadawana przy

	[Header("Fields")]
	public List<Transform>		fall = new List<Transform>();
	public List<Transform>		dodge = new List<Transform>();

	[Header("Sounds")]
	public AudioSource          snd = null;
	public List<AudioClip>      stepsSound = new List<AudioClip>();
	public AudioClip            jumpSound = null;
	public AudioClip            dodgeSound = null;
	public AudioClip            deadSound = null;

	[Header("Information")]
	public GameObject           jumpIndicator;
	public GameObject           dodgeIndicator;
	public Text					info;

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
		if(CheckDodge())
		{
			StartCoroutine(DodgeTime());
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

		if(isInAir || isSliding) forceCurrent = forceJump;
		else
		{
			forceCurrent -= forceFalling;
			if(forceCurrent < 0) forceCurrent = 0;
		}
	}

	void End() //zakończenie gry
	{
		if(end) return;
		end = true;
		snd.PlayOneShot(deadSound);
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
		if(!jumpTime) return;
		if(isJumping || isDodging) return;
		isJumping = true;
		isInAir = true;
		snd.PlayOneShot(jumpSound);
		info.text = "jump";
	}
	void JumpOut()
	{
		if(!isJumping) return;
		isJumping = false;
		info.text = "out jump";
	}

	void Dodge() //rozpoczęcie wślizgu
	{
		if(!dodgeTime) return;
		if(isJumping || isDodging) return;
		isDodging = true;
		isSliding = true;
		snd.PlayOneShot(dodgeSound);
		info.text = "dodge";
	}

	void DodgeOut()
	{
		if(!isDodging) return;
		isDodging = false;
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
		timer.ToggleTime(true);
		snd.PlayOneShot(stepsSound[Random.Range(0, stepsSound.Count)]);
	}

	IEnumerator JumpTime()
	{
		jumpTime = true;
		jumpIndicator.SetActive(true);
		yield return new WaitForSeconds(0.8f);
		jumpIndicator.SetActive(false);
		jumpTime = false;

		if(isInAir)
		{
			StartCoroutine(EndFlyTime());
		}
		else
		{
			End();
			onEndGame.Invoke();
			cinemachine.m_Path = null;
			rigidbody.useGravity = true;

		}
	}

	IEnumerator EndFlyTime()
	{
		yield return new WaitForSeconds(1f);
		isInAir = false;
#if UNITY_EDITOR
		JumpOut();
#endif
	}

	IEnumerator DodgeTime()
	{
		dodgeTime = true;
		dodgeIndicator.SetActive(true);
		yield return new WaitForSeconds(0.9f);
		dodgeIndicator.SetActive(false);
		dodgeTime = false;

		if(isSliding)
		{
			StartCoroutine(EndSlideTime());
		}
		else
		{
			End();
			onEndGame.Invoke();
			cinemachine.m_Path = null;
			cinemachine.enabled = false;
			rigidbody.useGravity = true;
			rigidbody.AddForce(-Vector3.left * 50);
			//rigidbody.MovePosition(Vector3.left * 10);
		}
	}

	IEnumerator EndSlideTime()
	{
		yield return new WaitForSeconds(1f);
		isSliding = false;

#if UNITY_EDITOR
		DodgeOut();
#endif
	}
}
