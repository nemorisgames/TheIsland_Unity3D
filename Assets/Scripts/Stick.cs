using UnityEngine;
using System.Collections;

public class Stick : InventoryItem {

	protected override void Awake()
	{
		type = ItemType.stick;
		base.Awake();
	}
	protected override void UseItem()
	{
		Debug.Log("using stick");
	} 
	//public override void Update()
	//{
	//	base.Update();
	//}
}
