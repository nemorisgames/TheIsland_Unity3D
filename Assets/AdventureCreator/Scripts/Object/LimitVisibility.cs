/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2016
 *	
 *	"LimitVisibility.cs"
 * 
 *	Attach this script to a GameObject to limit its visibility
 *	to a specific GameCamera in your scene.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This component limits the visibility of a GameObject so that it can only be viewed through a specific _Camera.
	 */
	[AddComponentMenu("Adventure Creator/Camera/Limit visibility to camera")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_limit_visibility.html")]
	#endif
	public class LimitVisibility : MonoBehaviour
	{

		/** The _Camera to limit the GameObject's visibility to */
		public _Camera limitToCamera;
		/** If True, then child GameObjects will be affected in the same way */
		public bool affectChildren = false;
		/** If True, then the object will not be visible even if the correct _Camera is active */
		[HideInInspector] public bool isLockedOff = false;

		private _Camera activeCamera;
		private bool isVisible = false;


		private void Start ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;

			if (!isLockedOff)
			{
				if (activeCamera == limitToCamera)
				{
					SetVisibility (true);
				}
				else if (activeCamera != limitToCamera)
				{
					SetVisibility (false);
				}
			}
			else
			{
				SetVisibility (false);
			}
		}


		/**
		 * Updates the visibility based on the attached camera. This is public so that it can be called by StateHandler.
		 */
		public void _Update ()
		{
			activeCamera = KickStarter.mainCamera.attachedCamera;

			if (!isLockedOff)
			{
				if (activeCamera == limitToCamera && !isVisible)
				{
					SetVisibility (true);
				}
				else if (activeCamera != limitToCamera && isVisible)
				{
					SetVisibility (false);
				}
			}
			else if (isVisible)
			{
				SetVisibility (false);
			}
		}


		private void SetVisibility (bool state)
		{
			if (GetComponent <Renderer>())
			{
				GetComponent <Renderer>().enabled = state;
			}
			else if (gameObject.GetComponent <SpriteRenderer>())
			{
				gameObject.GetComponent <SpriteRenderer>().enabled = state;
			}
			if (gameObject.GetComponent <GUITexture>())
			{
				gameObject.GetComponent <GUITexture>().enabled = state;
			}

			if (affectChildren)
			{
				Renderer[] _children = GetComponentsInChildren <Renderer>();
				foreach (Renderer child in _children)
				{
					child.enabled = state;
				}

				SpriteRenderer[] spriteChildren = GetComponentsInChildren <SpriteRenderer>();
				foreach (SpriteRenderer child in spriteChildren)
				{
					child.enabled = state;
				}

				GUITexture[] textureChildren = GetComponentsInChildren <GUITexture>();
				foreach (GUITexture child in textureChildren)
				{
					child.enabled = state;
				}
			}

			isVisible = state;
		}

	}

}