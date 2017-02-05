using UnityEngine;
using System.Collections;

public class ItemFollowHand : MonoBehaviour {
	public Transform hand;
	// Use this for initialization
	void Start () {
		hand = transform.FindChild ("../Hand/female_hand_left/female_hand_left/upperarm_l/lowerarm1_l/lowerarm2_l/lowerarm3_l/hand_l");
		transform.parent = hand;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
