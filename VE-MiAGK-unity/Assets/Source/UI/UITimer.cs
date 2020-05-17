using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
	private float currentTime;
	private int currentTimeInt;
	private TextMeshProUGUI timerText;

	private void Start()
	{
		timerText = GetComponent<TextMeshProUGUI>();
	}

	void Update()
	{
		currentTime += Time.deltaTime;

		if((int)currentTime > currentTimeInt)
		{
			currentTimeInt = (int)currentTime;

			timerText.text = $"{currentTimeInt / 60:00}:{currentTimeInt % 60:00}";
		}
	}
}
