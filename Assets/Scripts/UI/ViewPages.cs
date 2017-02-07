using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ViewPages : MonoBehaviour
{
	public RawImage image;	
	public GameObject noPages;
	private int currentPage = 0;
	private int pagesCount = 1;
	private List<Texture2D> pages;
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
		pages = new List<Texture2D>();
		//aqui cargar paginas
		for (int i = 0; i < 15; i++)
		{
			pages.Add(new Texture2D(500, 500));
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
		image.texture = pages[currentPage];
		//image.color = pages[currentPage].;
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
