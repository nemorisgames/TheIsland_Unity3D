using UnityEngine;
using System.Collections;

public class PushRigidbody : MonoBehaviour {
    Rigidbody rigidbody;
    public Vector3 force = new Vector3(0f, 0f, 10f);
    // Use this for initialization
    void Start () {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void push()
    {
        rigidbody.isKinematic = false;
        rigidbody.AddForce(transform.right * force.x + transform.up * force.y + transform.forward * force.z);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
