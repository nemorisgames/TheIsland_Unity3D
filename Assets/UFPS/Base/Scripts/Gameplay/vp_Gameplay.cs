/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Gameplay.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	a place for globally accessible info on the game session, such
//					as whether we're in singleplayer or multiplayer mode. this can
//					be inherited to provide more info on the game: for example: custom
//					game modes
//
//					TIP: for global quick-access to the local player, see 'vp_Local'
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class vp_Gameplay
{

	public static bool IsMultiplayer = false;
	protected static bool m_IsMaster = true;

	/// <summary>
	/// this property can be set by multiplayer scripts to assign master status
	/// to the local player. in singleplayer this is forced to true
	/// </summary>
	public static bool IsMaster
	{
		get
		{
			if (!IsMultiplayer)
				return true;
			return m_IsMaster;
		}
		set
		{
			if (!IsMultiplayer)
				return;
			m_IsMaster = value;
		}
	}


	/// <summary>
	/// pauses or unpauses the game by means of setting timescale to zero. will
	/// backup the current timescale for when the game is unpaused.
	/// NOTE: will not work in multiplayer
	/// </summary>
	public static bool IsPaused
	{
		get { return vp_TimeUtility.Paused; }
		set { vp_TimeUtility.Paused = (vp_Gameplay.IsMultiplayer ? false : value); }
	}


	// --- the below properties renamed for consistency ---


	[System.Obsolete("Please use the 'IsMaster' property instead.")]
	public static bool isMaster
	{
		get { return IsMaster; }
		set { IsMaster = value; }
	}


	[System.Obsolete("Please use the 'IsMultiplayer' property instead.")]
	public static bool isMultiplayer
	{
		get { return IsMultiplayer; }
		set { IsMultiplayer = value; }
	}


	/// <summary>
	/// returns the build index of the currently loaded level (Unity version agnostic)
	/// </summary>
	public static int CurrentLevel
	{
		get
		{
#if UNITY_5_4_OR_NEWER
			return SceneManager.GetActiveScene().buildIndex;
#else
			return Application.loadedLevel;
#endif
		}
	}


	/// <summary>
	/// returns the name of the currently loaded level (Unity version agnostic)
	/// </summary>
	public static string CurrentLevelName
	{
		get
		{
#if UNITY_5_4_OR_NEWER
			return SceneManager.GetActiveScene().name;
#else
			return Application.loadedLevelName;
#endif
		}
	}


}