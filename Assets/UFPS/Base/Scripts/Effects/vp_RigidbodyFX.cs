/////////////////////////////////////////////////////////////////////////////////
//
//	vp_RigidbodyFX.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	this script can be placed on a RIGIDBODY object to make it spawn
//					SurfaceEffects and object sounds upon collision with external surfaces.
//
//					the general recommendation is to have target surfaces emit GENERIC FX,
//					and for rigidbodies to emit their own SPECIFIC SOUNDS.
//					in other words: all rigidbodies should have the same (or a limited range of)
//					ImpactEvent(s) resulting in GENERIC impact particles + sounds for every target
//					surface (fx that ANY object would make when hitting the surface), and use the
//					CollisionSounds list to trigger impact sounds that are SPECIFIC to a certain
//					type of physics object, allowing for a massive range of combinatory effects.
//
//					USAGE:
//						1) make sure the gameobject has a Rigidbody component.
//						2) assign an ImpactEvent. this is the type of impact that the object
//							will impose on other objects. it will determine what type of
//							SURFACE SOUNDS AND PARTICLE FX are emanated from the surfaces
//							we hit. the default ImpactEvent used on all rigidbodies in the UFPS
//							demo scenes is 'ObjectCollision'. in turn, this ImpactEvent is
//							represented in all SurfaceType objects with suitable effects assigned.
//							for example: an 'ObjectCollision' hitting rock will make the terrain
//							eject dust and pebble particles, along with a gravely sound, while an
//							'ObjectCollision' hitting metal will make the metal clang, but there
//							are very subtle particle effects (if any). if ImpactEvent is left blank,
//							the SurfaceManager will try and come up with fallback effects that may
//							or may not make sense
//						3) define what OBJECT SOUNDS this object should make upon collision
//							with a defined range of world surfaces. these are the sounds
//							that will emanate from the object itself upon collision.
//							for example: a wooden crate might make a hard, rattly sound when
//							hitting rock, but a softer, muffled sound when bouncing on grass
//
//					NOTE: this component does NOT determine what decals and sounds to attach to the
//						OBJECT'S SURFACE when hit by something else (like bullets, or footsteps).
//						for that functionality, attach a SurfaceIdentifier to the object and assign
//						the SurfaceType. for a wooden crate: 'Wood'
//
/////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_RigidbodyFX : MonoBehaviour
{

	public vp_ImpactEvent ImpactEvent;	// for sending an impact to the other collider

#if UNITY_EDITOR
	[vp_HelpBox("'ImpactEvent' determines what type of impact the object will impose upon other rigidbody objects on collision.", UnityEditor.MessageType.None, null, null, false, vp_PropertyDrawerUtility.Space.Nothing)]
	public float impactEventHelp;
#endif

	[Range(1, 100)]
	public float ImpactThreshold = 1.0f;		// collisions below this force will be ignored

	[Range(1, 100)]
	public float MinCameraDistance = 20.0f;		// collisions beyond this distance from the main camera will be ignored
	
	[SerializeField]
	public List<SurfaceFXInfo> CollisionSounds = new List<SurfaceFXInfo>();		// list of surfaces and corresponding sounds that will be emanated from this object on collision
	
	// internal state
	protected static float nextAllowedCollisionFXTime = 0.0f;
	protected bool m_HaveRigidbody = false;
	protected bool m_HaveObjectSounds = false;
	protected bool m_HaveSurfaceManager = false;

	// rigidbody of the gameobject. this is a required component
	protected Rigidbody m_RigidBody = null;
	public Rigidbody Rigidbody
	{
		get
		{
			if (m_RigidBody == null)
			{
				m_RigidBody = GetComponent<Rigidbody>();
				if (m_RigidBody != null)
					m_HaveRigidbody = true;
			}
			return m_RigidBody;
		}
	}

	// constants
	protected const float STARTUP_MUTE_DELAY = 2.0f;
	protected const float SURFACE_DETECTION_RAYCAST_DISTANCE = 0.4f;
	protected const float MIN_GLOBAL_FX_INTERVAL = 0.2f;


	// main audio source. this will be the one found on the transform.
	// if none is found, one will be auto-added. NOTE: if the audio
	// source is used by other scripts for playing sounds, any sounds
	// resulting from the ImpactEvent upon the target surface will be
	// muted
	protected AudioSource m_Audio = null;
	protected AudioSource Audio
	{
		get
		{
			if (m_Audio == null)
				m_Audio = GetComponent<AudioSource>();
			if (m_Audio == null)
				m_Audio = gameObject.AddComponent<AudioSource>();
			return m_Audio;
		}
	}

	// auto-created second audio source. this is needed so we can play
	// sounds from the surface and from the object simultaneously
	protected AudioSource m_Audio2 = null;
	protected AudioSource Audio2
	{
		get
		{
			if (m_Audio2 == null)
			{
				m_Audio2 = gameObject.AddComponent<AudioSource>();
				// copy some parameters from the main audio source
				// TIP: add more parameters to copy here as needed
				m_Audio2.rolloffMode = Audio.rolloffMode;
				m_Audio2.minDistance = Audio.minDistance;
				m_Audio2.maxDistance = Audio.maxDistance;
				m_Audio2.spatialBlend = 1.0f;
			}
			return m_Audio2;
		}
	}


	// this struct is used to declare certain SurfaceEffects to spawn
	// upon collision with certain SurfaceTypes
	[System.Serializable]
	public struct SurfaceFXInfo
	{
		public SurfaceFXInfo(bool init)
		{
			SurfaceType = null;
			Sound = null;
		}
		public vp_SurfaceType SurfaceType;
		public AudioClip Sound;
	}
	

	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{

		if (Rigidbody == null)
			Debug.LogWarning("Warning (" + this + ") This component requires a RigidBody. Physics impact FX won't work!");

		if ((CollisionSounds != null) && (CollisionSounds.Count > 0))
		{

			for(int v = CollisionSounds.Count - 1; v > -1; v--)
			{
				if ((CollisionSounds[v].SurfaceType == null) || (CollisionSounds[v].Sound == null))
					CollisionSounds.RemoveAt(v);
			}

			if (CollisionSounds.Count > 0)
				m_HaveObjectSounds = true;

		}

		m_HaveSurfaceManager = (vp_SurfaceManager.Instance != null);

	}

	
	/// <summary>
	/// extracts surface information from the object we collided with, plays
	/// surface FX on it depending on the ImpactEvent of the surface identifier
	/// on the same transform as this component, and plays an impact sound
	/// emanated from this object depending on the detected SurfaceType
	/// </summary>
	protected virtual void OnCollisionEnter(Collision collision)
	{

		// this prevents a ton of effects going off on scene startup due to slightly
		// hovering rigidbodies
		if (Time.realtimeSinceStartup < STARTUP_MUTE_DELAY)
			return;

		// this effectively prevents the system from spamming too many sounds at once,
		// for example: it prevents playing two metal impact sounds at the exact same
		// time when two metal crates collide. instead, only one of the sounds will play.

		if (Time.time < nextAllowedCollisionFXTime)
			return;
		
		if (!m_HaveRigidbody)
			return;

		if (Rigidbody.IsSleeping())
			return;

		if (collision.relativeVelocity.sqrMagnitude < ImpactThreshold)
			return;

		if ((Camera.main != null) && Vector3.Distance(Camera.main.transform.position, transform.position) > MinCameraDistance)
			return;

		RaycastHit hit = new RaycastHit();

		// raycast to get a raycasthit, which is required for the SurfaceManager
		// NOTE: this is a bit crude and not suited for small (or long and thin) objects
		Vector3 dir = (collision.contacts[0].point - transform.position).normalized;	// direction from center of object to the collision point
		if (!Physics.Raycast(
			new Ray(collision.contacts[0].point - (dir * (SURFACE_DETECTION_RAYCAST_DISTANCE * 0.5f)), dir),	// raycast to the collision point from a 2dm (default) distance
			out hit,
			SURFACE_DETECTION_RAYCAST_DISTANCE,	// raycast for a 4 dm (default) distance
			vp_Layer.Mask.ExternalBlockers))	// ignore player and all non-solids
			return;
		//Debug.Log("raycast success");	// DEBUG: if this line fails to print: the raycast failed, possibly because the object hit itself or missed because it was too small / thin
		
		// if we get here, we can play a collision effect!

		// prevent other effects playing for a short while to avoid spammy sound crescendos
		// and weird doppler effects when two identical sounds play at the same time.
		// NOTE: this is global (not another rigidbody in the world will be able to play a
		// sound until time's up) but since this system is only active within a certain range
		// of the main camera, this is hardly detectable and an acceptable tradeoff
		nextAllowedCollisionFXTime = Time.time + MIN_GLOBAL_FX_INTERVAL;

		// --- spawn fx (including sounds) emanated from / on the SURFACE WE HIT in this collision ---

		bool mainAudioSourceTaken = Audio.isPlaying;
		vp_SurfaceManager.SpawnEffect(hit, ImpactEvent, (!mainAudioSourceTaken ? Audio : null));

		// fallback in case of no object sounds, and if the surface effect above didn't play a sound
		if (!m_HaveObjectSounds)
		{
			if (m_HaveSurfaceManager && mainAudioSourceTaken)
			{
				if (ImpactEvent == null)
					vp_SurfaceManager.SpawnEffect(hit, vp_SurfaceManager.Instance.Fallbacks.ImpactEvent, Audio2);
				else
					vp_SurfaceManager.SpawnEffect(hit, ImpactEvent, Audio2);
			}
			return;
		}

		// --- play sound (no particles) emanated from THIS OBJECT due to the collision ---

		TryPlaySound(vp_SurfaceManager.GetSurfaceType(hit));

	}


	/// <summary>
	/// tries to play the sound related to this object and 'surfaceType'.
	/// if none found, reverts to fallbacks
	/// </summary>
	void TryPlaySound(vp_SurfaceType surfaceType)
	{

		if (surfaceType == null)
			return;

		if (Audio2 == null)
			return;

		// don't allow audiosources in 'Logarithmic Rolloff' mode to be audible
		// beyond their max distance (Unity bug?)
		if (Vector3.Distance(transform.position, UnityEngine.Camera.main.transform.position) > Audio2.maxDistance)
			return;

		if (!vp_Utility.IsActive(gameObject))
			return;

		bool playedSound = false;
		int fallback = -1;

		for (int v = CollisionSounds.Count - 1; v > -1; v--)
		{
			if (surfaceType == CollisionSounds[v].SurfaceType)
			{
				DoPlaySound(CollisionSounds[v].Sound);
				playedSound = true;
			}
			else if (m_HaveSurfaceManager && (CollisionSounds[v].SurfaceType == vp_SurfaceManager.Instance.Fallbacks.SurfaceType))
				fallback = v;
		}

		if (!playedSound && (fallback >= 0))
			DoPlaySound(CollisionSounds[fallback].Sound);

	}


	/// <summary>
	/// plays 'sound' on the auto-created, second audio source
	/// </summary>
	void DoPlaySound(AudioClip sound)
	{

		if (sound == null)
			return;

		Audio2.pitch = vp_TimeUtility.AdjustedTimeScale;
		Audio2.clip = sound;
		Audio2.Stop();
		Audio2.Play();

	}
	

}
