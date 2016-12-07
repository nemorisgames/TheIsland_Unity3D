﻿using UnityEngine;
using System.Collections;

public class CaraFunctions : MonoBehaviour {
    public static CaraFunctions Instance = null;
    public GameObject leftHand;
    public GameObject rightHand;
	public GameObject defaultRightHand;
	// Use this for initialization
	void Start () {
		leftHand.SendMessage("Hide");
		rightHand.SendMessage("Hide");
		defaultRightHand.SendMessage("Hide");
	}
	void Awake()
    {
        Instance = this;
    }
	// Update is called once per frame
	void Update () {
	    
	}
}
