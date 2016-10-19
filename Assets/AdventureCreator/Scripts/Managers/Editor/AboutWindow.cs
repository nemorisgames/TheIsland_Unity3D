using UnityEngine;
using UnityEditor;

namespace AC
{

	public class AboutWindow : EditorWindow
	{

		private static AboutWindow window;

		[MenuItem ("Adventure Creator/About", false, 20)]
		static void Init ()
		{
			if (window)
				return;

			window = ScriptableObject.CreateInstance<AboutWindow>();
			UnityVersionHandler.SetWindowTitle (window, "About AC");
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
			window.ShowAuxWindow ();
		}


		private void OnGUI ()
		{
			if (Resource.ACLogo)
			{
				GUILayout.Label (Resource.ACLogo);
			}
			GUILayout.Label ("By Chris Burton, ICEBOX Studios 2013-2016", EditorStyles.largeLabel);
			GUILayout.Label ("Version " + AdventureCreator.version);
		}

	}

}