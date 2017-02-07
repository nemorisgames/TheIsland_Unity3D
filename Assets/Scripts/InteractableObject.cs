using UnityEngine;
using System.Collections;
public enum ObjectType
{
	tree,	
}
public class InteractableObject : MonoBehaviour
{
	[SerializeField]
	ObjectType type;
	[SerializeField]
	Animator anim;
	public string triggerName;
	void Update()
	{
		
	}
	void OnCollisionEnter(Collision collision)
	{
		switch (type)
		{
			case ObjectType.tree:
				if (collision.collider.tag == "stick")
				{
					Destroy(collision.gameObject);
					anim.SetTrigger(triggerName);			
				}
				break;			
			default:
				break;
		}
	}
}
