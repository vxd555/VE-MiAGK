using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
	private float currentTime;
	private int currentTimeInt;
	private TextMeshProUGUI timerText;
	private bool started = false;

	public TextMeshProUGUI parentText;
	public float finishScale = 3f;
	public float finishFadeOutTime = 1f;
	public Vector2 finishPosition = new Vector2(100, 100);

	private void Start()
	{
		timerText = GetComponent<TextMeshProUGUI>();
	}

	void Update()
	{
		if (started)
		{
			currentTime += Time.deltaTime;

			if ((int)currentTime > currentTimeInt)
			{
				currentTimeInt = (int)currentTime;

				timerText.text = $"{currentTimeInt / 60:00}:{currentTimeInt % 60:00}";
			}
		}
	}

	public void ToggleTime(bool value)
	{
		started = value;
	}

	private IEnumerator Scale()
	{
		// animation will take one second
		float elapsedTime = 0;

		float beginWidth = timerText.rectTransform.rect.width;
		float beginHeigth = timerText.rectTransform.rect.height;
		float beginFontSize = timerText.fontSize;

		while (elapsedTime < finishFadeOutTime)
		{
			float rectWidth = Mathf.Lerp(timerText.rectTransform.rect.width, beginWidth * finishScale, (elapsedTime / finishFadeOutTime));
			float rectHeight = Mathf.Lerp(timerText.rectTransform.rect.height, beginHeigth * finishScale, (elapsedTime / finishFadeOutTime));
			timerText.rectTransform.sizeDelta = new Vector2(rectWidth, rectHeight);

			float x = Mathf.Lerp(timerText.rectTransform.position.x, finishPosition.x, (elapsedTime / finishFadeOutTime));
			float y = Mathf.Lerp(timerText.rectTransform.position.y, finishPosition.y, (elapsedTime / finishFadeOutTime));
			timerText.rectTransform.position = new Vector3(x, y);

			timerText.fontSize = Mathf.Lerp(timerText.fontSize, beginFontSize * finishScale, (elapsedTime / finishFadeOutTime));

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	public void ScaleToCenter()
	{
		parentText.CrossFadeAlpha(0f, finishFadeOutTime, true);
		StartCoroutine(Scale());
	}
}
