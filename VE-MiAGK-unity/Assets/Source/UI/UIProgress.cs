using Cinemachine;
using TMPro;
using UnityEngine;

public class UIProgress : MonoBehaviour
{
	private float currentDistance;
	private float fullDistance;
	private int currentPercent;
	private TextMeshProUGUI progressText;

	public CinemachineSmoothPath path;
	public CinemachineDollyCart cart;

	private void Start()
	{
		progressText = GetComponent<TextMeshProUGUI>();

		if (path != null)
			fullDistance = path.PathLength;
	}

	void Update()
	{
		currentDistance = cart.m_Position;

		int percent = (int)((currentDistance / fullDistance) * 100);

		if (percent > currentPercent)
		{
			currentPercent = percent;
			progressText.text = $"{percent}%";
		}
	}
}
