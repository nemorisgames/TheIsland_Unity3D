using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Photo : MonoBehaviour {
	public int pos = 0;
	public GameObject highlight;
	public RawImage texture;
	public void OnClick()
	{
		PhotoReview.Instance.ShowPhoto(pos);
	}
}
