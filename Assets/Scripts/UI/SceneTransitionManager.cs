using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SceneTransitionManager : MonoBehaviour
{
	public static SceneTransitionManager Instance = null;
	[SerializeField]
	Image bg;
	public bool debug = false;
	public int sceneindex = 1;
	private void Awake()
	{
		if (Instance != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		Instance = this;
		bg.gameObject.SetActive(false);
		DontDestroyOnLoad(gameObject);
	}
	private void Update()
	{
		if (debug)
		{
			debug = false;
			TransitionToScene(sceneindex);
		}
	}
	public void TransitionToScene(int sceneIndex)
	{
		bg.color = Color.black;
		bg.gameObject.SetActive(true);
		StartCoroutine(waitForLoadedScene(sceneIndex));
	}
	IEnumerator waitForLoadedScene(int s)
	{
		float r = 0;
		AsyncOperation op = SceneManager.LoadSceneAsync(s, LoadSceneMode.Single);
		while (!op.isDone)
		{
			r += Time.deltaTime * 5;
			if (r > 255)
				r = 255;
			bg.color = new Color(r, bg.color.g, bg.color.b);
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(0.5f);
		while (bg.color.a > 0)
		{
			bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, bg.color.a - Time.deltaTime * 0.2f);
		}
		bg.gameObject.SetActive(false);
	}
}
