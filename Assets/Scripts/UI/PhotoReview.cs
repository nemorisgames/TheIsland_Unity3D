using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class PhotoReview : MonoBehaviour {
	public static PhotoReview Instance = null;
    int photoNumber;
	//[SerializeField]
	//RawImage photo;
	[SerializeField]
	GameObject photoGameObject;
	[SerializeField]
    RawImage photo;
    [SerializeField]
    GameObject noPhoto;
	[SerializeField]
	GameObject ConfirmationPopup;
	[SerializeField]
	GameObject allPhotosGrid;
	[SerializeField]
	GameObject photoPrefab;

	private List<Texture2D> photosTaken;
    private List<string> photosTakenPath;
	private List<Photo>scrollPhotos;
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
		Instance = this;
        isDirty = false;
        currentPhoto = 0;		
        photoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
    }
	public void LoadPhotos()
    {
        Debug.Log("loading fotos");
        photoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
        photosTaken = new List<Texture2D>();
		if (scrollPhotos!=null && scrollPhotos.Count>0)
		{
			for (int i = 0; i < scrollPhotos.Count; i++)
			{
				Destroy(scrollPhotos[i].gameObject);
			}
		}		
		scrollPhotos = new List<Photo>();
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
				GameObject p = GameObject.Instantiate(photoPrefab, allPhotosGrid.transform) as GameObject;
				p.transform.SetAsLastSibling();
				p.transform.localScale = new Vector3(1, 1, 1);
				Photo pc = p.GetComponent<Photo>();
				pc.pos = i;
				pc.texture.texture = texture;				
				scrollPhotos.Add(pc);				
            }
            else
            {
                isDirty = true;
            }
        }
        if (photosTaken.Count > 0)
        {
            noPhoto.SetActive(false);
            photoGameObject.SetActive(true);
            ShowPhoto(0);
        }
        else
        {
            noPhoto.SetActive(true);
			photoGameObject.SetActive(false);
        }
    }
    public void ShowPhoto(int pos)
    {
		if (pos < 0)
			pos = 0;
        //photo.texture=photosTaken[i];
        photo.texture = photosTaken[pos];
		for(int i=0; i< scrollPhotos.Count; i++)
		{
			scrollPhotos[i].highlight.SetActive(false);
		}
		scrollPhotos[pos].highlight.SetActive(true);
        currentPhoto = pos;
    }
	public void ShowConfirmationPopup()
	{
		ConfirmationPopup.SetActive(true);
	}
	public void OnClickYes()
	{
		DeletePhoto();
		ConfirmationPopup.SetActive(false);
	}
	public void OnClickNo()
	{
		ConfirmationPopup.SetActive(false);
	}
    public void DeletePhoto()
    {
        photosTaken.RemoveAt(currentPhoto);
		//new
		if (File.Exists(photosTakenPath[currentPhoto]))
		{
			File.Delete(photosTakenPath[currentPhoto]);
		}
        //FileUtil.DeleteFileOrDirectory(photosTakenPath[currentPhoto]);
		Debug.Log("Deleted photo");
        photosTakenPath.RemoveAt(currentPhoto);
		Destroy(scrollPhotos[currentPhoto].gameObject);
		scrollPhotos.RemoveAt(currentPhoto);
		ShowPhoto(currentPhoto - 1);
        isDirty = true;
    }
    void RenamePhotos()
    {
        string path = Application.persistentDataPath + "/" + TakePhoto.photoName;
		string temp = "";
        for (int i = 0; i < photosTaken.Count; i++)
        {
			temp = path + i + ".png";
			if (File.Exists(temp))
			{
				File.Delete(temp);
			}
			byte[] bytes;
			bytes = photosTaken[i].EncodeToPNG();
			File.WriteAllBytes(temp, bytes);
			//FileUtil.MoveFileOrDirectory(photosTakenPath[i], path + i + ".png");
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
