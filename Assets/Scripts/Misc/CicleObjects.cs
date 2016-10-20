using UnityEngine;
using System.Collections;

public class CicleObjects : MonoBehaviour {
    public GameObject[] g;
    public int indice = 0;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyUp(KeyCode.Q))
        {
            if (g[indice] != null) g[indice].SetActive(false);
            indice = Mathf.Clamp(indice - 1, 0, g.Length - 1);
            if(g[indice] != null) g[indice].SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (g[indice] != null) g[indice].SetActive(false);
            indice = Mathf.Clamp(indice + 1, 0, g.Length - 1);
            if (g[indice] != null) g[indice].SetActive(true);
        }
    }
}
