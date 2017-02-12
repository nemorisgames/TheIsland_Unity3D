using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance = null;
	public AudioClip[] backgrounds;
	public AudioClip[] jumpScares;
	public AudioClip[] effects;
	public AudioSource bgSource;
	public AudioSource jsSource;
	public AudioSource efSource;
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
		}
		else
		{
			AudioSource[] sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			for (int i = 0; i < 3 - sources.Length; i++)
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
		}
		/*for(int i = 0; i < backgrounds.Length; i++)
		{
			
		}*/
	}
	void PlayJumpScare(int i, float volume = 0)
	{
		if (volume != 0)
			jsSource.PlayOneShot(jumpScares[i], volume);
		else
			jsSource.PlayOneShot(jumpScares[i]);
	}
	void PlayAmbientMusic(int i)
	{
		FadeAmbient(1f, i);
	}
	IEnumerator FadeAmbient(float time, int pos)
	{
		int steps = 10;
		float timeStep = time / steps * 1.0f;
		float volumeStep = bgSource.volume / (timeStep);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			bgSource.volume -= volumeStep;
		}
		bgSource.clip = backgrounds[pos];
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			bgSource.volume += volumeStep;
		}
	}
}
