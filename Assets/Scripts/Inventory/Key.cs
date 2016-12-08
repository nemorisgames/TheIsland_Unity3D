using UnityEngine;
using System.Collections;

public class Key : InventoryItem
{
	[SerializeField]
	KeyType keyType;
	public string tagToOpen;
	protected override void Awake()
	{
		type = ItemType.key;
		switch (keyType)
		{
			case KeyType.houseKey_01:
				tagToOpen = "key1";
				break;
			case KeyType.lockKey_01:
				tagToOpen = "something";
				break;
			default:
				tagToOpen = "none";
				break;
		}
		base.Awake();
	}
	public override void SetActive(bool b)
	{
		base.SetActive(b);
	}
	protected override void UseItem()
	{
		//don't call base
		Debug.Log("using key");
		//trigger keys
	}
	protected override void ThrowItem()
	{
		objectInHand.SetActive(false);
		base.ThrowItem();
	}
	//public override void Update()
	//{
	//	base.Update();
	//}
}
