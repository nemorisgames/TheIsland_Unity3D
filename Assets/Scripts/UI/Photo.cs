using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Photo : MonoBehaviour
{
	public int pos = 0;
	public Color higlighted;
	public Color unHiglighted;
	public Image highlight;
	public RawImage texture;
	public void OnClick()
	{
		PhotoReview.Instance.ShowPhoto(pos);
	}
	public void Highlight()
	{
		highlight.color = higlighted;
	}
	public void UnHighlight()
	{
		highlight.color = unHiglighted;
	}
}
