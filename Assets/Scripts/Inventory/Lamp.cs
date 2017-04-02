using UnityEngine;
using System.Collections;
public class Lamp : InventoryItem
{
	protected override void Awake()
	{
		type = ItemType.lantern;
		base.Awake();
	}
	public override void SetActive(bool b)
	{
		base.SetActive(b);
	}
	protected override void UseItem()
	{		
		base.UseItem();
		Debug.Log("using lamp");
	}
	protected override void ThrowItem()
	{
		base.ThrowItem();
	}
}
