using UnityEngine;
using System.Collections;

public class CellPhone : MonoBehaviour
{
	public Transform target;
	GameObject cellphoneBody;
	public float speed = 1f;
	public float speedRotation = 1f;
	public Vector3 position;
	public Vector3 positionSelected;
	bool selected = false;
	public Light light;
	public Light lightStandBy;
	bool inTransition = false;
	public bool active = false;


	public enum CellphoneFunctions { Light, CameraPhoto, CameraVideo, ReviewPhotos, Call };
	public CellphoneFunctions currentFunction = CellphoneFunctions.Light;

	public Material cellphoneMaterialFunctions;
	int indiceActual = 0;

	[Header("ForScreenShots")]
	public TakePhoto photoFunctionality;
	private bool isSavingPhoto = false;
	private bool canUseMouseScroll = true;
	[Header("ForNotifications")]
	public GameObject notification;
	public AudioClip notificationSound;
	public AudioSource source;
	private float timeOut = 0;
	private bool showingNotification = false;
	private int maxNotification = 5;
	private int currentNotifications = 0;

	void Start()
	{
		cellphoneMaterialFunctions.mainTextureOffset = new Vector2(0f, 0.032f);
		notification.SetActive(false);
		cellphoneBody = transform.FindChild("Cuerpo").gameObject;
		cellphoneBody.SetActive(active);
	}

	public void activate()
	{
		activate(true);
	}

	public void deactivate()
	{
		activate(false);
	}

	public void activate(bool active)
	{
		this.active = active;
		cellphoneBody.SetActive(active);
	}
	void LateUpdate()
	{

		if (!selected)
		{
			Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * position.x + target.forward * position.z + target.up * position.y;
			transform.position = Vector3.Lerp(transform.position, tVec, Time.deltaTime * speed);
			transform.forward = Vector3.Lerp(transform.forward, target.forward, Time.deltaTime * speedRotation);
		}
		else
		{
			if (transform.parent != target)
			{
				Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * positionSelected.x + target.forward * positionSelected.z + target.up * positionSelected.y;
				transform.position = Vector3.Lerp(transform.position, tVec, Time.deltaTime * speed * 5f);
				transform.forward = Vector3.Lerp(transform.forward, target.forward, Time.deltaTime * speedRotation * 10f);
			}
		}

		if (!active)
			return;

		//Checkout on cellphone
		if (Input.GetMouseButtonDown(1))
		{
			if (!inTransition)
			{
				selected = !selected;
				if (selected)
				{
					StartCoroutine(waitPosition());
					CaraFunctions.Instance.leftHand.SendMessage("Hide");
					CaraFunctions.Instance.rightHand.SendMessage("Show");
					WhenUsingCellPhone();
				}
				else
				{
					transform.parent = null;
					CaraFunctions.Instance.leftHand.SendMessage("Show");
					CaraFunctions.Instance.rightHand.SendMessage("Hide");
				}
			}
			ScreenManager.Instance.CloseScreen();
			canUseMouseScroll = true;
		}

		if (Input.GetAxis("Mouse ScrollWheel") != 0f && canUseMouseScroll)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				nextFunction(true);
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				nextFunction(false);
			}
		}

		//Cellphone function
		if (Input.GetMouseButtonDown(0) && selected)
		{
			switch (currentFunction)
			{
				case CellphoneFunctions.Light:
					light.enabled = !light.enabled;
					AC.GlobalVariables.SetBooleanValue(0, light.enabled);
					break;
				case CellphoneFunctions.CameraPhoto:
					StartCoroutine(takePhoto());
					break;
				case CellphoneFunctions.ReviewPhotos:
					ScreenManager.Instance.ShowScreen(ScreenType.PhotoView);
					canUseMouseScroll = false;
					break;
			}
		}

		cellphoneMaterialFunctions.mainTextureOffset = new Vector2(0f, Mathf.Lerp(cellphoneMaterialFunctions.mainTextureOffset.y, (indiceActual * 0.2f + 0.032f), Time.deltaTime * 3f));
	}
	IEnumerator takePhoto()
	{
		float intensityOriginal = light.intensity;
		float angleOriginal = light.spotAngle;
		bool lightEnabledOriginal = light.enabled;
		light.enabled = true;

		light.spotAngle = 140f;
		lightStandBy.enabled = true;
		while (light.intensity > intensityOriginal * 0.1f)
		{
			light.intensity -= intensityOriginal / 20f;
			yield return new WaitForSeconds(0.01f);
		}

		yield return new WaitForSeconds(0.2f);
		lightStandBy.enabled = false;
		lightStandBy.intensity = 2f;

		while (light.intensity < intensityOriginal * 4f)
		{
			light.intensity += intensityOriginal * 4f / 5f;
			yield return new WaitForSeconds(0.01f);
			if (!isSavingPhoto)
			{
				isSavingPhoto = true;
				photoFunctionality.SaveCameraScreenShot();
			}
		}
		yield return new WaitForSeconds(0.2f);
		light.spotAngle = angleOriginal;
		light.intensity = intensityOriginal;
		light.enabled = lightEnabledOriginal;
		isSavingPhoto = false;
	}

	void nextFunction(bool next)
	{
		indiceActual = Mathf.Clamp(indiceActual + (next ? 1 : -1), 0, 4);
		switch (indiceActual)
		{
			case 0: currentFunction = CellphoneFunctions.Light; break;
			case 1: currentFunction = CellphoneFunctions.CameraPhoto; break;
			case 2: currentFunction = CellphoneFunctions.CameraVideo; break;
			case 3: currentFunction = CellphoneFunctions.ReviewPhotos; break;
			case 4: currentFunction = CellphoneFunctions.Call; break;
		}
	}

	IEnumerator waitPosition()
	{
		inTransition = true;
		yield return new WaitForSeconds(.3f);
		transform.parent = target;
		Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * positionSelected.x + target.forward * positionSelected.z + target.up * positionSelected.y;
		transform.position = tVec;
		transform.forward = target.forward;
		transform.rotation = target.rotation;
		inTransition = false;
	}
	void WhenUsingCellPhone()
	{
		if (showingNotification)
		{
			timeOut = Time.time;
			InvokeRepeating("NotificationDuration", 0f, 1f);
			showingNotification = false;
			CancelInvoke("NotificationSound");
		}
	}
	void ShowNotification()
	{		
		if (!showingNotification)
		{
			currentNotifications = 0;
			showingNotification = true; //can be used to close notification when she shows the cell phone
			InvokeRepeating("NotificationSound", 0f, 1.5f);
		}		
	}
	void NotificationSound()
	{
		//make sound
		currentNotifications++;
		source.clip = notificationSound;
		source.Play();
		if (currentNotifications >= maxNotification)
		{
			showingNotification = false;
			CancelInvoke("NotificationSound");
			return;
		}			
	}
	void NotificationDuration()
	{
		notification.SetActive(true);
		//define max time
		if (Time.time - timeOut >= 5f)
		{
			timeOut = 0;
			CancelInvoke("NotificaionDuration");
			notification.SetActive(false);			
		}
	}
}
