using UnityEngine;
using System.Collections;
public enum Weather
{
	rain,
	heavyRain,
	Fog
}
public class WeatherManager : MonoBehaviour
{
	public static WeatherManager Instance = null;
	void Awake()
	{
		Instance = this;
	}
	void RemoveOldWeather()
	{

	}
	void StartNewWeather(int newWeather)
	{
		RemoveOldWeather();
		switch (newWeather)
		{
			case (int)Weather.rain:
				SetRain();
				break;
			case (int)Weather.heavyRain:
				SetHeavyRain();
				break;
			case (int)Weather.Fog:
				SetFog();
				break;
			default:
				Debug.LogWarning("NO WEATHER GIVEN");
				break;
		}
	}
	void SetFog()
	{
		Debug.Log("setting fog");
	}
	void SetRain()
	{
		Debug.Log("setting rain");
	}
	void SetHeavyRain()
	{
		Debug.Log("setting heavy rain");
	}
}
