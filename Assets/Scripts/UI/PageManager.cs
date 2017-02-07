using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
	public static PageManager Instance = null;
	public GameObject pagePrefab;
	public List<int> unlockedPages;
	[SerializeField]
	GameObject[] allPages;
	private void Awake()
	{
		Instance = this;
		unlockedPages = new List<int>();
		Load();
	}
	void Load()
	{

	}
	public void AddPage(int page)
	{
		if (!unlockedPages.Contains(page))
		{
			unlockedPages.Add(page);
		}
	}
}
