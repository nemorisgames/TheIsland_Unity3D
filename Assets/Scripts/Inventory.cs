using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public enum ItemType
{
	stick,
	key,
	lantern,
	axe,
	matches,
}
public class Inventory : MonoBehaviour
{
	public static Inventory Instance = null;
	[SerializeField]
	InventoryItem[] inventoryItems;//same order as ItemType enum
	private int currentPos = 0;
	private bool canUseMouseScroll = true;

	void Awake()
	{
		Instance = this;
	}
	void Start()
	{

	}
	void Update()
	{
		if (Input.GetAxis("Mouse ScrollWheel") != 0f && canUseMouseScroll)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				CycleInventoryItem(true);
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				CycleInventoryItem(false);
			}
		}
	}
	public void RemoveInventoryItem(ItemType type)
	{
		inventoryItems[(int)type].hasItem = false;
		inventoryItems[(int)type].hand.SetActive(false);
	}
	public void AddInventoryItem(ItemType type)
	{
		//checking
		inventoryItems[(int)type].hasItem = true;
		inventoryItems[(int)type].hand.SetActive(true);
	}
	public void CycleInventoryItem(bool forward)
	{
		if (forward)
		{
			currentPos++;
		}
		else
		{
			currentPos--;
		}
	}
}
