using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	public GameObject thunderSpawns;
	public AudioSource[] thunderSounds;
	int currentWeather = (int)Weather.rain_heavy;
	void Awake()
	{
		Instance = this;
		if (GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>() == null)
		{
			GameObject.FindGameObjectWithTag("Player").AddComponent<AudioSource>();
		}
		source = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
		//thunderSounds = thunderSpawns.GetComponentsInChildren<AudioSource>();
	}
	void RemoveOldWeather()
	{

	}
	void StartNewWeather(int newWeather = -1)
	{
		print(newWeather);
		if (newWeather != -1)
			currentWeather = newWeather;
		if (particles != null)
		{
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
					//Debug.Log ("Low Rain");
					break;
				case 1:
					rate.mode = ParticleSystemCurveMode.Constant;
					rate.constant = 1000f;
					rateSheet.mode = ParticleSystemCurveMode.Constant;
					rateSheet.constant = 15f;
					//Debug.Log ("Normal Rain");
					break;
				case 2:
					rate.mode = ParticleSystemCurveMode.Constant;
					rate.constant = 2000f;
					rateSheet.mode = ParticleSystemCurveMode.Constant;
					rateSheet.constant = 150f;
					//Debug.Log ("Heavy Rain");
					break;
				case 3:
					rate.mode = ParticleSystemCurveMode.Constant;
					rate.constant = 200f;
					ThunderAndFlash();
					//Debug.Log ("Thunder");
					break;
				case 4:
					//lluvia interior
					break;
				default:
					break;
			}
			em.rateOverTime = rate;
			emSheet.rateOverTime = rateSheet;
		}
		switch (currentWeather)
		{
			case 0:
				source.clip = weather_Sounds[0];
				source.Play();
				//Debug.Log ("Low Rain");
				break;
			case 1:
				source.clip = weather_Sounds[1];
				source.Play();
				//Debug.Log ("Normal Rain");
				break;
			case 2:
				source.clip = weather_Sounds[2];
				source.Play();
				//Debug.Log ("Heavy Rain");
				break;
			case 3:
				break;
			case 4:
				//lluvia interior
				//print ("interior");
				source.clip = weather_Sounds[3];
				source.Play();
				break;
			default:
				break;
		}
	}
	IEnumerator FlashEffect(int pos)
	{
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(true);
		yield return new WaitForSeconds(0.2f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(false);
		yield return new WaitForSeconds(0.1f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(false);
	}
	IEnumerator ThunderSound(ulong time, int pos, float volume)
	{
		yield return new WaitForSeconds(time);
		thunderSounds[pos].PlayDelayed(time);
		Debug.Log("ThunderSound");
	}
	void ThunderAndFlash(int pos = -1)
	{
		if (pos == -1)
			pos = Random.Range(0, thunderSounds.Length);
		//play lightning visual effect
		StartCoroutine(FlashEffect(pos));
		//calculate time based on distance of lightning to Cara
		float dist = Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, thunderSounds[pos].transform.position);
		float time = Vector3.Distance(thunderSounds[pos].transform.position, GameObject.FindWithTag("Player").transform.position) * 0.05f;
		StartCoroutine(ThunderSound((ulong)time, pos, 1f));
	}
	void NextWeather()
	{
		currentWeather++;
		if (currentWeather > 3)
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
		/*
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				NextWeather();
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				LastWeather();
			}
		}*/
	}
}
