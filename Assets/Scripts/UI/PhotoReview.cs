using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class PhotoReview : MonoBehaviour {
    int photoNumber;
   //[SerializeField]
    //RawImage photo;
    [SerializeField]
    UITexture photo;
    [SerializeField]
    GameObject noPhoto;
    private List<Texture2D> photosTaken;
    private List<string> photosTakenPath;
    bool isDirty=false;
    int currentPhoto=0;

    void OnEnable()
    {
        //this screen has been activated, show/load taken images
        //LoadPhotos();
    }
    void OnDisable()
    {
        if (isDirty)
        {
            RenamePhotos();
        }
    }
	// Use this for initialization
	void Start () {
	
	}
    void Awake()
    {
        isDirty = false;
        currentPhoto = 0;
        photoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
    }
	public void LoadPhotos()
    {
        Debug.Log("loading fotos");
        photoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
        photosTaken = new List<Texture2D>();
        photosTakenPath = new List<string>();
        Texture2D texture = null;
        byte[] fileData;
        string path = "";
        for(int i = 0; i < photoNumber; i++)
        {
            path = Application.persistentDataPath + "/" + TakePhoto.photoName + i + ".png";
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                texture = new Texture2D(2,2);
                texture.LoadImage(fileData);
                photosTaken.Add(texture);
                photosTakenPath.Add(path);
            }
            else
            {
                isDirty = true;
            }
        }
        if (photosTaken.Count > 0)
        {
            noPhoto.SetActive(false);
            photo.gameObject.SetActive(true);
            ShowPhoto(0);
        }
        else
        {
            noPhoto.SetActive(true);
            photo.gameObject.SetActive(false);
        }
    }
    void ShowPhoto(int i)
    {
        //photo.texture=photosTaken[i];
        photo.mainTexture = photosTaken[i];
        currentPhoto = i;
    }
    public void DeletePhoto()
    {
        photosTaken.RemoveAt(currentPhoto);
        FileUtil.DeleteFileOrDirectory(photosTakenPath[currentPhoto]);
        photosTakenPath.RemoveAt(currentPhoto);
        isDirty = true;
    }
    void RenamePhotos()
    {
        string path = Application.persistentDataPath + "/" + TakePhoto.photoName;
        for (int i = 0; i < photosTaken.Count; i++)
        {
            FileUtil.MoveFileOrDirectory(photosTakenPath[i], path + i + ".png");
        }
    }
    public void NextPhoto()
    {
        currentPhoto++;
        if (currentPhoto == photosTaken.Count)
        {
            currentPhoto = 0;
        }
        ShowPhoto(currentPhoto);
    }
    public void LastPhoto()
    {
        currentPhoto--;
        if (currentPhoto<0)
        {
            currentPhoto = photosTaken.Count-1;
        }
        ShowPhoto(currentPhoto);
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                NextPhoto();
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                LastPhoto();
            }
        }
    }
}
