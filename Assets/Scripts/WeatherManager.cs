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
	public ParticleSystem rainSheet;
	public AudioSource source;
	public AudioClip[] weather_Sounds;
	public AudioClip[] weather_effects;
	public GameObject[] positions;
	int currentWeather = (int)Weather.rain_heavy;
	void Awake()
	{
		Instance = this;
		if (GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>() == null)
		{
			GameObject.FindGameObjectWithTag("Player").AddComponent<AudioSource>();
		}
		source = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
	}
	void RemoveOldWeather()
	{

	}
	void StartNewWeather(int newWeather = -1)
	{
		if (newWeather != -1)
			currentWeather = newWeather;
		var em = particles.emission;
		var rate = new ParticleSystem.MinMaxCurve();
		var emSheet = rainSheet.emission;
		var rateSheet = new ParticleSystem.MinMaxCurve();
		//RemoveOldWeather();
		switch (currentWeather)
		{
			case 0:
				rate.mode = ParticleSystemCurveMode.Constant;
				rate.constant = 100f;
				rateSheet.mode = ParticleSystemCurveMode.Constant;
				rateSheet.constant = 1f;
				Debug.Log("Low Rain");
				break;
			case 1:
				rate.mode = ParticleSystemCurveMode.Constant;
				rate.constant = 1000f;
				rateSheet.mode = ParticleSystemCurveMode.Constant;
				rateSheet.constant = 15f;
				Debug.Log("Normal Rain");
				break;
			case 2:
				rate.mode = ParticleSystemCurveMode.Constant;
				rate.constant = 2000f;
				rateSheet.mode = ParticleSystemCurveMode.Constant;
				rateSheet.constant = 150f;
				Debug.Log("Heavy Rain");
				break;
			case 3:
				rate.mode = ParticleSystemCurveMode.Constant;
				rate.constant = 200f;
				Invoke("ThunderAndFlash", Random.Range(0f, 5f));
				break;
			case 4:
				break;
			default:
				break;
		}
		em.rate = rate;
		emSheet.rate = rateSheet;
	}
	IEnumerator FlashEffect(int pos)
	{
		yield return null;

	}
	IEnumerator ThunderSound(float time, int pos, float volume)
	{
		yield return new WaitForSeconds(time);
		source.volume = volume;
		source.PlayOneShot(weather_effects[pos]);
		Invoke("ThunderAndFlash", Random.Range(3f, 10f));
	}
	void ThunderAndFlash()
	{
		int pos = Random.Range(0, positions.Length);
		//play lightning visual effect
		StartCoroutine(FlashEffect(pos));
		//calculate time based on distance of lightning to Cara
		float time = Random.Range(0f, 3f);
		float volume = 1f;
		StartCoroutine(ThunderSound(time, pos, volume));
	}
	void NextWeather()
	{
		currentWeather++;
		if (currentWeather >= 3)
		{
			currentWeather = 0;
		}
		StartNewWeather();
	}
	void LastWeather()
	{
		currentWeather--;
		if (currentWeather < 0)
			currentWeather = 2;
		StartNewWeather();
	}
	void Update()
	{
		//for debuging
		//if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		//{
		//	if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		//	{
		//		NextWeather();
		//	}
		//	if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		//	{
		//		LastWeather();
		//	}
		//}
	}
}
