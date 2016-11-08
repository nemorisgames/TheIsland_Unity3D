using UnityEngine;
using System.Collections;

public class TakePhoto : MonoBehaviour {
    [Header("ForScreenShots")]
    public Camera cellphoneView;
    public RenderTexture screenTexture;

    bool save = false;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SaveCameraScreenShot()
    {
        Debug.Log("Saving Screen Shot");        
        save = true;
    }
    void OnPostRender()
    {
        if (save)
        {
            int width = cellphoneView.pixelWidth;
            int height = cellphoneView.pixelHeight;
            Debug.Log(width + " " + height);
            //RenderTexture.active = tempRT;
            Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
            // false, meaning no need for mipmaps
            virtualPhoto.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            virtualPhoto.Apply();

            // consider ... Destroy(tempRT);

            byte[] bytes;
            bytes = virtualPhoto.EncodeToPNG();

            System.IO.File.WriteAllBytes(ScreenShotLocation(), bytes);
            save = false;
            RenderTexture.active = screenTexture; //can help avoid errors 
            cellphoneView.targetTexture = screenTexture;

        }
       

    }
    private string ScreenShotLocation()
    {
        string r = Application.persistentDataPath + "/p.png";
        Debug.Log(r);
        return r;
    }
}
