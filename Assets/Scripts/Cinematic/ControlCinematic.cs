using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ControlCinematic : MonoBehaviour {
	UILabel speakerLabel;
	UILabel textLabel;
	UITexture background;
	GameObject continueMessage;
	public CinematicDialog[] dialogs;
	public enum ActionAfterFinish {UnloadScene, GoToScene};
	public ActionAfterFinish action = ActionAfterFinish.GoToScene;
	public string nextScene = "";

	int index = 0;
	bool nextAllowed = false;

	// Use this for initialization
	void Awake () {
		speakerLabel = transform.FindChild ("SpeakerSprite/Speaker").GetComponent<UILabel> ();
		textLabel = transform.FindChild ("TextSprite/Text").GetComponent<UILabel> ();
		background = transform.FindChild ("Texture").GetComponent<UITexture> ();
		continueMessage = transform.FindChild ("Next").gameObject;
		continueMessage.SetActive (false);
		//textLabel.GetComponent<TypewriterEffect> ().ResetToBeginning ();
		showDialog();
	}

	void showDialog(){
		//textLabel.GetComponent<TypewriterEffect> ().Finish();
		continueMessage.SetActive (false);
		speakerLabel.text = dialogs [index].speakerName;
		textLabel.text = dialogs [index].text;
		if (dialogs [index].imageBackground != background.mainTexture) {
			background.mainTexture = dialogs [index].imageBackground;
		}
		if(index > 0) textLabel.GetComponent<TypewriterEffect> ().ResetToBeginning();
		//textLabel.GetComponent<TypewriterEffect> ().;
		index++;
		nextAllowed = false;
	}

	public void textFinished(){
		nextAllowed = true;
		continueMessage.SetActive (true);
	}

	void finishCinematic(){
		print ("end cinematic");
		switch (action) {
		case ActionAfterFinish.GoToScene:
			SceneManager.LoadScene (nextScene);
			break;
		case ActionAfterFinish.UnloadScene:
			SceneManager.UnloadScene (SceneManager.GetActiveScene ().name);
			break;
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.E)) {
			if (nextAllowed) {
				if (index >= dialogs.Length) {
					finishCinematic ();
				} else {
					showDialog ();
				}
			} else {
				textLabel.GetComponent<TypewriterEffect> ().Finish ();
			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			finishCinematic ();
		}
	}
}

[System.Serializable]
public class CinematicDialog{
	public string speakerName;
	public string text;
	public Texture imageBackground;
}