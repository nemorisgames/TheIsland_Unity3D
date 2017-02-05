using UnityEngine;
using System.Collections;
public class InventoryItem : MonoBehaviour
{
	public ItemType type;
	public GameObject objectInHand;
	public GameObject throwPrefab;
	public Transform spawnPosition;
	public GameObject hand;//contiene la mano para hacer sus animaciones
	public Animator anim;

	public bool activeItem = false;
	public bool hasItem = false;

	bool throwing = false;
	float timeOut = 0f;
	float throwTimeout = 2f;
	public Animator animator;
	protected virtual void Awake()
	{
		//hand.SetActive(false);
		animator = GetComponent<Animator>();
	}
	public virtual ItemType GetItemType()
	{
		return type;
	}
	public virtual void SetActive(bool b)
	{
		if (!b)
		{
			objectInHand.SetActive(b);
			hand.SetActive(b);
		}
		if (hasItem)
		{
			activeItem = b;
			objectInHand.SetActive(activeItem);
			hand.SetActive(activeItem);
		}
		else
		{
			Debug.Log("Item not in inventory");
		}
	}
	protected virtual void UseItem()
	{

	}
	protected virtual void ThrowItem()
	{
		//set animation

		//throw gameObject
		objectInHand.SetActive(false);
		spawnPosition = Inventory.Instance.spawnPoint;
		GameObject ob = Instantiate(throwPrefab,spawnPosition.position,spawnPosition.rotation) as GameObject;
		ob.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 10f, ForceMode.Impulse);
		Destroy (ob, 10f);
	}
	protected virtual void Update()
	{
		if (!activeItem)
			return;
		if (Input.GetMouseButtonDown(0))
		{
			//first frame while pressing left click
			timeOut = Time.time;
			throwing = false;
		}
		if (Input.GetMouseButton(0))
		{
			//while pressing wait 2 seconds
			if (Time.time - timeOut >= throwTimeout && !throwing)
			{
				throwing = true;
				Debug.Log("Throwing Item");
				StartCoroutine(Inventory.Instance.RemoveInventoryItem(type));
				Invoke ("ThrowItem", 1f);
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (Time.time - timeOut < throwTimeout)
			{
				Debug.Log("Using Item");
				UseItem();
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			//cancel throw
			throwing = true;
		}
	}
}