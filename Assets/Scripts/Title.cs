using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {
    public TweenAlpha[] tweens;
	// Use this for initialization
	void Start () {
        Time.timeScale = 1f;
	}

    public void playGame()
    {
        StartCoroutine(gameBegin());
    }

    IEnumerator gameBegin() {
        foreach(TweenAlpha t in tweens)
        {
            t.delay = 0f;
            t.PlayReverse();
        }
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("ExteriorPrototype");
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
