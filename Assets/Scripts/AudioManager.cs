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
	public AudioSource jsSource;
	public AudioSource efSource;
	public AudioSource sitSource;
	private List<AudioSource> playingloop;
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
		}
		else
		{
			AudioSource[] sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			for (int i = 0; i < 4 - sources.Length; i++)
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
		}
		PlayAmbientMusic(2, 0f);
		StartCoroutine(Demo());
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
		StartCoroutine(FadeAmbient(time, i));
	}
	public void PlaySituationMusic(int i, float time = 1f)
	{
		StartCoroutine(FadeSituacion(time, i));
	}
	public void PlayOnlySituation(int i, float time = 1f)
	{
		StartCoroutine(FadeSituacion(time, i, true));
	}
	public void PlayOnlyAmbient(int i, float time = 1f)
	{
		StartCoroutine(FadeAmbient(time, i, true));
	}

	IEnumerator FadeAll(float time)
	{
		int steps = 10;
		float timeStep = time / steps * 1.0f;
		float volumeStep = bgSource.volume / (timeStep);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			sitSource.volume -= volumeStep;
			bgSource.volume -= volumeStep;
		}
	}
	IEnumerator FadeSituacion(float time, int pos, bool forceAll = false)
	{
		int steps = 10;
		float timeStep = time / steps * 1.0f;
		float volumeStep = sitSource.volume / (timeStep);
		if (forceAll)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				sitSource.volume -= volumeStep;
			}
		}
		else
			yield return (FadeAll(time));
		sitSource.Stop();
		sitSource.clip = situation[pos];
		sitSource.Play();
		sitSource.loop = true;
		sitSource.clip = situation[pos];
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			sitSource.volume += volumeStep;
		}
	}
	IEnumerator FadeAmbient(float time, int pos, bool forceAll = false)
	{
		int steps = 10;
		float timeStep = time / steps * 1.0f;
		float volumeStep = bgSource.volume / (timeStep);
		if (forceAll)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				bgSource.volume -= volumeStep;
			}
		}
		else
			yield return (FadeAll(time));
		//Debug.Log("stopping audio");
		bgSource.Stop();
		bgSource.clip = backgrounds[pos];
		bgSource.Play();
		bgSource.loop = true;
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			bgSource.volume += volumeStep;
		}
		//Debug.Log("audio back to full volume");
	}
	IEnumerator Demo()
	{
		yield return new WaitForSeconds(2f);
		//Debug.Log("Switching to new music");
		PlayAmbientMusic(1);
		PlaySituationMusic(2);
	}
}
