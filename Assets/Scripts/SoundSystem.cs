using UnityEngine;
using System.Collections;

public class SoundSystem : MonoBehaviour {
    AudioSource caraAudioSource;
    AudioSource musicAudioSource;
    public enum CaraState {Idle, Walking, Running, Scared};
    public CaraState caraState = CaraState.Idle;
    Transform caraTransform;
    [Header("Cara's sounds")]
    public AudioClip respirationNormal;
    public AudioClip respirationRunning;
    public AudioClip respirationScared;

    [Header("Music")]
    public AudioClip soundscape;
    public AudioClip danger;
    public AudioClip[] coven;

    // Use this for initialization
    void Start () {
        caraTransform = GameObject.FindWithTag("Player").transform;
        caraAudioSource = gameObject.GetComponent<AudioSource>();
        musicAudioSource = transform.FindChild("MusicManager").GetComponent<AudioSource>();
        playAudio();
        playMusic(0);
    }

    public void playMusic(int index)
    {
        switch (index)
        {
            case 0:
                if(musicAudioSource.clip != soundscape)
                {
                    StartCoroutine(changeMusic(soundscape));
                }
                break;
            case 1:
                if (musicAudioSource.clip != coven[0] || musicAudioSource.clip != coven[1] || musicAudioSource.clip != coven[2])
                {
                    StartCoroutine(changeMusic(coven[1]));
                }
                break;
            case 2:
                if (musicAudioSource.clip != danger)
                {
                    StartCoroutine(changeMusic(danger));
                }
                break;
        }
    }

    IEnumerator changeMusic(AudioClip a)
    {
        while (musicAudioSource.volume > 0.1f)
        {
            yield return new WaitForEndOfFrame();
            musicAudioSource.volume -= Time.deltaTime;
        }
        musicAudioSource.clip = a;
        musicAudioSource.Play();
        while (musicAudioSource.volume < 1f)
        {
            yield return new WaitForEndOfFrame();
            musicAudioSource.volume += Time.deltaTime;
        }
        musicAudioSource.volume = 1f;
    }

    public void playAudio()
    {
        switch (caraState)
        {
            case CaraState.Idle:
                caraAudioSource.clip = respirationNormal;
                break;
        }
        caraAudioSource.Play();
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = caraTransform.position;
    }
}
