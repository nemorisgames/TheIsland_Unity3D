﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CaraFunctions : MonoBehaviour
{
	public static CaraFunctions Instance = null;
	public AC.Cutscene killAnimation;
	public GameObject killCamera;
	public GameObject fpsCamera;
	public GameObject leftHand;
	public GameObject rightHand;
	public GameObject defaultRightHand;

	public bool kill = false;
	// Use this for initialization
	void Start()
	{
		leftHand.SendMessage("Hide");
		rightHand.SendMessage("Hide");
		defaultRightHand.SendMessage("Hide");
	}
	void Awake()
	{
		Instance = this;
	}
	// Update is called once per frame
	void Update()
	{
		if (kill)
		{
			Kill();
			kill = false;
		}
		//if (Input.GetKeyDown(KeyCode.P))
		//{
		//	ScreenManager.Instance.ShowScreen(ScreenType.BookView);
		//}
	}
	void Kill()
	{
		killCamera.transform.parent.position = fpsCamera.transform.position;
		killCamera.transform.parent.rotation = fpsCamera.transform.rotation;
		killCamera.GetComponent<Camera>().enabled = true;
		fpsCamera.GetComponent<Camera>().enabled = false;
		killAnimation.Interact();
		killCamera.GetComponent<Animator>().SetBool("Kill", true);
	}
	void Reset()
	{
		Debug.Log("Resseting to last start");
		//SceneManager.LoadScene(0);
	}
}
