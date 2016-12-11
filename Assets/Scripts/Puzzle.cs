using UnityEngine;
using System.Collections;

public class Puzzle : MonoBehaviour {
	public PuzzlePart[] parts;
	public enum TypePuzzle {Lock};
	public TypePuzzle typePuzzle;
	public GameObject callbackObject;
	public string callbackFunction;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		bool done = true;
		for (int i = 0; i < parts.Length; i++) {
			if (Vector3.Distance (parts [i].finalPosition, parts [i].part.localPosition) <= parts [i].distanceError) {
				if (Quaternion.Angle (Quaternion.Euler (parts [i].finalRotation), parts [i].part.localRotation) <= parts [i].rotationError) {
					parts [i].part.localPosition = parts [i].finalPosition;
					//parts [i].part.localRotation = Quaternion.Euler (parts [i].finalRotation);
					//print ("done " + i);
				} else
					done = false;
			} else
				done = false;

			if (!Input.GetMouseButton (0)) {
				switch (typePuzzle) {
				case TypePuzzle.Lock:
					int currentIndex = 0;
					float ang = 1000f;
					for (int j = 0; j < 10; j++) {
						if (Quaternion.Angle (Quaternion.Euler (new Vector3 (parts [i].finalRotation.x, parts [i].finalRotation.y, j * 360f / 10f)), parts [i].part.localRotation) < ang) {
							ang = Quaternion.Angle (Quaternion.Euler (new Vector3 (parts [i].finalRotation.x, parts [i].finalRotation.y, j * 360f / 10f)), parts [i].part.localRotation);
							currentIndex = j;
						}
					}
					//if (ang > 1f)
						parts [i].part.localRotation = Quaternion.RotateTowards (parts [i].part.localRotation, Quaternion.Euler (new Vector3 (parts [i].finalRotation.x, parts [i].finalRotation.y, currentIndex * 360f / 10f - 180f)), ang * 3f * Time.deltaTime);
					/*else {
						parts [i].part.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
						parts [i].part.GetComponent<Rigidbody> ().velocity = Vector3.zero;
					}*/
					//if (ang > 1f)
					//	parts [i].part.Rotate (new Vector3 (0f, 0f, ang));
					break;
				}
			}
		}
		if (done) {
			PuzzleDone ();
		}
	}

	public void PuzzleDone(){
		if (callbackObject != null) {
			callbackObject.SendMessage (callbackFunction);
		}
		Destroy (gameObject);
	}
}

[System.Serializable]
public class PuzzlePart{
	public Transform part;
	public Vector3 finalPosition;
	public float distanceError = 0.1f;
	public Vector3 finalRotation;
	public float rotationError = 2f;
}