using UnityEngine;
using System.Collections;

public class FollowDelay : MonoBehaviour {
	Transform target;
	public float speed = 1f;
	public float speedRotation = 1f;
	public Vector3 position;
	public Vector3 positionSelected;
	// Use this for initialization
	void Awake () {
		target = GameObject.FindWithTag ("Player").transform.FindChild ("FPSCamera");
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * position.x + target.forward * position.z + target.up * position.y;
		transform.position = Vector3.Lerp(transform.position, tVec, Time.deltaTime * speed);
		transform.forward = Vector3.Lerp(transform.forward, target.forward, Time.deltaTime * speedRotation);
	}
}
