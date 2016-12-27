using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance = null;
	public AudioClip[] audios;
	private AudioSource[] sources;
	private List<AudioSource> playingloop;
	void Awake()
	{
		Instance = this;
		playingloop = new List<AudioSource>();
		sources = new AudioSource[audios.Length];
		for (int i = 0; i < audios.Length; i++)
		{
			var audio = gameObject.AddComponent<AudioSource>();
			audio.clip = audios[i];
			audio.playOnAwake = false;
			audio.loop = false;
			sources[i] = audio;
		}
	}
	void PlayJumpScare(int i)
	{
		sources[i].PlayOneShot(audios[i]);
	}
	void PlayAmbientMusic(int i)
	{
		sources[i].loop = true;
		sources[i].clip = audios[i];
		sources[i].Play();
		if (!playingloop.Contains(sources[i]))
			playingloop.Add(sources[i]);
	}
	void ChangeAmbientMusic(int clip)
	{
		for (int i = 0; i < playingloop.Count; i++)
		{
			StartCoroutine(FadeOut(i, 1f));
		}
		playingloop.Clear();
		sources[clip].clip = audios[clip];
		sources[clip].Play();
		playingloop.Add(sources[clip]);
	}
	IEnumerator FadeOut(int i, float time)
	{
		float startVolume = sources[i].volume;
		while (sources[i].volume > 0)
		{
			sources[i].volume = startVolume * Time.deltaTime / time;
			yield return null;
		}
		sources[i].Stop();
		sources[i].volume = startVolume;
	}
}
