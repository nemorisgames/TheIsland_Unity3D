using UnityEngine;
using System.Collections;
public enum Weather
{
	rain_low,
	rain_normal,
	rain_heavy,
	rain_storm
}
public enum WeatherEffects
{
	flash,
	thunder,
}
public class WeatherManager : MonoBehaviour
{
	public static WeatherManager Instance = null;
	public ParticleSystem particles;
	public AudioClip[] weather_Sounds;
	public AudioClip[] weather_effects;
	void Awake()
	{
		Instance = this;
	}
	void RemoveOldWeather()
	{

	}
	void StartNewWeather(int newWeather)
	{
		//RemoveOldWeather();
		switch (newWeather)
		{
			case 0:
				//particles.emission.rate = 10f;
				break;
			case 1:
				break;
			case 2:
				break;
			case 3:
				break;
			case 4:
				break;
			default:
				break;
		}
	}
}
