using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ViewPages : MonoBehaviour
{
	public RawImage image;
	public GameObject pagePrefab;
	public GameObject grid;
	public GameObject noPages;
	private int currentPage = 0;
	private int pagesCount = 1;
	private List<GameObject> pages;
	void Awake()
	{

	}
	void LoadPages()
	{
		//load pages from here and add to grid
	}
	void NextPage()
	{
		currentPage++;
		if (currentPage >= pagesCount)
		{
			currentPage = 0;
		}
		ShowPage();
	}
	void LastPage()
	{
		currentPage--;
		if (currentPage < 0)
			currentPage = pagesCount - 1;
		ShowPage();
	}
	public void Load()
	{
		if (pages != null)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				Destroy(pages[i]);
			}
		}
		pages = new List<GameObject>();
		for (int i = 0; i < 15; i++)
		{
			GameObject ob = GameObject.Instantiate(pagePrefab, grid.transform) as GameObject;
			ob.transform.localScale = Vector3.one;
			ob.transform.SetAsLastSibling();
			ob.GetComponent<RawImage>().color = new Color(255f, Random.Range(0f, 255f), Random.Range(0f, 255f));
			pages.Add(ob);
		}
		pagesCount = 15;
		currentPage = 0;
		noPages.SetActive(false);
	}
	void ShowPage(int pos = -1)
	{
		if (pos != -1)
		{
			currentPage = pos;

		}
		image.texture = pages[currentPage].GetComponent<RawImage>().texture;
		image.color = pages[currentPage].GetComponent<RawImage>().color;
		//show currentPage		
	}
	void Update()
	{
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				NextPage();
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				LastPage();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ScreenManager.Instance.CloseScreen();
		}
	}

}
