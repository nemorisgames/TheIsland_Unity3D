using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	public class ToolbarLinks2DDemo : EditorWindow
	{

		[MenuItem ("Adventure Creator/Getting started/Load 2D Demo managers", false, 5)]
		static void Demo2D ()
		{
			ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/2D Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
			if (package != null)
			{
				package.AssignManagers ();
				AdventureCreator.RefreshActions ();
			}
		}

	}

}