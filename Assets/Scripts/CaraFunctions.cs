﻿using UnityEngine;
using System.Collections;

public class CaraFunctions : MonoBehaviour {
    public static CaraFunctions Instance = null;
    public GameObject bothHands;
    public GameObject rightHand;
	// Use this for initialization
	void Start () {
	
	}
	void Awake()
    {
        Instance = this;
    }
	// Update is called once per frame
	void Update () {
	
	}
}
