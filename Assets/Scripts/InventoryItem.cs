using UnityEngine;
using System.Collections;
public class InventoryItem : MonoBehaviour
{
	public ItemType type;
	public bool activeItem = false;
	public bool hasItem = false;
	public GameObject hand;//contiene la mano para hacer sus animaciones
	public Animator anim;
	protected virtual void Awake()
	{
		hand.SetActive(false);
	}
	public virtual ItemType GetItemType()
	{
		return type;
	}
	public void SetActive(bool b)
	{
		if (hasItem)
		{
			activeItem = b;
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
		Inventory.Instance.RemoveInventoryItem(type);
	}
	protected virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && activeItem)
		{
			Debug.Log("Using Item");
			UseItem();
		}
		if (Input.GetMouseButtonDown(1) && activeItem)
		{
			Debug.Log("Throwing Item");
			ThrowItem();
		}
	}
}
