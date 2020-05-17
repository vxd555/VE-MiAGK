using System.Collections;
using TMPro;
using UnityEngine;

public class UIIndicatorFlash : MonoBehaviour
{
	private TextMeshProUGUI text;

	public float fadeOutTime = .6f;
	public float fadeInTime = .2f;

	private void Awake()
	{
        text = GetComponent<TextMeshProUGUI>();		
	}

	private void OnEnable()
    {
		StartCoroutine(Flash());
    }

	IEnumerator Flash()
	{
		text.CrossFadeAlpha(1f, fadeInTime, true);
		yield return new WaitForSeconds(0.2f);
		text.CrossFadeAlpha(0f, fadeOutTime, true);
	}
}
