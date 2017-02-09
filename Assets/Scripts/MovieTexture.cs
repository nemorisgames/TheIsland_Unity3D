//#define MOBILE_PLATFORM
 
using UnityEngine;
using System.Collections;
 
#if PS4
public class MovieTexture : Texture
{
    // Constructors
    public MovieTexture ();
   
    // Methods
    public void Play ();
    public void Stop ();
    public void Pause ();
   
    // Properties
    public AudioClip audioClip { get; }
    public bool loop { get; set; }
    public bool isPlaying { get; }
    public bool isReadyToPlay { get; }
}
#endif