using UnityEngine;
using System.Collections;
public enum KeyType
{
	houseKey_01,
	lockKey_01,
}
public class Stick : InventoryItem
{
	protected override void Awake()
	{
		type = ItemType.stick;
		base.Awake();
	}
	public override void SetActive(bool b)
	{
		base.SetActive(b);
	}
	protected override void UseItem()
	{		
		base.UseItem();
		Debug.Log("using stick");
	}
	protected override void ThrowItem()
	{
		base.ThrowItem();
	}
}
