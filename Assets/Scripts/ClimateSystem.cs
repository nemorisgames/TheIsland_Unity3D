using UnityEngine;
using System.Collections;

public class ClimateSystem : MonoBehaviour {
    Transform player;
    public Transform rainSound;
	// Use this for initialization
	void Start () {
        player = GameObject.FindWithTag("Player").transform; 
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
        rainSound.transform.position = player.position;
    }
}
