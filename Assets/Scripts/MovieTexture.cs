//#define MOBILE_PLATFORM
 
using UnityEngine;
using System.Collections;
 
#if UNITY_PS4
public class MovieTexture : Texture
{
    // Constructors
	public MovieTexture (){}
   
    // Methods
	public void Play (){}
	public void Stop (){}
	public void Pause (){}
   
    // Properties
	public AudioClip audioClip { get{ return null; } }
	public bool loop { get{ return false; } set{ } }
	public bool isPlaying { get{ return false; } }
	public bool isReadyToPlay { get{ return false; } }
}
#endif