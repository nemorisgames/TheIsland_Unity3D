using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ViewPages : MonoBehaviour
{
	public GameObject pagePrefab;
	public GameObject grid;
	private int currentPage = 0;
	private int pagesCount = 1;
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
	void ShowPage(int pos = -1)
	{
		if (pos != -1)
		{
			currentPage = pos;
		}
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
	}

}
