/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2016
 *	
 *	"SpeechManager.cs"
 * 
 *	This script handles the "Speech" tab of the main wizard.
 *	It is used to auto-number lines for audio files, and handle translations.
 * 
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Handles the "Speech" tab of the Game Editor window.
	 * All translations for a game's text are stored here, as are the settings that control how speech is handled in-game.
	 */
	[System.Serializable]
	public class SpeechManager : ScriptableObject
	{

		/** If True, then speech text will scroll when displayed */
		public bool scrollSubtitles = true;
		/** If True, then narration text will scroll when displayed */
		public bool scrollNarration = false;
		/** The speed of scrolling text */
		public float textScrollSpeed = 50;
		/** The AudioClip to play when scrolling text */
		public AudioClip textScrollCLip = null;
		/** If True, the textScrollClip audio will be played with every character addition to the subtitle text, as opposed to waiting for the previous audio to end */
		public bool playScrollAudioEveryCharacter = true;

		/** If True, then speech text will remain on the screen until the player clicks */
		public bool displayForever = false;
		/** The minimum time, in seconds, that a speech line will be displayed (unless an AudioClip is setting it's length) */
		public float minimumDisplayTime = 1f;
		/** The time that speech text will be displayed, divided by the number of characters in the text, if displayForever = False */
		public float screenTimeFactor = 0.1f;
		/** If True, then speech text during a cutscene can be skipped by the player left-clicking */
		public bool allowSpeechSkipping = false;
		/** If True, then speech text during gameplay can be skipped by the player left-clicking */
		public bool allowGameplaySpeechSkipping = false;
		/** If True, then left-clicking will complete any scrolling speech text */
		public bool endScrollBeforeSkip = false;

		/** If True, then speech audio files will play when characters speak */
		public bool searchAudioFiles = true;
		/** If True, then the audio files associated with speech text will be named automatically according to their ID number */
		public bool autoNameSpeechFiles = true;
		/** If True, then speech text will always display if no relevant audio file is found - even if Subtitles are off in the Options menu */
		public bool forceSubtitles = true;
		/** If True, then each translation will have its own set of speech audio files */
		public bool translateAudio = true;
		/** If True, then translations that don't have speech audio files will use the audio files from the game's original language */
		public bool fallbackAudio = false;
		/** If True, then the text stored in the speech buffer (in MenuLabel) will not be cleared when no speech text is active */
		public bool keepTextInBuffer = false;
		/** If True, then background speech audio will end if foreground speech audio begins to play */
		public bool relegateBackgroundSpeechAudio = false;
		/** If True, then speech audio spoken by the player will expect the audio filenames to be named after the player's prefab, rather than just "Player" */
		public bool usePlayerRealName = false;

		/** If True, then speech audio files will need to be placed in subfolders named after the character who speaks */
		public bool placeAudioInSubfolders = false;
		/** If True, then a speech line will be split by carriage returns into separate speech lines */
		public bool separateLines = false;
		/** The delay between carriage return-separated speech lines, if separateLines = True */
		public float separateLinePause = 1f;

		/** All SpeechLines generated to store translations and audio filename references */
		public List<SpeechLine> lines = new List<SpeechLine> ();
		/** The names of the game's languages. The first is always "Original". */
		public List<string> languages = new List<string> ();
		/** If True, then the game's original text cannot be displayed in-game, and only translations will be available */
		public bool ignoreOriginalText = false;
	
		/** The factor by which to reduce SFX audio when speech plays */
		public float sfxDucking = 0f;
		/** The factor by which to reduce music audio when speech plays */
		public float musicDucking = 0f;

		/** The game's lip-syncing method (Off, FromSpeechText, ReadPamelaFile, ReadSapiFile, ReadPapagayoFile, FaceFX, Salsa2D) */
		public LipSyncMode lipSyncMode = LipSyncMode.Off;
		/** What lip-syncing actually affects (Portrait, PortraitAndGameObject, GameObjectTexture) */
		public LipSyncOutput lipSyncOutput = LipSyncOutput.Portrait;
		/** The phoneme bins used to separate phonemes into animation frames */
		public List<string> phonemes = new List<string> ();
		/** The speed at which to process lip-sync data */
		public float lipSyncSpeed = 1f;

		#if UNITY_EDITOR

		/** An array of all scene names in the Build settings */
		public string[] sceneFiles;
		/** The current SpeechLine selected to reveal its properties */
		public SpeechLine activeLine = null;
		/** If True, then speech lines that are exactly the same will share the same ID number */
		public bool mergeMatchingSpeechIDs = false;

		/** If True, then 'Dialogue: Play speech' Actions can be assigned a SpeechTag, or label, to use when exporting script sheets */
		public bool useSpeechTags = false;
		/** A List of the available SpeechTags */
		public List<SpeechTag> speechTags = new List<SpeechTag>();
		
		private List<string> sceneNames = new List<string>();
		private List<SpeechLine> tempLines = new List<SpeechLine>();
		private string sceneLabel;

		private List<ActionListAsset> allActionListAssets;
		private string textFilter;
		private FilterSpeechLine filterSpeechLine = FilterSpeechLine.Text;
		private List<ActionListAsset> checkedAssets = new List<ActionListAsset>();
		private AC_TextType typeFilter = AC_TextType.Speech;
		private int tagFilter;
		private int sceneFilter;
		private int sideLanguage;
		

		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox ("Exporting game text cannot be performed in WebPlayer mode - please switch platform to do so.", MessageType.Warning);
			GUILayout.Space (10);
			#endif
			
			EditorGUILayout.LabelField ("Subtitles", EditorStyles.boldLabel);
			
			separateLines = EditorGUILayout.ToggleLeft ("Treat carriage returns as separate speech lines?", separateLines);
			if (separateLines)
			{
				separateLinePause = EditorGUILayout.Slider ("Split line delay (s):", separateLinePause, 0f, 1f);
			}
			scrollSubtitles = EditorGUILayout.ToggleLeft ("Scroll speech text?", scrollSubtitles);
			scrollNarration = EditorGUILayout.ToggleLeft ("Scroll narration text?", scrollNarration);
			if (scrollSubtitles || scrollNarration)
			{
				textScrollSpeed = EditorGUILayout.FloatField ("Text scroll speed:", textScrollSpeed);
			}
			
			displayForever = EditorGUILayout.ToggleLeft ("Display speech forever until user skips it?", displayForever);
			if (displayForever)
			{
				endScrollBeforeSkip = EditorGUILayout.ToggleLeft ("Skipping speech first displays currently-scrolling text?", endScrollBeforeSkip);
				allowGameplaySpeechSkipping = EditorGUILayout.ToggleLeft ("Speech during gameplay can also be skipped?", allowGameplaySpeechSkipping);
			}
			else
			{
				minimumDisplayTime = EditorGUILayout.FloatField ("Minimum display time (s):", minimumDisplayTime);
				screenTimeFactor = EditorGUILayout.FloatField ("Display time factor:", screenTimeFactor);
				allowSpeechSkipping = EditorGUILayout.ToggleLeft ("Subtitles can be skipped?", allowSpeechSkipping);
				if (allowSpeechSkipping)
				{
					allowGameplaySpeechSkipping = EditorGUILayout.ToggleLeft ("Speech during gameplay can also be skipped?", allowGameplaySpeechSkipping);
					if (scrollSubtitles)
					{
						endScrollBeforeSkip = EditorGUILayout.ToggleLeft ("Skipping speech first displays currently-scrolling text?", endScrollBeforeSkip);
					}
				}
			}
			
			keepTextInBuffer = EditorGUILayout.ToggleLeft ("Retain subtitle text buffer once line has ended?", keepTextInBuffer);

			if (GUILayout.Button ("Edit speech tags"))
			{
				SpeechTagsWindow window = (SpeechTagsWindow) EditorWindow.GetWindow (typeof (SpeechTagsWindow));
				window.Repaint ();
			}

			if (scrollSubtitles || scrollNarration)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Subtitle-scrolling audio", EditorStyles.boldLabel);
				textScrollCLip = (AudioClip) EditorGUILayout.ObjectField ("Default text scroll audio:", textScrollCLip, typeof (AudioClip), false);
				playScrollAudioEveryCharacter = EditorGUILayout.Toggle ("Play audio on every letter?", playScrollAudioEveryCharacter);
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Speech audio", EditorStyles.boldLabel);

			forceSubtitles = EditorGUILayout.ToggleLeft ("Force subtitles to display when no speech audio is found?", forceSubtitles);
			searchAudioFiles = EditorGUILayout.ToggleLeft ("Auto-play speech audio files?", searchAudioFiles);
			autoNameSpeechFiles = EditorGUILayout.ToggleLeft ("Auto-name speech audio files?", autoNameSpeechFiles);
			translateAudio = EditorGUILayout.ToggleLeft ("Speech audio can be translated?", translateAudio);
			if (translateAudio)
			{
				fallbackAudio = EditorGUILayout.ToggleLeft ("Use original language audio if none found?", fallbackAudio);
			}
			usePlayerRealName = EditorGUILayout.ToggleLeft ("Use Player prefab name in filenames?", usePlayerRealName);
			if (autoNameSpeechFiles)
			{
				placeAudioInSubfolders = EditorGUILayout.ToggleLeft ("Place audio files in speaker subfolders?", placeAudioInSubfolders);
			}
			sfxDucking = EditorGUILayout.Slider ("SFX reduction during:", sfxDucking, 0f, 1f);
			musicDucking = EditorGUILayout.Slider ("Music reduction during:", musicDucking, 0f, 1f);
			relegateBackgroundSpeechAudio = EditorGUILayout.ToggleLeft ("End background speech audio if non-background plays?", relegateBackgroundSpeechAudio);
			mergeMatchingSpeechIDs = EditorGUILayout.ToggleLeft ("Give matching speech lines the same ID?", mergeMatchingSpeechIDs);

			EditorGUILayout.Space ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Lip synching", EditorStyles.boldLabel);
			
			lipSyncMode = (LipSyncMode) EditorGUILayout.EnumPopup ("Lip syncing:", lipSyncMode);
			if (lipSyncMode == LipSyncMode.FromSpeechText || lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.ReadPapagayoFile)
			{
				lipSyncOutput = (LipSyncOutput) EditorGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput);
				lipSyncSpeed = EditorGUILayout.FloatField ("Process speed:", lipSyncSpeed);
				
				if (GUILayout.Button ("Phonemes Editor"))
				{
					PhonemesWindow window = (PhonemesWindow) EditorWindow.GetWindow (typeof (PhonemesWindow));
					window.Repaint ();
				}

				if (lipSyncOutput == LipSyncOutput.GameObjectTexture)
				{
					EditorGUILayout.HelpBox ("Characters will require the 'LipSyncTexture' component in order to perform lip-syncing.", MessageType.Info);
				}
			}
			else if (lipSyncMode == LipSyncMode.FaceFX && !FaceFXIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
			}
			else if (lipSyncMode == LipSyncMode.Salsa2D)
			{
				lipSyncOutput = (LipSyncOutput) EditorGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput);
				
				EditorGUILayout.HelpBox ("Speaking animations must have 4 frames: Rest, Small, Medium and Large.", MessageType.Info);
				
				#if !SalsaIsPresent
				EditorGUILayout.HelpBox ("The 'SalsaIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				#endif
			}
			else if (lipSyncMode == LipSyncMode.RogoLipSync && !RogoLipSyncIntegration.IsDefinePresent ())
			{
				EditorGUILayout.HelpBox ("The 'RogoLipSyncIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
			}

			EditorGUILayout.Space ();
			LanguagesGUI ();
			
			EditorGUILayout.Space ();
			
			GUILayout.Label ("Game text", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Gather text", EditorStyles.miniButtonLeft))
			{
				PopulateList ();
				
				if (sceneFiles.Length > 0)
				{
					Array.Sort (sceneFiles);
				}
			}
			if (GUILayout.Button ("Reset text", EditorStyles.miniButtonMid))
			{
				ClearList ();
			}

			if (lines.Count == 0)
			{
				GUI.enabled = false;
			}
			
			if (GUILayout.Button ("Create script sheet..", EditorStyles.miniButtonRight))
			{
				if (lines.Count > 0)
				{
					ScriptSheetWindow.Init ();
				}
			}
			EditorGUILayout.EndHorizontal ();


			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Import all translations", EditorStyles.miniButtonLeft))
			{
				ImportGameText ();
			}
			if (GUILayout.Button ("Export all translations", EditorStyles.miniButtonRight))
			{
				ExportGameText ();
			}
			EditorGUILayout.EndHorizontal ();
			
			GUI.enabled = true;

			if (lines.Count > 0)
			{
				EditorGUILayout.Space ();
				ListLines ();
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void GetSceneNames ()
		{
			sceneNames.Clear ();
			sceneNames.Add ("(No scene)");
			sceneNames.Add ("(Any or no scene)");
			foreach (string sceneFile in sceneFiles)
			{
				int slashPoint = sceneFile.LastIndexOf ("/") + 1;
				string sceneName = sceneFile.Substring (slashPoint);

				sceneNames.Add (sceneName.Substring (0, sceneName.Length - 6));
			}
		}
		
		
		private void ListLines ()
		{
			if (sceneNames == null || sceneNames == new List<string>() || sceneNames.Count != (sceneFiles.Length + 2))
			{
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
			}
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type filter:", GUILayout.Width (65f));
			typeFilter = (AC_TextType) EditorGUILayout.EnumPopup (typeFilter);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Scene filter:", GUILayout.Width (65f));
			sceneFilter = EditorGUILayout.Popup (sceneFilter, sceneNames.ToArray ());
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Text filter:", GUILayout.Width (65f));
			filterSpeechLine = (FilterSpeechLine) EditorGUILayout.EnumPopup (filterSpeechLine, GUILayout.MaxWidth (100f));
			textFilter = EditorGUILayout.TextField (textFilter);
			EditorGUILayout.EndHorizontal ();

			if (useSpeechTags && speechTags != null && speechTags.Count > 1)
			{
				List<string> tagNames = new List<string>();
				foreach (SpeechTag speechTag in speechTags)
				{
					tagNames.Add (speechTag.label);
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Tag filter:", GUILayout.Width (65f));
				tagFilter = EditorGUILayout.Popup (tagFilter, tagNames.ToArray ());
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				tagFilter = -1;
			}

			EditorGUILayout.Space ();

			if (sceneNames.Count <= sceneFilter)
			{
				sceneFilter = 0;
				return;
			}
			string selectedScene = sceneNames[sceneFilter] + ".unity";
			foreach (SpeechLine line in lines)
			{
				if (line.textType == typeFilter && line.Matches (textFilter, filterSpeechLine))
				{
					string scenePlusExtension = (line.scene != "") ? (line.scene + ".unity") : "";
					
					if ((line.scene == "" && sceneFilter == 0)
					    || sceneFilter == 1
					    || (line.scene != "" && sceneFilter > 1 && line.scene.EndsWith (selectedScene))
					    || (line.scene != "" && sceneFilter > 1 && scenePlusExtension.EndsWith (selectedScene)))
					{
						if (tagFilter == -1
						|| (tagFilter < speechTags.Count && line.tagID == speechTags[tagFilter].ID))
						{
							line.ShowGUI ();
						}
					}
				}
			}
		}
		
		
		private void LanguagesGUI ()
		{
			GUILayout.Label ("Translations", EditorStyles.boldLabel);

			if (lines.Count == 0)
			{
				EditorGUILayout.HelpBox ("No text has been gathered for translations - add your scenes to the build, and click 'Gather text' below.", MessageType.Info);
				return;
			}

			if (languages.Count == 0)
			{
				ClearLanguages ();
			}
			else
			{
				if (languages.Count > 1)
				{
					ignoreOriginalText = EditorGUILayout.ToggleLeft ("Prevent original language from being used?", ignoreOriginalText);
					if (!ignoreOriginalText)
					{
						languages[0] = EditorGUILayout.TextField ("Name of original language:", languages[0]);
					}
					
					for (int i=1; i<languages.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Language #" + i.ToString (), GUILayout.Width (146f));
						languages[i] = EditorGUILayout.TextField (languages[i]);

						if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (i);
						}
						EditorGUILayout.EndHorizontal ();
					}
				}

				if (GUILayout.Button ("Create new translation"))
				{
					Undo.RecordObject (this, "Add translation");
					CreateLanguage ("New " + languages.Count.ToString ());
				}
			}
		}


		private void SideMenu (int i)
		{
			GenericMenu menu = new GenericMenu ();

			sideLanguage = i;
			menu.AddItem (new GUIContent ("Import"), false, MenuCallback, "Import translation");
			menu.AddItem (new GUIContent ("Export"), false, MenuCallback, "Export translation");
			menu.AddItem (new GUIContent ("Delete"), false, MenuCallback, "Delete translation");

			if (lines.Count > 0)
			{
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Create script sheet.."), false, MenuCallback, "Create script sheet");
			}

			menu.ShowAsContext ();
		}


		private void MenuCallback (object obj)
		{
			if (sideLanguage >= 0)
			{
				int i = sideLanguage;

				switch (obj.ToString ())
				{
				case "Import translation":
					ImportTranslation (i);
					break;

				case "Export translation":
					ExportTranslation (i);
					break;

				case "Delete translation":
					Undo.RecordObject (this, "Delete translation: " + languages[i]);
					DeleteLanguage (i);
					break;

				case "Create script sheet":
					ScriptSheetWindow.Init (i);
					break;
				}
			}
			
			sideLanguage = -1;
		}
		
		
		private void CreateLanguage (string name)
		{
			languages.Add (name);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Add (line.text);
			}
		}
		
		
		private void DeleteLanguage (int i)
		{
			languages.RemoveAt (i);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.RemoveAt (i-1);

				if (line.customTranslationAudioClips != null && line.customTranslationAudioClips.Count > (i-1))
				{
					line.customTranslationAudioClips.RemoveAt (i-1);
				}
				if (line.customTranslationLipsyncFiles != null && line.customTranslationLipsyncFiles.Count > (i-1))
				{
					line.customTranslationLipsyncFiles.RemoveAt (i-1);
				}
			}
			
		}
		

		/**
		 * Removes all translations.
		 */
		public void ClearLanguages ()
		{
			languages.Clear ();
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Clear ();
				line.customTranslationAudioClips.Clear ();
				line.customTranslationLipsyncFiles.Clear ();
			}
			
			languages.Add ("Original");	
		}
		
		
		private void PopulateList ()
		{
			string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();

			bool canProceed = EditorUtility.DisplayDialog ("Gather game text", "AC will now go through your game, and collect all game text so that it can be translated/voiced.\n\nIt is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			if (UnityVersionHandler.SaveSceneIfUserWants ())
			{
				Undo.RecordObject (this, "Update speech list");
				
				// Store the lines temporarily, so that we can update the translations afterwards
				BackupTranslations ();
				
				lines.Clear ();
				checkedAssets.Clear ();
				
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
				
				// First look for lines that already have an assigned lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, false);
				}

				GetLinesFromSettings (false);
				GetLinesFromInventory (false);
				GetLinesFromVariables (true, false);
				GetLinesFromCursors (false);
				GetLinesFromMenus (false);

				GetAllActionListAssets ();
				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					ProcessActionListAsset (actionListAsset, false);
				}
				
				checkedAssets.Clear ();
				
				// Now look for new lines, which don't have a unique lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, true);
				}

				GetLinesFromSettings (true);
				GetLinesFromInventory (true);
				GetLinesFromVariables (true, true);
				GetLinesFromCursors (true);
				GetLinesFromMenus (true);

				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					ProcessActionListAsset (actionListAsset, true);
				}

				if (mergeMatchingSpeechIDs)
				{
					MergeMatchingSpeechIDs ();
				}
				
				RestoreTranslations ();
				checkedAssets.Clear ();

				allActionListAssets.Clear ();
				UnityVersionHandler.OpenScene (originalScene);
			}
		}
		
		
		private string RemoveLineBreaks (string text)
		{
			if (text.Length == 0) return " ";
            text = text.Replace("\r\n", "[break]").Replace("\n", "[break]");//.Replace ("\r", "[break3]");
            return text;
        }
		
		
		private string AddLineBreaks (string text)
		{
            text = text.Replace ("[break]", "\n");
            return text;
        }
		
		
		private void ExtractConversation (Conversation conversation, bool onlySeekNew)
		{
			foreach (ButtonDialog dialogOption in conversation.options)
			{
				if (onlySeekNew && dialogOption.lineID < 1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetIDArray(), UnityVersionHandler.GetCurrentSceneName (), dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
					dialogOption.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && dialogOption.lineID > 0)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (dialogOption.lineID, UnityVersionHandler.GetCurrentSceneName (), dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) dialogOption.lineID = lineID;
				}
			}
		}
		
		
		private void ExtractHotspot (Hotspot hotspot, bool onlySeekNew)
		{
			string hotspotName = hotspot.name;
			if (hotspot.hotspotName != "")
			{
				hotspotName = hotspot.hotspotName;
			}
			
			if (onlySeekNew && hotspot.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), UnityVersionHandler.GetCurrentSceneName (), hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				hotspot.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && hotspot.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (hotspot.lineID, UnityVersionHandler.GetCurrentSceneName (), hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) hotspot.lineID = lineID;
			}
		}


		private void ExtractCharacter (AC.Char character, bool onlySeekNew)
		{
			if (character == null) return;
			string characterName = character.speechLabel;

			if (character.speechLabel != null && character.speechLabel.Length > 0)
			{
				if (onlySeekNew && character.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetIDArray(), UnityVersionHandler.GetCurrentSceneName (), characterName, languages.Count - 1, AC_TextType.Character);
					
					character.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && character.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (character.lineID, UnityVersionHandler.GetCurrentSceneName (), characterName, languages.Count - 1, AC_TextType.Character);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) character.lineID = lineID;
				}
			}
		}

		
		private void ExtractInventory (InvItem invItem, bool onlySeekNew)
		{
			if (onlySeekNew && invItem.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				newLine = new SpeechLine (GetIDArray(), "", _label, languages.Count - 1, AC_TextType.InventoryItem);
				invItem.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && invItem.lineID > -1)
			{
				// Already has an ID, so don't replace
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				SpeechLine existingLine = new SpeechLine (invItem.lineID, "", _label, languages.Count - 1, AC_TextType.InventoryItem);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) invItem.lineID = lineID;
			}
		}
		
		
		private void ExtractPrefix (HotspotPrefix prefix, bool onlySeekNew)
		{
			if (onlySeekNew && prefix.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				prefix.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			else if (!onlySeekNew && prefix.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (prefix.lineID, "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) prefix.lineID = lineID;
			}
		}
		
		
		private void ExtractIcon (CursorIcon icon, bool onlySeekNew)
		{
			if (onlySeekNew && icon.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetIDArray(), "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				icon.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && icon.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (icon.lineID, "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) icon.lineID = lineID;
			}
		}
		
		
		private void ExtractElement (MenuElement element, string elementLabel, bool onlySeekNew)
		{
			if (elementLabel == null || elementLabel.Length == 0)
			{
				element.lineID = -1;
				return;
			}

			if (onlySeekNew && element.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				element.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && element.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (element.lineID, "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) element.lineID = lineID;
			}
		}
		
		
		private void ExtractHotspotOverride (MenuButton button, string hotspotLabel, bool onlySeekNew)
		{
			if (hotspotLabel == "")
			{
				button.hotspotLabelID = -1;
				return;
			}
			
			if (onlySeekNew && button.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetIDArray(), "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				button.hotspotLabelID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && button.hotspotLabelID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (button.hotspotLabelID, "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) button.hotspotLabelID = lineID;
			}
		}
		
		
		private void ExtractJournalElement (MenuJournal journal, List<JournalPage> pages, bool onlySeekNew)
		{
			foreach (JournalPage page in pages)
			{
				if (onlySeekNew && page.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetIDArray(), "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					page.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && page.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (page.lineID, "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) page.lineID = lineID;
				}
			}
		}
		
		
		private void ExtractSpeech (ActionSpeech action, bool onlySeekNew, bool isInScene, int tagID)
		{
			string speaker = "";
			bool isPlayer = action.isPlayer;
			if (!isPlayer && action.speaker != null && action.speaker is Player)
			{
				isPlayer = true;
			}
			
			if (isPlayer)
			{
				speaker = "Player";

				if (action.isPlayer && KickStarter.settingsManager != null && KickStarter.settingsManager.player)
				{
					speaker = KickStarter.settingsManager.player.name;
				}
				else if (action.speaker != null)
				{
					speaker = action.speaker.name;
				}
			}
			else
			{
				if (!isInScene)
				{
					action.SetSpeaker ();
				}

				if (action.speaker)
				{
					speaker = action.speaker.name;
				}
				else
				{
					speaker = "Narrator";
				}
			}

			if (speaker != "" && action.messageText != "")
			{
				if (separateLines)
				{
					string[] messages = action.GetSpeechArray ();
					if (messages != null && messages.Length > 0)
					{
						action.lineID = ProcessSpeechLine (onlySeekNew, isInScene, action.lineID, speaker, messages[0], isPlayer, tagID);

						if (messages.Length > 1)
						{
							if (action.multiLineIDs == null || action.multiLineIDs.Length != (messages.Length - 1))
							{
								List<int> lineIDs = new List<int>();
								for (int i=1; i<messages.Length; i++)
								{
									if (action.multiLineIDs != null && action.multiLineIDs.Length > (i-1))
									{
										lineIDs.Add (action.multiLineIDs[i-1]);
									}
									else
									{
										lineIDs.Add (-1);
									}
								}
								action.multiLineIDs = lineIDs.ToArray ();
							}

							for (int i=1; i<messages.Length; i++)
							{
								action.multiLineIDs [i-1] = ProcessSpeechLine (onlySeekNew, isInScene, action.multiLineIDs [i-1], speaker, messages[i], isPlayer, tagID);
							}
						}
						else
						{
							action.multiLineIDs = null;
						}
					}
				}
				else
				{
					action.lineID = ProcessSpeechLine (onlySeekNew, isInScene, action.lineID, speaker, action.messageText, isPlayer, tagID);
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
				action.multiLineIDs = null;
			}
		}


		private int ProcessSpeechLine (bool onlySeekNew, bool isInScene, int lineID, string speaker, string messageText, bool isPlayer, int tagID)
		{
			if (onlySeekNew && lineID == -1)
			{
				// Assign a new ID on creation
				string _scene = "";
				SpeechLine newLine;
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				newLine = new SpeechLine (GetIDArray(), _scene, speaker, messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
				newLine.tagID = tagID;

				lineID = newLine.lineID;
				lines.Add (newLine);
			}
			else if (!onlySeekNew && lineID > -1)
			{
				// Already has an ID, so don't replace
				string _scene = "";
				SpeechLine existingLine;
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				existingLine = new SpeechLine (lineID, _scene, speaker, messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
				existingLine.tagID = tagID;
				
				int _lineID = SmartAddLine (existingLine);
				if (_lineID >= 0) lineID = _lineID;
			}
			return lineID;
		}


		private void ExtractHotspotName (ActionRename action, bool onlySeekNew, bool isInScene)
		{
			if (action.newName != "")
			{
				string _scene = "";
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}

				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetIDArray(), _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (action.lineID, _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void ExtractCharacterName (ActionCharRename action, bool onlySeekNew, bool isInScene)
		{
			if (action.newName != "")
			{
				string _scene = "";
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetIDArray(), _scene, action.newName, languages.Count - 1, AC_TextType.Character);
					
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (action.lineID, _scene, action.newName, languages.Count - 1, AC_TextType.Character);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}

		
		private int SmartAddLine (SpeechLine existingLine)
		{
			if (DoLinesMatchID (existingLine.lineID))
			{
				// Same ID, different text, so re-assign ID
				int lineID = 0;
				
				foreach (int _id in GetIDArray (true))
				{
					if (lineID == _id)
						lineID ++;
				}
				
				ACDebug.LogWarning ("Conflicting ID number (" + existingLine.lineID + ") found with '"  + existingLine.text + "'. Changing to " + lineID + ".");
				existingLine.lineID = lineID;
				lines.Add (existingLine);
				return lineID;
			}
			else
			{
				lines.Add (existingLine);
			}
			return -1;
		}
		
		
		private bool DoLinesMatchID (int newLineID)
		{
			if (lines == null || lines.Count == 0)
			{
				return false;
			}
			
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == newLineID)
				{
					return true;
				}
			}

			return false;
		}

		
		private void ExtractJournalEntry (ActionMenuState action, bool onlySeekNew, bool isInScene)
		{
			if (action.changeType == ActionMenuState.MenuChangeType.AddJournalPage && action.journalText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					if (isInScene)
					{
						newLine = new SpeechLine (GetIDArray(), UnityVersionHandler.GetCurrentSceneName (), action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						newLine = new SpeechLine (GetIDArray(), "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine;
					if (isInScene)
					{
						existingLine = new SpeechLine (action.lineID, UnityVersionHandler.GetCurrentSceneName (), action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						existingLine = new SpeechLine (action.lineID, "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void GetLinesFromSettings (bool onlySeekNew)
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			if (settingsManager)
			{
				if (settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					foreach (PlayerPrefab playerPrefab in settingsManager.players)
					{
						if (playerPrefab != null && playerPrefab.playerOb != null)
						{
							ExtractCharacter (playerPrefab.playerOb, onlySeekNew);
							EditorUtility.SetDirty (playerPrefab.playerOb);
						}
					}
				}
				else if (settingsManager.player)
				{
					ExtractCharacter (settingsManager.player, onlySeekNew);
					EditorUtility.SetDirty (settingsManager.player);
				}
			}
		}
		
		
		private void GetLinesFromInventory (bool onlySeekNew)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				ProcessInventoryProperties (inventoryManager.items, inventoryManager.invVars, onlySeekNew);
				
				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						ExtractInventory (item, onlySeekNew);

						// Prefixes
						if (item.overrideUseSyntax)
						{
							ExtractPrefix (item.hotspotPrefix1, onlySeekNew);
							ExtractPrefix (item.hotspotPrefix2, onlySeekNew);
						}
					}
				}

				EditorUtility.SetDirty (inventoryManager);
			}
		}


		private void GetLinesFromVariables (bool readGlobal, bool onlySeekNew)
		{
			if (readGlobal)
			{
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
				if (variablesManager != null)
				{
					variablesManager.vars = ExtractVariables (variablesManager.vars, readGlobal, onlySeekNew);
					EditorUtility.SetDirty (variablesManager);
				}
			}
			else
			{
				LocalVariables localVariables = GameObject.FindObjectOfType (typeof (LocalVariables)) as LocalVariables;
				if (localVariables != null)
				{
					localVariables.localVars = ExtractVariables (localVariables.localVars, readGlobal, onlySeekNew);
					EditorUtility.SetDirty (localVariables);
				}
			}
		}


		private List<GVar> ExtractVariables (List<GVar> vars, bool readGlobal, bool onlySeekNew)
		{
			string sceneName = (readGlobal) ? "" : UnityVersionHandler.GetCurrentSceneName ();

			foreach (GVar var in vars)
			{
				if (var.type == VariableType.String)
				{
					if (onlySeekNew && var.textValLineID == -1)
					{
						// Assign a new ID on creation
						SpeechLine newLine = new SpeechLine (GetIDArray(), sceneName, var.textVal, languages.Count - 1, AC_TextType.Variable);
						
						var.textValLineID = newLine.lineID;
						lines.Add (newLine);
					}
					else if (!onlySeekNew && var.textValLineID > -1)
					{
						// Already has an ID, so don't replace
						SpeechLine existingLine = new SpeechLine (var.textValLineID, sceneName, var.textVal, languages.Count - 1, AC_TextType.Variable);
						
						int lineID = SmartAddLine (existingLine);
						if (lineID >= 0) var.textValLineID = lineID;
					}
				}
				else if (var.type == VariableType.PopUp)
				{
					if (onlySeekNew && var.popUpsLineID == -1)
					{
						// Assign a new ID on creation
						SpeechLine newLine = new SpeechLine (GetIDArray(), sceneName, var.GetPopUpsString (), languages.Count - 1, AC_TextType.Variable);
						
						var.popUpsLineID = newLine.lineID;
						lines.Add (newLine);
					}
					else if (!onlySeekNew && var.popUpsLineID > -1)
					{
						// Already has an ID, so don't replace
						SpeechLine existingLine = new SpeechLine (var.popUpsLineID, sceneName, var.GetPopUpsString (), languages.Count - 1, AC_TextType.Variable);
						
						int lineID = SmartAddLine (existingLine);
						if (lineID >= 0) var.popUpsLineID = lineID;
					}
				}
			}
			return vars;
		}
		
		
		private void GetLinesFromMenus (bool onlySeekNew)
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								ExtractElement (element, menuButton.label, onlySeekNew);
								ExtractHotspotOverride (menuButton, menuButton.hotspotLabel, onlySeekNew);
							}
							else if (element is MenuCycle)
							{
								MenuCycle menuCycle = (MenuCycle) element;
								ExtractElement (element, menuCycle.label, onlySeekNew);
							}
							else if (element is MenuDrag)
							{
								MenuDrag menuDrag = (MenuDrag) element;
								ExtractElement (element, menuDrag.label, onlySeekNew);
							}
							else if (element is MenuInput)
							{
								MenuInput menuInput = (MenuInput) element;
								ExtractElement (element, menuInput.label, onlySeekNew);
							}
							else if (element is MenuLabel)
							{
								MenuLabel menuLabel = (MenuLabel) element;
								if (menuLabel.CanTranslate ())
								{
									ExtractElement (element, menuLabel.label, onlySeekNew);
								}
								else
								{
									menuLabel.lineID = -1;
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								if (menuSavesList.saveListType == AC_SaveListType.Save && menuSavesList.showNewSaveOption)
								{
									ExtractElement (element, menuSavesList.newSaveText, onlySeekNew);
								}
							}
							else if (element is MenuSlider)
							{
								MenuSlider menuSlider = (MenuSlider) element;
								ExtractElement (element, menuSlider.label, onlySeekNew);
							}
							else if (element is MenuToggle)
							{
								MenuToggle menuToggle = (MenuToggle) element;
								ExtractElement (element, menuToggle.label, onlySeekNew);
							}
							else if (element is MenuJournal)
							{
								MenuJournal menuJournal = (MenuJournal) element;
								ExtractJournalElement (menuJournal, menuJournal.pages, onlySeekNew);
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}
		}
		
		
		private void GetLinesFromCursors (bool onlySeekNew)
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				ExtractPrefix (cursorManager.hotspotPrefix1, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix2, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix3, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix4, onlySeekNew);
				ExtractPrefix (cursorManager.walkPrefix, onlySeekNew);

				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						ExtractIcon (icon, onlySeekNew);
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void GetLinesInScene (string sceneFile, bool onlySeekNew)
		{
			UnityVersionHandler.OpenScene (sceneFile);

			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.InScene)
				{
					ProcessActionList (list, onlySeekNew);
				}
			}

			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				ExtractHotspot (hotspot, onlySeekNew);
				EditorUtility.SetDirty (hotspot);
			}

			// Characters
			AC.Char[] characters = GameObject.FindObjectsOfType (typeof (AC.Char)) as AC.Char[];
			foreach (AC.Char character in characters)
			{
				ExtractCharacter (character, onlySeekNew);
				EditorUtility.SetDirty (character);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				ExtractConversation (conversation, onlySeekNew);
				EditorUtility.SetDirty (conversation);
			}

			// Local variables
			GetLinesFromVariables (false, onlySeekNew);

			// Save the scene
			UnityVersionHandler.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private int[] GetIDArray (bool includeTempLines = false)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (SpeechLine line in lines)
			{
				idArray.Add (line.lineID);
			}

			foreach (SpeechLine tempLine in tempLines)
			{
				idArray.Add (tempLine.lineID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private void RestoreTranslations ()
		{
			// Match IDs for each entry in lines and tempLines, send over translation data
			foreach (SpeechLine tempLine in tempLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (tempLine.lineID == line.lineID)
					{
						line.translationText = tempLine.translationText;
						line.description = tempLine.description;
						line.customAudioClip = tempLine.customAudioClip;
						line.customLipsyncFile = tempLine.customLipsyncFile;
						line.customTranslationAudioClips = tempLine.customTranslationAudioClips;
						line.customTranslationLipsyncFiles = tempLine.customTranslationLipsyncFiles;
						break;
					}
				}
			}
			
			tempLines = null;
		}
		
		
		private void BackupTranslations ()
		{
			tempLines = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				tempLines.Add (line);
			}
		}


		private void ImportTranslation (int i)
		{
			string fileName = EditorUtility.OpenFilePanel ("Import " + languages[i] + " translation", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName, true);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
				
				int lineID = 0;
				string translationText = "";
				string owner = "";
				
				for (int y = 1; y < csvOutput.GetLength (1); y++)
				{
					if (csvOutput [0,y] != null && csvOutput [0,y].Length > 0)
					{
						lineID = -1;
						if (int.TryParse (csvOutput [0,y], out lineID))
						{
							translationText = csvOutput [3, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
							string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
							
							if (typeText.Contains ("JournalEntry (Page "))
							{
								owner = typeText.Replace ("JournalEntry (", "");
								owner = owner.Replace (")", "");
							}
							else
							{
								owner = "";
							}
							UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
						}
						else
						{
							ACDebug.LogWarning ("Error importing translation (ID:" + csvOutput [0,y] + ") - make sure that the CSV file is delimited by a '" + CSVReader.csvDelimiter + "' character.");
						}
					}
				}
				
				EditorUtility.SetDirty (this);
			}
			else
			{
				ACDebug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}
		
		
		private void UpdateTranslation (int i, int _lineID, string _owner, string translationText)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					line.translationText [i-1] = translationText;
				}
			}
		}
		
		
		private void ImportGameText ()
		{
			string fileName = EditorUtility.OpenFilePanel ("Import game text", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadSaveFile (fileName, true);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
				
				int lineID = 0;
				string translationText = "";
				string owner = "";
				
				for (int y = 1; y < csvOutput.GetLength (1); y++)
				{
					if (csvOutput [0,y] != null && csvOutput [0,y].Length > 0)
					{
						lineID = -1;
						if (int.TryParse (csvOutput [0,y], out lineID))
						{
							string typeText = csvOutput [1, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
							
							if (typeText.Contains ("JournalEntry (Page "))
							{
								owner = typeText.Replace ("JournalEntry (", "");
								owner = owner.Replace (")", "");
							}
							else
							{
								owner = "";
							}
							
							if (languages.Count > 1)
							{
								for (int i=1; i<languages.Count; i++)
								{
									translationText = csvOutput [i+2, y].Replace (CSVReader.csvTemp, CSVReader.csvComma);
									UpdateTranslation (i, lineID, owner, AddLineBreaks (translationText));
								}
							}
						}
						else
						{
							ACDebug.LogWarning ("Error importing translation (ID:" + csvOutput [0,y] + ") - make sure that the CSV file is delimited by a '" + CSVReader.csvDelimiter + "' character.");
						}
					}
				}
		
				EditorUtility.SetDirty (this);
			}
			else
			{
				ACDebug.LogWarning ("No CSV file found.  Looking for: " + fileName);
			}
		}
		
		
		private void ExportGameText ()
		{
			#if UNITY_WEBPLAYER
			ACDebug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else
			
			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "AllGameText.csv";
			
			string fileName = EditorUtility.SaveFilePanel ("Export game text", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			bool fail = false;
			List<string[]> output = new List<string[]>();
			
			List<string> headerList = new List<string>();
			headerList.AddRange (new [] { "ID", "Type", "Original text" });
			if (languages.Count > 1)
			{
				for (int i=1; i<languages.Count; i++)
				{
					headerList.Add (languages [i]);
				}
			}
			output.Add (headerList.ToArray ());
			
			foreach (SpeechLine line in lines)
			{
				List<string> rowList = new List<string>();
				rowList.AddRange (new [] { 
					line.lineID.ToString (), 
					line.GetInfo (),
					RemoveLineBreaks (line.text)
				});
				
				foreach (string translationText in line.translationText)
				{
					rowList.Add (RemoveLineBreaks (translationText));
					
					if (translationText.Contains (CSVReader.csvDelimiter))
					{
						fail = true;
						ACDebug.LogError ("Cannot export translation since line " + line.lineID.ToString () + "'s translation (" + translationText + ") contains the character '" + CSVReader.csvDelimiter + "'.");
					}
				}
				
				output.Add (rowList.ToArray ());
				
				if (line.textType != AC_TextType.JournalEntry && line.text.Contains (CSVReader.csvDelimiter))
				{
					fail = true;
					ACDebug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " (" + line.text + ") contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}
			
			if (!fail)
			{
				int length = output.Count;
				
				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
				}
				
				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}
			
			#endif
		}
		
		
		private void ExportTranslation (int i)
		{
			#if UNITY_WEBPLAYER
			ACDebug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			return;
			#else
			
			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += languages[i].ToString () + ".csv";
			
			string fileName = EditorUtility.SaveFilePanel ("Export " + languages[i] + " translation", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			bool fail = false;
			List<string[]> output = new List<string[]>();
			output.Add (new string[] {"ID", "Type", "Original line", languages[i] + " translation"});
			
			foreach (SpeechLine line in lines)
			{
				output.Add (new string[] 
				            {
					line.lineID.ToString (),
					line.GetInfo (),
					RemoveLineBreaks (line.text),
					RemoveLineBreaks (line.translationText [i-1])
				});
				
				if (line.textType != AC_TextType.JournalEntry && (line.text.Contains (CSVReader.csvDelimiter) || line.translationText [i-1].Contains (CSVReader.csvDelimiter)))
				{
					fail = true;
					ACDebug.LogError ("Cannot export translation since line " + line.lineID.ToString () + " contains the character '" + CSVReader.csvDelimiter + "'.");
				}
			}
			
			if (!fail)
			{
				int length = output.Count;
				
				StringBuilder sb = new StringBuilder();
				for (int j=0; j<length; j++)
				{
					string newLine = string.Join (CSVReader.csvDelimiter, output[j]);
					newLine = newLine.Replace ("\r", "");
					sb.AppendLine (newLine);
				}
				
				Serializer.CreateSaveFile (fileName, sb.ToString ());
			}
			
			#endif
		}
		

		private void ClearList ()
		{
			if (EditorUtility.DisplayDialog ("Reset all translation lines?", "This will completely reset the IDs of every text line in your game, removing any supplied translations and invalidating speech audio filenames. Continue?", "OK", "Cancel"))
			{
				string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
				
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					lines.Clear ();
					checkedAssets.Clear ();
					
					sceneFiles = AdvGame.GetSceneFiles ();
					GetSceneNames ();
					
					// First look for lines that already have an assigned lineID
					foreach (string sceneFile in sceneFiles)
					{
						ClearLinesInScene (sceneFile);
					}

					GetAllActionListAssets ();
					foreach (ActionListAsset actionListAsset in allActionListAssets)
					{
						ClearLinesFromActionListAsset (actionListAsset);
					}

					ClearLinesFromInventory ();
					ClearLinesFromCursors ();
					ClearLinesFromMenus ();
					
					checkedAssets.Clear ();

					if (originalScene == "")
					{
						UnityVersionHandler.NewScene ();
					}
					else
					{
						UnityVersionHandler.OpenScene (originalScene);
					}

					allActionListAssets.Clear ();
					ACDebug.Log ("Process complete.");
				}
			}
		}
		
		
		private void ClearLinesInScene (string sceneFile)
		{
			UnityVersionHandler.OpenScene (sceneFile);
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.InScene)
				{
					ClearLinesFromActionList (list);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				hotspot.lineID = -1;
				EditorUtility.SetDirty (hotspot);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					dialogOption.lineID = -1;
				}
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			UnityVersionHandler.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private void ClearLinesFromActionListAsset (ActionListAsset actionListAsset)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ClearLines (actionListAsset.actions);
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ClearLinesFromActionList (ActionList actionList)
		{
			if (actionList != null)
			{
				ClearLines (actionList.actions);
				EditorUtility.SetDirty (actionList);
			}
		}
		
		
		private void ClearLines (List<Action> actions)
		{
			if (actions == null)
			{
				return;
			}
			
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}

				if (action is ActionSpeech)
				{
					ActionSpeech actionSpeech = (ActionSpeech) action;
					actionSpeech.lineID = -1;
				}
				else if (action is ActionMenuState)
				{
					ActionMenuState actionMenuState = (ActionMenuState) action;
					actionMenuState.lineID = -1;
				}
			}
			
		}

		
		private void ClearLinesFromInventory ()
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager != null && inventoryManager.items.Count > 0)
			{
				foreach (InvItem item in inventoryManager.items)
				{
					// Label
					item.lineID = -1;
				}
			}
				
			EditorUtility.SetDirty (inventoryManager);
		}
		
		
		private void ClearLinesFromCursors ()
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				cursorManager.hotspotPrefix1.lineID = -1;
				cursorManager.hotspotPrefix2.lineID = -1;
				cursorManager.hotspotPrefix3.lineID = -1;
				cursorManager.hotspotPrefix4.lineID = -1;
				cursorManager.walkPrefix.lineID = -1;
				
				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						icon.lineID = -1;
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void ClearLinesFromMenus ()
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								menuButton.lineID = -1;
								menuButton.hotspotLabelID = -1;
							}
							else if (element is MenuCycle)
							{
								MenuCycle menuCycle = (MenuCycle) element;
								menuCycle.lineID = -1;
							}
							else if (element is MenuDrag)
							{
								MenuDrag menuDrag = (MenuDrag) element;
								menuDrag.lineID = -1;
							}
							else if (element is MenuInput)
							{
								MenuInput menuInput = (MenuInput) element;
								menuInput.lineID = -1;
							}
							else if (element is MenuLabel)
							{
								MenuLabel menuLabel = (MenuLabel) element;
								menuLabel.lineID = -1;
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								menuSavesList.lineID = -1;
							}
							else if (element is MenuSlider)
							{
								MenuSlider menuSlider = (MenuSlider) element;
								menuSlider.lineID = -1;
							}
							else if (element is MenuToggle)
							{
								MenuToggle menuToggle = (MenuToggle) element;
								menuToggle.lineID = -1;
							}
							else if (element is MenuJournal)
							{
								MenuJournal menuJournal = (MenuJournal) element;
								menuJournal.lineID = -1;
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}		
		}


		private void MergeMatchingSpeechIDs ()
		{
			if (lines == null || lines.Count == 0) return;

			List<SpeechLine> linesToCheck = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				if (line.textType == AC_TextType.Speech)
				{
					linesToCheck.Add (line);
				}
			}

			if (linesToCheck.Count <= 1) return;

			GetAllActionListAssets ();

			while (linesToCheck.Count > 0)
			{
				SpeechLine lineToCheck = linesToCheck[0];
				for (int i=1; i<linesToCheck.Count; i++)
				{
					if (linesToCheck[i].IsMatch (lineToCheck, true))
					{
						// Found a match
						SpeechLine matchingLine = linesToCheck[i];
						int originalID = matchingLine.lineID;
						int newID = lineToCheck.lineID;
						matchingLine.lineID = newID;

						if (matchingLine.customAudioClip != null && lineToCheck.customAudioClip == null)
						{
							lineToCheck.customAudioClip = matchingLine.customAudioClip;
							lineToCheck.customLipsyncFile = matchingLine.customLipsyncFile;
							lineToCheck.customTranslationAudioClips = matchingLine.customTranslationAudioClips;
							lineToCheck.customTranslationLipsyncFiles = matchingLine.customTranslationLipsyncFiles;
						}
						
						// Update ActionSpeech
						if (matchingLine.scene == "")
						{
							// In an asset
							foreach (ActionListAsset actionListAsset in allActionListAssets)
							{
								foreach (Action action in actionListAsset.actions)
								{
									if (action != null && action is ActionSpeech)
									{
										ActionSpeech actionSpeech = (ActionSpeech) action;
										if (actionSpeech.lineID == originalID)
										{
											actionSpeech.lineID = newID;
											EditorUtility.SetDirty (actionListAsset);
										}
									}
								}
							}
						}
						else
						{
							// In a scene
							foreach (string sceneFile in sceneFiles)
							{
								if (sceneFile.Contains (matchingLine.scene))
								{
									UnityVersionHandler.OpenScene (sceneFile);
									ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
									foreach (ActionList actionList in actionLists)
									{
										if (actionList.source == ActionListSource.InScene)
										{
											foreach (Action action in actionList.actions)
											{
												if (action != null && action is ActionSpeech)
												{
													ActionSpeech actionSpeech = (ActionSpeech) action;
													if (actionSpeech.lineID == originalID)
													{
														actionSpeech.lineID = newID;
														EditorUtility.SetDirty (actionList);
													}
												}
											}
										}
									}

									UnityVersionHandler.SaveScene ();
								}
							}
						}

						lines.Remove (matchingLine);
					}

				}

				linesToCheck.RemoveAt (0);
			}

			allActionListAssets.Clear ();
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
		}


		private void GetAllActionListAssets ()
		{
			allActionListAssets = new List<ActionListAsset>();

			// Search scenes
			foreach (string sceneFile in sceneFiles)
			{
				UnityVersionHandler.OpenScene (sceneFile);

				// ActionLists
				ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
				foreach (ActionList actionList in actionLists)
				{
					if (actionList.source == ActionListSource.AssetFile)
					{
						SmartAddAsset (actionList.assetFile);
					}
					else
					{
						GetActionListAssetsFromActions (actionList.actions);
					}
				}

				// Hotspots
				Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot.interactionSource == InteractionSource.AssetFile)
					{
						SmartAddAsset (hotspot.useButton.assetFile);
						SmartAddAsset (hotspot.lookButton.assetFile);
						SmartAddAsset (hotspot.unhandledInvButton.assetFile);

						foreach (Button _button in hotspot.useButtons)
						{
							SmartAddAsset (_button.assetFile);
						}
						
						foreach (Button _button in hotspot.invButtons)
						{
							SmartAddAsset (_button.assetFile);
						}
					}
				}
				
				// Dialogue options
				Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
				foreach (Conversation conversation in conversations)
				{
					foreach (ButtonDialog dialogOption in conversation.options)
					{
						SmartAddAsset (dialogOption.assetFile);
					}
					EditorUtility.SetDirty (conversation);
				}
			}

			// Settings Manager
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager)
			{
				SmartAddAsset (settingsManager.actionListOnStart);
				if (settingsManager.activeInputs != null)
				{
					foreach (ActiveInput activeInput in settingsManager.activeInputs)
					{
						SmartAddAsset (activeInput.actionListAsset);
					}
				}
			}

			// Inventory Manager
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			if (inventoryManager)
			{
				SmartAddAsset (inventoryManager.unhandledCombine);
				SmartAddAsset (inventoryManager.unhandledHotspot);
				SmartAddAsset (inventoryManager.unhandledGive);

				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						SmartAddAsset (item.useActionList);
						SmartAddAsset (item.lookActionList);
						SmartAddAsset (item.unhandledActionList);
						SmartAddAsset (item.unhandledCombineActionList);

						foreach (InvInteraction invInteraction in item.interactions)
						{
							SmartAddAsset (invInteraction.actionList);
						}

						foreach (ActionListAsset actionList in item.combineActionList)
						{
							SmartAddAsset (actionList);
						}
					}
				}
				foreach (Recipe recipe in inventoryManager.recipes)
				{
					SmartAddAsset (recipe.invActionList);
				}
			}

			// Cursor Manager
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			if (cursorManager)
			{
				foreach (ActionListAsset actionListAsset in cursorManager.unhandledCursorInteractions)
				{
					SmartAddAsset (actionListAsset);
				}
			}

			// Menu Manager
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			if (menuManager)
			{
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						SmartAddAsset (menu.actionListOnTurnOn);
						SmartAddAsset (menu.actionListOnTurnOff);

						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								if (menuButton.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									SmartAddAsset (menuButton.actionList);
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								SmartAddAsset (menuSavesList.actionListOnSave);
							}
						}
					}
				}
			}
		}


		private void SmartAddAsset (ActionListAsset asset)
		{
			if (asset != null)
			{
				if (allActionListAssets.Contains (asset))
				{
					return;
				}

				allActionListAssets.Add (asset);
				GetActionListAssetsFromActions (asset.actions);
			}
		}


		private void GetActionListAssetsFromActions (List<Action> actions)
		{
			if (actions != null)
			{
				foreach (Action action in actions)
				{
					if (action is ActionRunActionList)
					{
						ActionRunActionList runActionList = (ActionRunActionList) action;
						if (runActionList.listSource == ActionRunActionList.ListSource.AssetFile)
						{
							SmartAddAsset (runActionList.invActionList);
						}

						if ((runActionList.actionList != null && runActionList.actionList.useParameters) ||
						    (runActionList.linkedAsset != null && runActionList.linkedAsset.useParameters))
						{
							if (runActionList.localParameters != null)
							{
								foreach (ActionParameter localParameter in runActionList.localParameters)
								{
									if (localParameter.parameterType == ParameterType.UnityObject)
									{
										if (localParameter.objectValue != null)
										{
											if (localParameter.objectValue is ActionListAsset)
											{
												ActionListAsset _actionListAsset = (ActionListAsset) localParameter.objectValue;
												SmartAddAsset (_actionListAsset);
											}
										}
									}
								}
							}
						}
					}
					
					if (action is ActionCheck)
					{
						ActionCheck actionCheck = (ActionCheck) action;
						if (actionCheck.resultActionTrue == ResultAction.RunCutscene)
						{
							SmartAddAsset (actionCheck.linkedAssetTrue);
						}
						if (actionCheck.resultActionFail == ResultAction.RunCutscene)
						{
							SmartAddAsset (actionCheck.linkedAssetFail);
						}
					}
					else if (action is ActionCheckMultiple)
					{
						ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
						foreach (ActionEnd ending in actionCheckMultiple.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								SmartAddAsset (ending.linkedAsset);
							}
						}
					}
					else if (action is ActionParallel)
					{
						ActionParallel actionParallel = (ActionParallel) action;
						foreach (ActionEnd ending in actionParallel.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								SmartAddAsset (ending.linkedAsset);
							}
						}
					}
					else
					{
						if (action != null && action.endAction == ResultAction.RunCutscene)
						{
							SmartAddAsset (action.linkedAsset);
						}
					}
				}
			}
		}


		private void ProcessInventoryProperties (List<InvItem> items, List<InvVar> vars, bool onlySeekNew)
		{
			foreach (InvItem item in items)
			{
				foreach (InvVar var in item.vars)
				{
					if (var.type == VariableType.String)
					{
						if (onlySeekNew && var.textValLineID == -1)
						{
							// Assign a new ID on creation
							SpeechLine newLine = new SpeechLine (GetIDArray(), "", var.textVal, languages.Count - 1, AC_TextType.InventoryItemProperty);
							
							var.textValLineID = newLine.lineID;
							lines.Add (newLine);
						}
						else if (!onlySeekNew && var.textValLineID > -1)
						{
							// Already has an ID, so don't replace
							SpeechLine existingLine = new SpeechLine (var.textValLineID, "", var.textVal, languages.Count - 1, AC_TextType.InventoryItemProperty);
							
							int lineID = SmartAddLine (existingLine);
							if (lineID >= 0) var.textValLineID = lineID;
						}
					}
				}
			}

			foreach (InvVar var in vars)
			{
				if (onlySeekNew && var.popUpsLineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetIDArray(), "", var.GetPopUpsString (), languages.Count - 1, AC_TextType.InventoryItemProperty);
					
					var.popUpsLineID = newLine.lineID;
					lines.Add (newLine);
				}
				else if (!onlySeekNew && var.popUpsLineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (var.popUpsLineID, "", var.GetPopUpsString (), languages.Count - 1, AC_TextType.InventoryItemProperty);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) var.popUpsLineID = lineID;
				}
			}
		}
		
		
		private void ProcessActionListAsset (ActionListAsset actionListAsset, bool onlySeekNew)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ProcessActions (actionListAsset.actions, onlySeekNew, false, actionListAsset.tagID);
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ProcessActionList (ActionList actionList, bool onlySeekNew)
		{
			if (actionList != null)
			{
				ProcessActions (actionList.actions, onlySeekNew, true, actionList.tagID);
				EditorUtility.SetDirty (actionList);
			}
			
		}
		
		
		private void ProcessActions (List<Action> actions, bool onlySeekNew, bool isInScene, int tagID)
		{
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}
				
				if (action is ActionSpeech)
				{
					ExtractSpeech (action as ActionSpeech, onlySeekNew, isInScene, tagID);
				}
				else if (action is ActionRename)
				{
					ExtractHotspotName (action as ActionRename, onlySeekNew, isInScene);
				}
				else if (action is ActionCharRename)
				{
					ExtractCharacterName (action as ActionCharRename, onlySeekNew, isInScene);
				}
				else if (action is ActionMenuState)
				{
					ExtractJournalEntry (action as ActionMenuState, onlySeekNew, isInScene);
				}
			}
		}


		/**
		 * <summary>Gets a defined SpeechTag.</summary>
		 * <param name = "ID">The ID number of the SpeechTag to get</param>
		 * <returns>The SpeechTag</summary>
		 */
		public SpeechTag GetSpeechTag (int ID)
		{
			foreach (SpeechTag speechTag in speechTags)
			{
				if (speechTag.ID == ID)
				{
					return speechTag;
				}
			}
			return null;
		}

		#endif
		

		/**
		 * <summary>Gets the audio filename of a SpeechLine.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <returns>The audio filename of the speech line</summary>
		 */
		public string GetLineFilename (int _lineID)
		{
			foreach (SpeechLine line in lines) {
				if (line.lineID == _lineID) {
					return line.GetFilename ();
				}
			}
			return "";
		}



		/**
		 * <summary>Gets a SpeechLine class, as generated by the Speech Manager.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <returns>The generated SpeechLine class</summary>
		 */
		public SpeechLine GetLine (int _lineID)
		{
			foreach (SpeechLine line in lines) {
				if (line.lineID == _lineID) {
					return line;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the custom AudioClip of a SpeechLine.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "_language">The ID number of the language</param>
		 * <returns>The custom AudioClip of the speech line</summary>
		 */
		public AudioClip GetLineCustomAudioClip (int _lineID, int _language = 0)
		{
			foreach (SpeechLine line in lines) {
				if (line.lineID == _lineID) {
					if (translateAudio && _language > 0) {
						if (line.customTranslationAudioClips != null && line.customTranslationAudioClips.Count > (_language - 1)) {
							return line.customTranslationAudioClips [_language - 1];
						}
					} else {
						return line.customAudioClip;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the custom Object of a SpeechLine's lipsync.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "_language">The ID number of the language</param>
		 * <returns>The custom Object of the SpeechLine's lipsync</summary>
		 */
		public UnityEngine.Object GetLineCustomLipsyncFile (int _lineID, int _language = 0)
		{
			foreach (SpeechLine line in lines) {
				if (line.lineID == _lineID) {
					if (translateAudio && _language > 0) {
						if (line.customTranslationLipsyncFiles != null && line.customTranslationLipsyncFiles.Count > (_language - 1)) {
							return line.customTranslationLipsyncFiles [_language - 1];
						}
					} else {
						return line.customLipsyncFile;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Checks if the current lipsyncing method relies on external text files for each line.</summary>
		 * <returns>True if the current lipsyncing method relies on external text files for each line.</returns>
		 */
		public bool UseFileBasedLipSyncing ()
		{
			if (lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadPapagayoFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.RogoLipSync) {
				return true;
			}
			return false;
		}
		
	}
	
}