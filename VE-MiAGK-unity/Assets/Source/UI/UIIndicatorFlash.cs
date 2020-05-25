using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIndicatorFlash : MonoBehaviour
{
	private TextMeshProUGUI text;
	private Image image;

	public float fadeOutTime = .6f;
	public float fadeInTime = .2f;
	public float totalTime = 1f;

	private void Awake()
	{
        text = GetComponent<TextMeshProUGUI>();
		image = GetComponentInChildren<Image>();
	}

	private void OnEnable()
    {
		StartCoroutine(Flash());
    }

	IEnumerator Flash()
	{
		text.CrossFadeAlpha(1f, fadeInTime, true);
		image.CrossFadeAlpha(1f, fadeInTime, true);
		yield return new WaitForSeconds(totalTime - fadeInTime - fadeOutTime);
		text.CrossFadeAlpha(0f, fadeOutTime, true);
		image.CrossFadeAlpha(0f, fadeOutTime, true);
	}
}
