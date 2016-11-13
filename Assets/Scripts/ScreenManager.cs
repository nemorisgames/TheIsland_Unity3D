using UnityEngine;
using System.Collections.Generic;
public enum ScreenType
{
    PhotoView,
    ItemView,
}
public class ScreenManager : MonoBehaviour {
    public static ScreenManager Instance = null;
    [SerializeField]
    GameObject[] screens;
    // Use this for initialization
    private Stack<ScreenType> showedScreens= new Stack<ScreenType>();
	void Start () {
	
	}
    void Awake()
    {
        Instance = this;
        HideScreens();
    }
    void HideScreens()
    {
        for(int i=0; i< screens.Length; i++)
        {
            screens[i].SetActive(false);
        }
    }
	public void ShowScreen(ScreenType type)
    {
        if (screens[(int)type].activeInHierarchy)
        {
            if (!showedScreens.Contains(type))
            {
                showedScreens.Push(type);
            }
            return;
        }
        HideScreens();        
        if (showedScreens.Contains(type))
        {
            while (showedScreens.Pop() != type) 
            {
               
            }
        }
        //any aditional setups
        switch (type)
        {
            case ScreenType.PhotoView:
                screens[(int)type].GetComponent<PhotoReview>().LoadPhotos();
                break;
            case ScreenType.ItemView:
                break;
            default:
                break;
        }
        showedScreens.Push(type);
        screens[(int)type].SetActive(true);

    }
    public void CloseScreen()
    {
        HideScreens();
        if (showedScreens.Count <= 1)
        {
            return;
        }
        ScreenType type = showedScreens.Pop();
        screens[(int)type].SetActive(true);
    }
	// Update is called once per frame
	void Update () {
	
	}
}
