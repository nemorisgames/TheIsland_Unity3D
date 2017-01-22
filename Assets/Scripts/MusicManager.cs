using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance = null;
	public AudioClip[] audios;
	public AudioSource source;
	private List<AudioSource> playingloop;
	void Awake()
	{
		Instance = this;
		playingloop = new List<AudioSource>();
		if (GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>() == null)
		{
			GameObject.FindGameObjectWithTag("Player").AddComponent<AudioSource>();
		}
		source = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
		for (int i = 0; i < audios.Length; i++)
		{
			var audio = gameObject.AddComponent<AudioSource>();
			audio.clip = audios[i];
			audio.playOnAwake = false;
			audio.loop = false;
			source = audio;
		}
	}
	void PlayJumpScare(int i)
	{
		GetComponent<AudioSource>().playOnAwake = false;
		GetComponent<AudioSource>().loop = false;
		source.PlayOneShot(audios[i]);
	}
	void PlayAmbientMusic(int i)
	{
		source.loop = true;
		source.clip = audios[i];
		source.Play();
		if (!playingloop.Contains(source))
			playingloop.Add(source);
	}
	void ChangeAmbientMusic(int clip)
	{
		for (int i = 0; i < playingloop.Count; i++)
		{
			StartCoroutine(FadeOut(i, 1f));
		}
		playingloop.Clear();
		source.clip = audios[clip];
		source.Play();
		playingloop.Add(source);
	}
	IEnumerator FadeOut(int i, float time)
	{
		float startVolume = source.volume;
		while (source.volume > 0)
		{
			source.volume = startVolume * Time.deltaTime / time;
			yield return null;
		}
		source.Stop();
		source.volume = startVolume;
	}
}
