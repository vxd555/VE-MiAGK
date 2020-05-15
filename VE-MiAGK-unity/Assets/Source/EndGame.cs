using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGame: MonoBehaviour
{
	public CanvasGroup canvasGroup = null;
	public MovementSystem movementSystem = null;

	public float fadeOutTime = 2f;
	public float fadeInTime = 2f;

	float timer = 0f;

	private void Awake()
	{
		canvasGroup.alpha = 1f;
	}

	void Start()
	{
		movementSystem.onEndGame.AddListener(FadeInBegin);

		StartCoroutine(FadeOut());
		
	}


	void FadeInBegin() //koniec gry
	{
		StartCoroutine(FadeIn());
	}
	IEnumerator FadeIn()
	{
		yield return new WaitForSeconds(1f);
		timer = 0f;
		while(timer < fadeInTime)
		{
			timer += Time.deltaTime;
			float ratio = timer / fadeInTime;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, ratio);		
			yield return null;
		}
		canvasGroup.alpha = 1f;

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	IEnumerator FadeOut() //początek gry
	{
		timer = 0f;
		while(timer < fadeOutTime)
		{
			timer += Time.deltaTime;
			float ratio = timer / fadeInTime;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, ratio);
			yield return null;
		}
		canvasGroup.alpha = 0f;
	}
}
