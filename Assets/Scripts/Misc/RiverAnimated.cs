using UnityEngine;
using System.Collections;

public class RiverAnimated : MonoBehaviour {
	public float speed = 1f;
	Material mat;
	float offset = 0f;
	public enum WaterType {River, Sea};
	public WaterType waterType = WaterType.River;
	// Use this for initialization
	void Start () {
		mat = gameObject.GetComponent<Renderer> ().material;
	}
	
	// Update is called once per frame
	void Update () {
		switch(waterType){
		case WaterType.River:
			offset += speed * Time.deltaTime;
			mat.mainTextureOffset = new Vector2 (offset, 0f);
			mat.SetTextureOffset ("_DetailAlbedoMap", new Vector2 (offset * 0.2f, 0f));
			break;
		case WaterType.Sea:
			offset = Mathf.Sin (speed * Time.timeSinceLevelLoad * Mathf.PI / 180f) * 0.1f;
			print (offset);
			mat.mainTextureOffset = new Vector2 (offset, 0f);
			offset = Mathf.Sin (speed * 0.5f * Time.timeSinceLevelLoad * Mathf.PI / 180f) * 0.1f;
			mat.SetTextureOffset ("_DetailAlbedoMap", new Vector2 (offset, 0f));
			break;
		}
	}
}
