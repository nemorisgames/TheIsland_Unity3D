using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFadeLight : MonoBehaviour {
	Light m_Light = null;

	public float m_MinIntensity = 2.0f;
	public float m_MaxIntensity = 5.0f;
	public float m_Rate = 1.0f;


	/// <summary>
	/// Caches the light.
	/// </summary>
	void Start ()
	{
		m_Light = GetComponent<Light>();
	}


	/// <summary>
	/// Flashes the light up and down by applying a sine wave to its intensity.
	/// </summary>
	void Update ()
	{

		if (m_Light == null)
			return;

		m_Light.intensity = m_MinIntensity + Mathf.Abs(Mathf.Cos((Time.time * m_Rate * Random.Range(0f, 1f))) * (m_MaxIntensity - m_MinIntensity));

	}
}
