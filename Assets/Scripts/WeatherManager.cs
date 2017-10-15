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
	public bool debug = false;
	public static WeatherManager Instance = null;
	public ParticleSystem particles;
	public ParticleSystem rainSheet;
	public GameObject rainSoundGO;
	[HideInInspector]
	public AudioSource source;
	[HideInInspector]
	public AudioSource source_second;
	public AudioClip[] weather_Sounds;
	public AudioClip[] weather_effects;
	public GameObject[] positions;
	public GameObject thunderSpawns;
	public AudioSource[] thunderSounds;
	int currentWeather = (int)Weather.rain_heavy;

	void Awake()
	{
		Instance = this;
		rainSoundGO = GameObject.FindGameObjectWithTag("RainSound");
		if (source == null)
		{
			if (rainSoundGO.GetComponent<AudioSource>() == null)
			{
				rainSoundGO.AddComponent<AudioSource>();
				rainSoundGO.AddComponent<AudioSource>();
			}
			AudioSource[] sources = rainSoundGO.GetComponents<AudioSource>();
			source = sources[0];
			source.volume = 0;
			source_second = sources[1];
			source_second.volume = 0;
		}
		//thunderSounds = thunderSpawns.GetComponentsInChildren<AudioSource>();
	}
	void RemoveOldWeather()
	{

	}
	IEnumerator SmoothAudioChange(float time, float objectiveVolume, int weatherSound)
	{
		AudioSource one;
		AudioSource two;
		if (source.isPlaying)
		{
			one = source;
			two = source_second;
		}
		else
		{
			two = source;
			one = source_second;
		}
		two.clip = weather_Sounds[weatherSound];
		two.Play();
		two.loop = true;
		two.volume = 0f;
		int steps = 10;
		if (time == 0) time = steps;
		float volumeStep = (((objectiveVolume)) / steps);
		Debug.Log(volumeStep);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(time / steps);
			two.volume += volumeStep;
			one.volume -= volumeStep;
		}
		two.volume = objectiveVolume;
		one.Stop();
		one.volume = objectiveVolume;
	}
	IEnumerator SmoothWeatherChange(float time, float objectiveRate, float objectiveRateSheet, float currentRate, float currentRateSheet)
	{
		var em = particles.emission;
		var rate = new ParticleSystem.MinMaxCurve();
		var emSheet = rainSheet.emission;
		var rateSheet = new ParticleSystem.MinMaxCurve();
		rateSheet.mode = ParticleSystemCurveMode.Constant;
		rate.mode = ParticleSystemCurveMode.Constant;

        /*while (!Mathf.Approximately(rate.constant, objectiveRate))
        {
            rate.constant = Mathf.Lerp(rate.constant, objectiveRate, Time.deltaTime / time);
            em.rateOverTime = rate;
            yield return new WaitForSeconds(Time.deltaTime);
        }*/
        rate = em.rateOverTime;
        rateSheet = emSheet.rateOverTime;
        float steps = Mathf.Abs(((objectiveRate - currentRate)) / time);
		float sheetStep = ((objectiveRateSheet - currentRateSheet) / steps);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(time / steps);
			rate.constant += ((objectiveRate - currentRate)) / steps;
			rateSheet.constant += sheetStep;
			em.rateOverTime = rate;
			emSheet.rateOverTime = rateSheet;
		}
    }
	void StartNewWeather(int newWeather = -1)
	{
		float time = 5f;
		print("weather " + currentWeather);
		if (newWeather != -1)
			currentWeather = newWeather;

		Debug.Log("Changing weather to " + currentWeather);
		if (particles != null)
		{
			StopCoroutine("SmoothWeatherChange");
			//RemoveOldWeather();
			switch (currentWeather)
			{
				case 0:
					StartCoroutine(SmoothWeatherChange(time, 100f, 1f, particles.emission.rateOverTime.constant, rainSheet.emission.rateOverTime.constant));
					StartCoroutine(SmoothAudioChange(time, 0.5f, currentWeather));
					//Debug.Log ("Low Rain");
					break;
				case 1:
					StartCoroutine(SmoothWeatherChange(time, 1000f, 15f, particles.emission.rateOverTime.constant, rainSheet.emission.rateOverTime.constant));
					StartCoroutine(SmoothAudioChange(time, 0.5f, currentWeather));
					//Debug.Log ("Normal Rain");
					break;
				case 2:
					StartCoroutine(SmoothWeatherChange(time, 2000f, 150f, particles.emission.rateOverTime.constant, rainSheet.emission.rateOverTime.constant));
					StartCoroutine(SmoothAudioChange(time, 0.75f, currentWeather));
					//Debug.Log ("Heavy Rain");
					break;
				case 3:
					StartCoroutine(SmoothWeatherChange(time, 200f, 0f, particles.emission.rateOverTime.constant, rainSheet.emission.rateOverTime.constant));
					StartCoroutine(SmoothAudioChange(time, 0.75f, currentWeather));
					ThunderAndFlash();
					//Debug.Log ("Thunder");
					break;
				case 4:
                    //lluvia interior
                    StartCoroutine(SmoothAudioChange(time, 0.75f, 3));
                    break;
				default:
					break;
			}
		}
		//switch (currentWeather)
		//{
		//	case 0:
		//		source.clip = weather_Sounds[0];
		//		source.volume = 0.75f;
		//		source.Play();
		//		//Debug.Log ("Low Rain");
		//		break;
		//	case 1:
		//		source.clip = weather_Sounds[1];
		//		source.volume = 1f;
		//		source.Play();
		//		//Debug.Log ("Normal Rain");
		//		break;
		//	case 2:
		//		source.clip = weather_Sounds[2];
		//		source.volume = 1f;
		//		source.Play();
		//		//Debug.Log ("Heavy Rain");
		//		break;
		//	case 3:
		//		break;
		//	case 4:
		//		//lluvia interior
		//		//print ("interior");
		//		source.clip = weather_Sounds[3];
		//		source.volume = 1f;
		//		source.Play();
		//		break;
		//	default:
		//		break;
		//}
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
		if (!debug)
			return;
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
		}
	}
}
