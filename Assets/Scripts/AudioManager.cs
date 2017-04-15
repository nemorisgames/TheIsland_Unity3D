using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance = null;
	public AudioClip[] backgrounds;
	public AudioClip[] jumpScares;
	public AudioClip[] situation;
	public AudioClip[] effects;
	public AudioSource bgSource;
	public AudioSource bgSource_second;
	public AudioSource jsSource;
	public AudioSource jsSource_second;
	public AudioSource efSource;
	public AudioSource efSource_second;
	public AudioSource sitSource;
	public AudioSource sitSource_second;
	private List<AudioSource> playingloop;

	private AudioSource one;
	private AudioSource two;

	[SerializeField]
	private float volume = 1f;
	void Awake()
	{
		Instance = this;
		playingloop = new List<AudioSource>();
		if (GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>() == null)
		{
			bgSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			bgSource.playOnAwake = false;
			jsSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			jsSource.playOnAwake = false;
			efSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			efSource.playOnAwake = false;
			sitSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			sitSource.playOnAwake = false;


			bgSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			bgSource_second.playOnAwake = false;
			jsSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			jsSource_second.playOnAwake = false;
			efSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			efSource_second.playOnAwake = false;
			sitSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			sitSource_second.playOnAwake = false;
		}
		else
		{
			AudioSource[] sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			for (int i = 0; i < 8 - sources.Length; i++)
			{
				GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			}
			sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			bgSource = sources[0];
			bgSource.playOnAwake = false;
			jsSource = sources[1];
			jsSource.playOnAwake = false;
			efSource = sources[2];
			efSource.playOnAwake = false;
			sitSource = sources[3];
			sitSource.playOnAwake = false;

			bgSource_second = sources[4];
			bgSource_second.playOnAwake = false;
			jsSource_second = sources[5];
			jsSource_second.playOnAwake = false;
			efSource_second = sources[6];
			efSource_second.playOnAwake = false;
			sitSource_second = sources[7];
			sitSource_second.playOnAwake = false;
		}
		//StartCoroutine(Demo());
	}
	void PlayJumpScare(int i, float volume = 0)
	{
		if (volume != 0)
			jsSource.PlayOneShot(jumpScares[i], volume);
		else
			jsSource.PlayOneShot(jumpScares[i]);
	}
	public void PlayAmbientMusic(int i, float time = 1f)
	{
		StopCoroutine("FadeAmbient");
		StartCoroutine(FadeAmbient(time, i));
	}
	public void PlaySituationMusic(int i, float time = 1f)
	{
		StopCoroutine("FadeSituation");
		StartCoroutine(FadeSituacion(time, i));
	}
	public void PlayOnlySituation(int i, float time = 1f)
	{
		StopCoroutine("FadeSituation");
		StartCoroutine(FadeSituacion(time, i, false));
	}
	public void PlayOnlyAmbient(int i, float time = 1f)
	{
		StopCoroutine("FadeAmbient");
		StartCoroutine(FadeAmbient(time, i, false));
	}
	IEnumerator FadeAll(float time)
	{
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			sitSource.volume -= volumeStep;
			bgSource_second.volume -= volumeStep;
			sitSource_second.volume -= volumeStep;
			bgSource.volume -= volumeStep;
		}
		sitSource.Stop();
		bgSource.Stop();
		sitSource.volume = volume;
		bgSource.volume = volume;
		sitSource_second.Stop();
		bgSource_second.Stop();
		sitSource_second.volume = volume;
		bgSource_second.volume = volume;
	}
	IEnumerator FadeSituacion(float time, int pos, bool playAnother = true)
	{
		if (sitSource.isPlaying)
		{
			one = sitSource;
			two = sitSource_second;
		}
		else
		{
			two = sitSource;
			one = sitSource_second;
		}
		two.clip = situation[pos];
		two.Play();
		two.loop = true;
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		two.volume = 0;
		if (playAnother)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				one.volume -= volumeStep;
				two.volume += volumeStep;
			}
		}
		else
			yield return (FadeAll(time));
		one.Stop();
		one.volume = volume;
		/*
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			
		}*/
	}
	IEnumerator FadeAmbient(float time, int pos, bool playAnother = true)
	{
		if (bgSource.isPlaying)
		{
			Debug.Log("yes");
			one = bgSource;
			two = bgSource_second;
		}
		else
		{
			Debug.Log("NO");
			two = bgSource;
			one = bgSource_second;
		}
		two.clip = backgrounds[pos];
		two.Play();
		two.loop = true;
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		Debug.Log("volume step: " + volumeStep);
		two.volume = 0;
		if (playAnother)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				one.volume -= volumeStep;
				two.volume += volumeStep;
				//Debug.Log("one volume " + one.volume + " two volume " + two.volume/* + " sources: " + one.clip != null ? one.clip.name : "nope" + " " + two.clip != null ? two.clip.name : "nope"*/);
			}
		}
		else
			yield return (FadeAll(time));
		one.Stop();
		one.volume = volume;
	}
	IEnumerator Demo()
	{
		PlayAmbientMusic(6, 0f);
		//PlayAmbientMusic(0);
		yield return new WaitForSeconds(10f);
		Debug.Log("Switching to new music");
		PlayAmbientMusic(2, 5f);
		//PlaySituationMusic(2);
		//yield return new WaitForSeconds(2f);
		//CellPhone.Instance.SendMessage("ShowNotification");
	}
}
