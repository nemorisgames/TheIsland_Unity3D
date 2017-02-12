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
    public AudioClip[] music;

    // Use this for initialization
    void Start () {
        caraTransform = GameObject.FindWithTag("Player").transform;
        caraAudioSource = gameObject.GetComponent<AudioSource>();
        musicAudioSource = transform.FindChild("MusicManager").GetComponent<AudioSource>();
		playSoundScape();
		setCaraMood (CaraState.Idle);
    }

    public void playMusic(int index)
    {
		if (musicAudioSource.clip != music[index])
		{
			StartCoroutine(changeMusic(music[index]));
		} 
    }

	public void playSoundScape(){
		if(musicAudioSource.clip != soundscape)
		{
			StartCoroutine(changeMusic(soundscape));
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

	public void setCaraMood(CaraState c){
		caraState = c;
		playAudio();
	}

    public void playAudio()
    {
        switch (caraState)
        {
        case CaraState.Idle:
            caraAudioSource.clip = respirationNormal;
			break;
		case CaraState.Running:
			caraAudioSource.clip = respirationRunning;
			break;
		case CaraState.Scared:
			caraAudioSource.clip = respirationScared;
			break;
		case CaraState.Walking:
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
