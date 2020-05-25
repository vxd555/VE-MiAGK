using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIProgress : MonoBehaviour
{
	private float currentDistance;
	private float fullDistance;
	private int currentPercent;
	private TextMeshProUGUI progressText;
	public TextMeshProUGUI parentText;

	public CinemachineSmoothPath path;
	public CinemachineDollyCart cart;
	public UnityEvent finished = new UnityEvent();
	private bool invoked = false;

	private void Start()
	{
		progressText = GetComponent<TextMeshProUGUI>();

		if (path != null)
			fullDistance = path.PathLength;
	}

	void Update()
	{
		currentDistance = cart.m_Position;

		int percent = Mathf.CeilToInt((currentDistance / fullDistance) * 100);

		if (percent > currentPercent)
		{
			currentPercent = percent;
			progressText.text = $"{percent}%";
		}

		if (percent >= 3 && !invoked)
		{
			finished.Invoke();
			invoked = true;
		}
	}

	public void FadeOut(float fadeOutTime)
	{
		progressText.CrossFadeAlpha(0f, fadeOutTime, true);
		parentText.CrossFadeAlpha(0f, fadeOutTime, true);
	}
}
