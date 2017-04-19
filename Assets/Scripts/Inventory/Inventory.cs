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
	public Transform spawnPoint;
	private int currentPos = 0;
	private bool canUseMouseScroll = true;
	int cycleTry = -1;
	void Awake()
	{
		Instance = this;
		//for(int i = 0; i < inventoryItems.Length; i++)
		//{
		//	AddInventoryItem(inventoryItems[i].type);
		//	inventoryItems[i].SetActive(false);
		//}
		//AddInventoryItem(ItemType.stick);
		//inventoryItems[(int)ItemType.stick].SetActive(true);
	}
	void Start()
	{
		inventoryItems[0].SetActive(false);
	}
	void Update()
	{
		if (Time.timeScale == 0.0f)
			return;
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
	public IEnumerator RemoveInventoryItem(ItemType type)
	{
		inventoryItems[(int)type].hasItem = false;
		inventoryItems[(int)type].animator.SetTrigger("Throw");
		yield return new WaitForSeconds(1f);
		inventoryItems[(int)type].SetActive(false);

		AC.GlobalVariables.SetIntegerValue (1, -1);
		//inventoryItems[(int)type].hand.SetActive(false);
	}
	public void AddInventoryItem(ItemType type)
	{
		//checking
		inventoryItems[(int)type].hasItem = true;
		inventoryItems[(int)type].animator.SetTrigger("PickingUp");
		//inventoryItems[(int)type].SetActive(true);
		print ("addInventory " + inventoryItems.Length + " " + inventoryItems [(int)type].activeItem);
		AC.GlobalVariables.SetIntegerValue (1, (int)type); 
		//inventoryItems[(int)type].objectInHand.SetActive(true);
		for (int i = 0; i < inventoryItems.Length; i++)
			//if (!inventoryItems [(int)type].activeItem) {
			//	print ("cycle item");
				CycleInventoryItem (true);
			//}
	}

	public void CycleInventoryItem(){
		CycleInventoryItem (true);
	}

	public void CycleInventoryItem(bool forward)
	{
		cycleTry++;
		if (cycleTry > inventoryItems.Length)
		{
			cycleTry = -1;
			return;
		}
		//turn off current item, even if Cara doesn't has it
		inventoryItems[currentPos].SetActive(false);
		if (forward)
		{
			currentPos++;
			if (currentPos >= inventoryItems.Length)
			{
				currentPos = 0;
			}
		}
		else
		{
			currentPos--;
			if (currentPos < 0)
			{
				currentPos = inventoryItems.Length - 1;
			}
		}
		if (inventoryItems[currentPos].hasItem)
		{
			//if she has the item, set it active
			cycleTry = -1;
			inventoryItems[currentPos].SetActive(true);
		}
		else
		{
			//Cara doesn't have the item, cycle next
			CycleInventoryItem(forward);
		}
		print (inventoryItems[currentPos].hasItem + " " + currentPos);
		if(inventoryItems[currentPos].hasItem)
			AC.GlobalVariables.SetIntegerValue (1, currentPos);
	}
}