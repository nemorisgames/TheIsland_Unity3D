using UnityEngine;
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
		print(gameObject.GetComponentInChildren<Camera>().enabled);
		gameObject.GetComponentInChildren<Camera>().enabled = true;
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
		//killCamera.transform.parent.parent = fpsCamera.transform.parent;
		killCamera.transform.parent.position = fpsCamera.transform.position;
		killCamera.transform.parent.rotation = fpsCamera.transform.rotation;
		fpsCamera.SetActive(false);
		fpsCamera.GetComponent<Camera>().enabled = false;
		killCamera.GetComponent<Camera>().enabled = true;
		killCamera.GetComponent<AudioListener> ().enabled = true;
		killAnimation.Interact();
		killCamera.GetComponent<Animator>().SetBool("Kill", true);
	}

	public void enhaceSight(int enhace)
	{
		fpsCamera.GetComponent<Camera>().farClipPlane = (enhace == 1) ? 37f : 15f;
		RenderSettings.fog = (enhace == 0);
	}

	void Reset()
	{
		Debug.Log("Resseting to last start");
		//SceneManager.LoadScene(0);
	}
}
