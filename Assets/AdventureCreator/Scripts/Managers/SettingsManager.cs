/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2016
 *	
 *	"SettingsManager.cs"
 * 
 *	This script handles the "Settings" tab of the main wizard.
 *	It is used to define the player, and control methods of the game.
 * 
 */

using UnityEngine;
#if UNITY_5
using UnityEngine.Audio;
#endif
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Handles the "Settings" tab of the Game Editor window.
	 * Most game-wide settings, including those related to control, input and interactions, are stored here.
	 */
	[System.Serializable]
	public class SettingsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		#endif
		
		// Save settings

		/** The name to give save game files */
		public string saveFileName = "";			
		/** How the time of a save file should be displayed (None, DateOnly, TimeAndDate) */
		public SaveTimeDisplay saveTimeDisplay = SaveTimeDisplay.DateOnly;
		/** If True, then a screenshot of the game will be taken whenever the game is saved */
		public bool takeSaveScreenshots;
		/** If True, then multiple save profiles - each with its own save files and options data - can be created */
		public bool useProfiles = false;
		/** The maximum number of save files that can be created */
		public int maxSaves = 5;
		/** If True, then save files listed in MenuSaveList will be displayed in order of update time */
		public bool orderSavesByUpdateTime = false;
		/** If True, then the scene will reload when loading a saved game that takes place in the same scene that the player is already in */
		public bool reloadSceneWhenLoading = false;
		/** If True, then Json serialization will be used instead (Unity 5.3 and above only) */
		public bool useJsonSerialization = false;

		// Cutscene settings

		/** The ActionListAsset to run when the game begins */
		public ActionListAsset actionListOnStart;
		/** If True, then the game will turn black whenever the user triggers the "EndCutscene" input to skip a cutscene */
		public bool blackOutWhenSkipping = false;
		
		// Character settings

		/** The state of player-switching (Allow, DoNotAllow) */
		public PlayerSwitching playerSwitching = PlayerSwitching.DoNotAllow;
		/** The player prefab, if playerSwitching = PlayerSwitching.DoNotAllow */
		public Player player;
		/** All available player prefabs, if playerSwitching = PlayerSwitching.Allow */
		public List<PlayerPrefab> players = new List<PlayerPrefab>();
		/** If True, then all player prefabs will share the same inventory, if playerSwitching = PlayerSwitching.Allow */
		public bool shareInventory = false;
		
		// Interface settings

		/** How the player character is controlled (PointAndClick, Direct, FirstPerson, Drag, None, StraightToCursor) */
		public MovementMethod movementMethod = MovementMethod.PointAndClick;
		/** The main input method used to control the game with (MouseAndKeyboard, KeyboardOrController, TouchScreen) */
		public InputMethod inputMethod = InputMethod.MouseAndKeyboard;
		/** How Hotspots are interacted with (ContextSensitive, ChooseInteractionThenHotspot, ChooseHotspotThenInteraction) */
		public AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
		/** How Interactions are triggered, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction (ClickingMenu, CyclingCursorAndClickingHotspot, CyclingMenuAndClickingHotspot) */
		public SelectInteractions selectInteractions = SelectInteractions.ClickingMenu;
		/** The method to close Interaction menus, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction (ClickOffMenu, CursorLeavesMenu, CursorLeavesMenuOrHotspot) */
		public CancelInteractions cancelInteractions = CancelInteractions.CursorLeavesMenuOrHotspot;
		/** How Interaction menus are opened, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction (ClickOnHotspot, CursorOverHotspot) */
		public SeeInteractions seeInteractions = SeeInteractions.ClickOnHotspot;
		/** If True, then the player will stop when a Hotspot is clicked on, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction */
		public bool stopPlayerOnClickHotspot = false;
		/** If True, then inventory items will be included in Interaction menus / cursor cycles, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction */
		public bool cycleInventoryCursors = true;
		/** If True, then triggering an Interaction will cycle the cursor mode, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction */
		public bool autoCycleWhenInteract = false;
		/** If True, then the cursor will be locked in the centre of the screen when the game begins */
		public bool lockCursorOnStart = false;
		/** If True, then the cursor will be hidden whenever it is locked */
		public bool hideLockedCursor = false;
		/** If True, and the game is in first-person, then free-aiming will be disabled while a moveable object is dragged */
		public bool disableFreeAimWhenDragging = false;
		/** If True, then Conversation dialogue options can be triggered with the number keys */
		public bool runConversationsWithKeys = false;
		/** If True, then interactions can be triggered by releasing the mouse cursor over an icon, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction */
		public bool clickUpInteractions = false;

		// Inventory settings

		/** If True, then inventory items can be drag-dropped (i.e. used on Hotspots and other items with a single mouse button press */
		public bool inventoryDragDrop = false;
		/** The number of pixels the mouse must be dragged for the inventory drag-drop effect becomes active, if inventoryDragDrop = True */
		public float dragDropThreshold = 0;
		/** If True, then using an inventory item on itself will trigger its Examine interaction */
		public bool inventoryDropLook = false;
		/** How many interactions an inventory item can have (Single, Multiple) */
		public InventoryInteractions inventoryInteractions = InventoryInteractions.Single;
		/** If True, then left-clicking will de-select an inventory item */
		public bool inventoryDisableLeft = true;
		/** If True, then triggering an unhandled Inventory interaction will de-select an inventory item */
		public bool inventoryDisableUnhandled = true;
		/** If True, then an inventory item will show its "active" texture when the mouse hovers over it */
		public bool activeWhenHover = false;
		/** The effect to apply to an active inventory item's icon (None, Pulse, Simple) */
		public InventoryActiveEffect inventoryActiveEffect = InventoryActiveEffect.Simple;
		/** The speed at which to pulse the active inventory item's icon, if inventoryActiveEffect = InventoryActiveEffect.Pulse */
		public float inventoryPulseSpeed = 1f;
		/** If True, then the inventory item will show its active effect when hovering over a Hotspot that has no matching Interaction */
		public bool activeWhenUnhandled = true;
		/** If True, then inventory items can be re-ordered in a MenuInventoryBox by the player */
		public bool canReorderItems = false;
		/** How the currently-selected inventory item should be displayed in InventoryBox elements */
		public SelectInventoryDisplay selectInventoryDisplay = SelectInventoryDisplay.NoChange;
		/** What happens when right-clicking while an inventory item is selected (ExaminesItem, DeselectsItem) */
		public RightClickInventory rightClickInventory = RightClickInventory.DeselectsItem;
		/** If True, then invntory item combinations will also work in reverse */
		public bool reverseInventoryCombinations = false;
		/** If True, then the player can move while an inventory item is seleted */
		public bool canMoveWhenActive = true;
		/** If True, and inventoryInteraction = InventoryInteractions.Multiple, then the item will be selected (in "use" mode) if a particular Interaction is unhandled */
		public bool selectInvWithUnhandled = false;
		/** The ID number of the CursorIcon interaction that selects the inventory item (in "use" mode) when unhandled, if selectInvWithUnhandled = True */
		public int selectInvWithIconID = 0;
		/** If True, and inventoryInteraction = InventoryInteractions.Multiple, then the item will be selected (in "give" mode) if a particular Interaction is unhandled */
		public bool giveInvWithUnhandled = false;
		/** The ID number of the CursorIcon interaction that selects the inventory item (in "give" mode) when unhandled, if selectInvWithUnhandled = True */
		public int giveInvWithIconID = 0;
	
		// Movement settings

		/** A prefab to instantiate whenever the user clicks to move the player, if movementMethod = AC_MovementMethod.PointAndClick */
		public Transform clickPrefab;
		/** How much of the screen will be searched for a suitable NavMesh, if the user doesn't click directly on one (it movementMethod = AC_MovementMethod.PointAndClick)  */
		public float walkableClickRange = 0.5f;
		/** How the nearest NavMesh to a cursor click is found, in screen space, if the user doesn't click directly on one */
		public NavMeshSearchDirection navMeshSearchDirection = NavMeshSearchDirection.RadiallyOutwardsFromCursor;
		/** If True, and movementMethod = AC_MovementMethod.PointAndClick, then the user will have to double-click to move the player */
		public bool doubleClickMovement = false;
		/** If True, and movementMethod = AC_MovementMethod.Direct, then the magnitude of the input axis will affect the Player's speed */
		public bool magnitudeAffectsDirect = false;
		/** If True, and movementMethod = AC_MovementMethod.Direct, then the Player will turn instantly when moving during gameplay */
		public bool directTurnsInstantly = false;
		/** If True, and Interaction menus are used, movement will be prevented while they are on */
		public bool disableMovementWhenInterationMenusAreOpen = false;
		/** How the player moves, if movementMethod = AC_MovementMethod.Direct (RelativeToCamera, TankControls) */
		public DirectMovementType directMovementType = DirectMovementType.RelativeToCamera;
		/** How to limit the player's moement, if directMovementType = DirectMovementType.RelativeToCamera */
		public LimitDirectMovement limitDirectMovement = LimitDirectMovement.NoLimit;
		/** If True, then the player's position on screen will be accounted for, if directMovemetType = DirectMovementType.RelativeToCamera */
		public bool directMovementPerspective = false;
		/** How accurate characters will be when navigating to set points on a NavMesh */
		public float destinationAccuracy = 0.8f;
		/** If True, and destinationAccuracy = 1, then characters will lerp to their destination when very close, to ensure they end up at exactly the intended point */
		public bool experimentalAccuracy = false;

		/** If >0, the time (in seconds) between pathfinding recalculations occur */
		public float pathfindUpdateFrequency = 0f;
		/** How much slower vertical movement is compared to horizontal movement, if the game is in 2D */
		public float verticalReductionFactor = 0.7f;
		/** The player's jump speed */
		public float jumpSpeed = 4f;
		/** If True, then single-clicking also moves the player, if movementMethod = AC_MovementMethod.StraightToCursor */
		public bool singleTapStraight = false;
		/** If True, then single-clicking will make the player pathfind, if singleTapStraight = True */
		public bool singleTapStraightPathfind = false;

		// First-person settings

		/** If True, then first-person games will use the first-person camera during conversations */
		public bool useFPCamDuringConversations = true;
		/** If True, then Hotspot interactions are only allowed if the cursor is unlocked (first person-games only) */
		public bool onlyInteractWhenCursorUnlocked = false;

		// Input settings

		/** If True, then try/catch statements used when checking for input will be bypassed - this results in better performance, but all available inputs must be defined. */
		public bool assumeInputsDefined = false;
		/** A List of active inputs that trigger ActionLists when an Input button is pressed */
		public List<ActiveInput> activeInputs = new List<ActiveInput>();
	
		// Drag settings

		/** The free-look speed when rotating a first-person camera, if inputMethod = AC_InputMethod.TouchScreen */
		public float freeAimTouchSpeed = 0.01f;
		/** The minimum drag magnitude needed to move the player, if movementMethod = AC_MovementMethod.Drag */
		public float dragWalkThreshold = 5f;
		/** The minimum drag magnitude needed to make the player run, if movementMethod = AC_MovementMethod.Drag */
		public float dragRunThreshold = 20f;
		/** If True, then a drag line will be drawn on screen if movementMethod = AC_MovementMethod.Drag */
		public bool drawDragLine = false;
		/** The width of the drag line, if drawDragLine = True */
		public float dragLineWidth = 3f;
		/** The colour of the drag line, if drawDragLine = True */
		public Color dragLineColor = Color.white;
	
		// Touch Screen settings

		/** If True, then the cursor is not set to the touch point, but instead is moved by dragging (if inputMethod = AC_InputMethod.TouchScreen) */
		public bool offsetTouchCursor = false;
		/** If True, then Hotspots are activated by double-tapping (if inputMethod = AC_InputMethod.TouchScreen) */
		public bool doubleTapHotspots = true;
		/** How first-person movement should work when using touch-screen controls (OneTouchToMoveAndTurn, OneTouchToTurnAndTwoTouchesToMove) */
		public FirstPersonTouchScreen firstPersonTouchScreen = FirstPersonTouchScreen.OneTouchToMoveAndTurn;

		// Camera settings

		/** If True, then the game's aspect ratio will be fixed */
		public bool forceAspectRatio = false;
		/** The aspect ratio, as a decimal, to use if forceAspectRatio = True */
		public float wantedAspectRatio = 1.5f;
		/** If True, then the game can only be played in landscape mode (iPhone only) */
		public bool landscapeModeOnly = true;
		/** The game's camera perspective (TwoD, TwoPointFiveD, ThreeD) */
		public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;

		private int cameraPerspective_int;
		#if UNITY_EDITOR
		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		#endif

		/** The method of moving and turning in 2D games (Unity2D, TopDown, ScreenSpace, WorldSpace) */
		public MovingTurning movingTurning = MovingTurning.Unity2D;

		// Hotspot settings

		/** How Hotspots are detected (MouseOver, PlayerVicinity) */
		public HotspotDetection hotspotDetection = HotspotDetection.MouseOver;
		/** What Hotspots gets detected, if hotspotDetection = HotspotDetection.PlayerVicinity (NearestOnly, CycleMultiple, ShowAll) */
		public HotspotsInVicinity hotspotsInVicinity = HotspotsInVicinity.NearestOnly;
		/** When Hotspot icons are displayed (Never, Always, OnlyWhenHighlighting, OnlyWhenFlashing) */
		public HotspotIconDisplay hotspotIconDisplay = HotspotIconDisplay.Never;
		/** The type of Hotspot icon to display, if hotspotIconDisplay != HotspotIconDisplay.Never (Texture, UseIcon) */
		public HotspotIcon hotspotIcon;
		/** The texture to use for Hotspot icons, if hotspotIcon = HotspotIcon.Texture */
		public Texture2D hotspotIconTexture = null;
		/** The size of Hotspot icons */
		public float hotspotIconSize = 0.04f;
		/** If True, then 3D player prefabs will turn their head towards the active Hotspot */
		public bool playerFacesHotspots = false;
		/** If true, and playerFacesHotspots = True, and interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction, then players will only turn their heads once a Hotspot has been selected */
		public bool onlyFaceHotspotOnSelect = false;
		/** If True, then Hotspots will highlight according to how close the cursor is to them */
		public bool scaleHighlightWithMouseProximity = false;
		/** The factor by which distance affects the highlighting of Hotspots, if scaleHighlightWithMouseProximity = True */
		public float highlightProximityFactor = 4f;
		/** If True, then Hotspot icons will be hidden behind colldiers placed on hotspotLayer */
		public bool occludeIcons = false;
		/** If True, then Hotspot icons will be hidden if an Interaction Menu is visible */
		public bool hideIconUnderInteractionMenu = false;
		/** How to draw Hotspot icons (ScreenSpace, WorldSpace) */
		public ScreenWorld hotspotDrawing = ScreenWorld.ScreenSpace;
		
		// Raycast settings

		/** The length of rays cast to find NavMeshes */
		public float navMeshRaycastLength = 100f;
		/** The length of rays cast to find Hotspots */
		public float hotspotRaycastLength = 100f;
		/** The length of rays cast to find moveable objects (see DragBase) */
		public float moveableRaycastLength = 30f;
		
		// Layer names

		/** The layer to place active Hotspots on */
		public string hotspotLayer = "Default";
		/** The layer to place active NavMeshes on */
		public string navMeshLayer = "NavMesh";
		/** The layer to place BackgroundImage prefabs on */
		public string backgroundImageLayer = "BackgroundImage";
		/** The layer to place deactivated objects on */
		public string deactivatedLayer = "Ignore Raycast";
		
		// Loading screen

		/** If True, then a specific scene will be loaded in-between scene transitions, to be used as a loading screen */
		public bool useLoadingScreen = false;
		/** How the scene that acts as a loading scene is chosen (Number, Name) */
		public ChooseSceneBy loadingSceneIs = ChooseSceneBy.Number;
		/** The name of the scene to act as a loading scene, if loadingScene = ChooseSceneBy.Name */
		public string loadingSceneName = "";
		/** The number of the scene to act as a loading scene, if loadingScene = ChooseSceneBy.Number */
		public int loadingScene = 0;
		/** If True, scenes will be loaded asynchronously */
		public bool useAsyncLoading = false;
		/** The delay, in seconds, before and after loading, if both useLoadingScreen = True and useAsyncLoading = True */
		public float loadingDelay = 0f;

		// Sound settings

		/** If True, then music can play when the game is paused */
		public bool playMusicWhilePaused = false;
		/** A list of all AudioClips that can be played as music using the "Sound: Play Music" Action */
		public List<MusicStorage> musicStorages = new List<MusicStorage>();
		#if UNITY_5
		/** How volume is controlled (AudioSources, AudioMixerGroups) (Unity 5 only) */
		public VolumeControl volumeControl = VolumeControl.AudioSources;
		/** The AudioMixerGroup for music audio, if volumeControl = VolumeControl.AudioSources */
		public AudioMixerGroup musicMixerGroup = null;
		/** The AudioMixerGroup for SF audio, if volumeControl = VolumeControl.AudioSources */
		public AudioMixerGroup sfxMixerGroup = null;
		/** The AudioMixerGroup for speech audio, if volumeControl = VolumeControl.AudioSources */
		public AudioMixerGroup speechMixerGroup = null;
		/** The name of the parameter in musicMixerGroup that controls attenuation */
		public string musicAttentuationParameter = "musicVolume";
		/** The name of the parameter in sfxMixerGroup that controls attenuation */
		public string sfxAttentuationParameter = "sfxVolume";
		/** The name of the parameter in speechMixerGroup that controls attenuation */
		public string speechAttentuationParameter = "speechVolume";
		#endif

		// Options data

		/** The game's default language index */
		public int defaultLanguage = 0;
		/** The game's default subtitles state */
		public bool defaultShowSubtitles = false;
		/** The game's default SFX audio volume */
		public float defaultSfxVolume = 0.9f;
		/** The game's default music audio volume */
		public float defaultMusicVolume = 0.6f;
		/** The game's default speech audio volume */
		public float defaultSpeechVolume = 1f;

		/** Determines when logs are written to the Console (Always, OnlyInEditor, Never) */
		public ShowDebugLogs showDebugLogs = ShowDebugLogs.Always;

		#if UNITY_EDITOR
		private OptionsData optionsData = new OptionsData ();

		// Debug

		/** If True, then all currently-running ActionLists will be listed in the corner of the screen */
		public bool showActiveActionLists = false;
		/** If True, then icons can be displayed in the Hierarchy window */
		public bool showHierarchyIcons = true;


		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			EditorGUILayout.LabelField ("Save game settings", EditorStyles.boldLabel);
			
			if (saveFileName == "")
			{
				saveFileName = SaveSystem.SetProjectName ();
			}
			maxSaves = EditorGUILayout.IntField ("Max. number of saves:", maxSaves);
			saveFileName = EditorGUILayout.TextField ("Save filename:", saveFileName);
			if (saveFileName != "")
			{
				if (saveFileName.Contains (" "))
				{
					EditorGUILayout.HelpBox ("The save filename cannot contain 'space' characters - please remove them to prevent file-handling issues.", MessageType.Warning);
				}
				else
				{
					#if !(UNITY_WP8 || UNITY_WINRT)
					string newSaveFileName = System.Text.RegularExpressions.Regex.Replace (saveFileName, "[^\\w\\._]", "");
					if (saveFileName != newSaveFileName)
					{
						EditorGUILayout.HelpBox ("The save filename contains special characters - please remove them to prevent file-handling issues.", MessageType.Warning);
					}
					#endif
				}
			}

			useProfiles = EditorGUILayout.ToggleLeft ("Enable save game profiles?", useProfiles);
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT && !UNITY_WII
			saveTimeDisplay = (SaveTimeDisplay) EditorGUILayout.EnumPopup ("Time display:", saveTimeDisplay);
			takeSaveScreenshots = EditorGUILayout.ToggleLeft ("Take screenshot when saving?", takeSaveScreenshots);
			orderSavesByUpdateTime = EditorGUILayout.ToggleLeft ("Order save lists by update time?", orderSavesByUpdateTime);
			#else
			EditorGUILayout.HelpBox ("Save-game screenshots are disabled for WebPlayer, Windows Store and Android platforms.", MessageType.Info);
			takeSaveScreenshots = false;
			#endif

			if (UnityVersionHandler.CanUseJson ())
			{
				useJsonSerialization = EditorGUILayout.ToggleLeft ("Save in Json format? (Experiemental)", useJsonSerialization);
			}

			if (GUILayout.Button ("Auto-add save components to GameObjects"))
			{
				AssignSaveScripts ();
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Cutscene settings:", EditorStyles.boldLabel);
			
			actionListOnStart = ActionListAssetMenu.AssetGUI ("ActionList on start game:", actionListOnStart);
			blackOutWhenSkipping = EditorGUILayout.Toggle ("Black out when skipping?", blackOutWhenSkipping);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Character settings:", EditorStyles.boldLabel);
			
			CreatePlayersGUI ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Interface settings", EditorStyles.boldLabel);
			
			movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
			inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
			interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);
			
			//if (inputMethod != InputMethod.TouchScreen)
			if (CanUseCursor ())
			{
				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					selectInteractions = (SelectInteractions) EditorGUILayout.EnumPopup ("Select Interactions by:", selectInteractions);
					if (selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						seeInteractions = (SeeInteractions) EditorGUILayout.EnumPopup ("See Interactions with:", seeInteractions);
						if (seeInteractions == SeeInteractions.ClickOnHotspot)
						{
							stopPlayerOnClickHotspot = EditorGUILayout.ToggleLeft ("Stop player moving when click Hotspot?", stopPlayerOnClickHotspot);
						}
					}

					if (selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						autoCycleWhenInteract = EditorGUILayout.ToggleLeft ("Auto-cycle after an Interaction?", autoCycleWhenInteract);
					}
				
					if (SelectInteractionMethod () == SelectInteractions.ClickingMenu)
					{
						clickUpInteractions = EditorGUILayout.ToggleLeft ("Trigger interaction by releasing click?", clickUpInteractions);
						cancelInteractions = (CancelInteractions) EditorGUILayout.EnumPopup ("Close interactions with:", cancelInteractions);
					}
					else
					{
						cancelInteractions = CancelInteractions.CursorLeavesMenu;
					}
				}
			}
			if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				autoCycleWhenInteract = EditorGUILayout.ToggleLeft ("Reset cursor after an Interaction?", autoCycleWhenInteract);
			}

			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen)
			{
				// First person dragging only works if cursor is unlocked
				lockCursorOnStart = false;
			}
			else
			{
				lockCursorOnStart = EditorGUILayout.ToggleLeft ("Lock cursor in screen's centre when game begins?", lockCursorOnStart);
				hideLockedCursor = EditorGUILayout.ToggleLeft ("Hide cursor when locked in screen's centre?", hideLockedCursor);
				if (movementMethod == MovementMethod.FirstPerson)
				{
					onlyInteractWhenCursorUnlocked = EditorGUILayout.ToggleLeft ("Disallow Interactions if cursor is locked?", onlyInteractWhenCursorUnlocked);
				}
			}
			if (IsInFirstPerson ())
			{
				disableFreeAimWhenDragging = EditorGUILayout.ToggleLeft ("Disable free-aim when moving Draggables and PickUps?", disableFreeAimWhenDragging);

				if (movementMethod == MovementMethod.FirstPerson)
				{
					useFPCamDuringConversations = EditorGUILayout.ToggleLeft ("Run Conversations in first-person?", useFPCamDuringConversations);
				}
			}
			if (inputMethod != InputMethod.TouchScreen)
			{
				runConversationsWithKeys = EditorGUILayout.ToggleLeft ("Dialogue options can be selected with number keys?", runConversationsWithKeys);
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Inventory settings", EditorStyles.boldLabel);

			if (interactionMethod != AC_InteractionMethod.ContextSensitive)
			{
				inventoryInteractions = (InventoryInteractions) EditorGUILayout.EnumPopup ("Inventory interactions:", inventoryInteractions);

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						cycleInventoryCursors = EditorGUILayout.ToggleLeft ("Include Inventory items in Hotspot Interaction cycles?", cycleInventoryCursors);
					}
					else
					{
						cycleInventoryCursors = EditorGUILayout.ToggleLeft ("Include Inventory items in Hotspot Interaction menus?", cycleInventoryCursors);
					}
				}

				if (inventoryInteractions == InventoryInteractions.Multiple && CanSelectItems (false))
				{
					selectInvWithUnhandled = EditorGUILayout.ToggleLeft ("Select item if Interaction is unhandled?", selectInvWithUnhandled);
					if (selectInvWithUnhandled)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						if (cursorManager != null && cursorManager.cursorIcons != null && cursorManager.cursorIcons.Count > 0)
						{
							selectInvWithIconID = GetIconID ("Select with unhandled:", selectInvWithIconID, cursorManager);
						}
						else
						{
							EditorGUILayout.HelpBox ("No Interaction cursors defined - please do so in the Cursor Manager.", MessageType.Info);
						}
					}

					giveInvWithUnhandled = EditorGUILayout.ToggleLeft ("Give item if Interaction is unhandled?", giveInvWithUnhandled);
					if (giveInvWithUnhandled)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						if (cursorManager != null && cursorManager.cursorIcons != null && cursorManager.cursorIcons.Count > 0)
						{
							giveInvWithIconID = GetIconID ("Give with unhandled:", giveInvWithIconID, cursorManager);
						}
						else
						{
							EditorGUILayout.HelpBox ("No Interaction cursors defined - please do so in the Cursor Manager.", MessageType.Info);
						}
					}
				}
			}

			if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && selectInteractions != SelectInteractions.ClickingMenu && inventoryInteractions == InventoryInteractions.Multiple)
			{}
			else
			{
				reverseInventoryCombinations = EditorGUILayout.ToggleLeft ("Combine interactions work in reverse?", reverseInventoryCombinations);
			}

			if (CanSelectItems (false))
			{
				inventoryDragDrop = EditorGUILayout.ToggleLeft ("Drag and drop Inventory interface?", inventoryDragDrop);
				if (!inventoryDragDrop)
				{
					if (interactionMethod == AC_InteractionMethod.ContextSensitive || inventoryInteractions == InventoryInteractions.Single)
					{
						rightClickInventory = (RightClickInventory) EditorGUILayout.EnumPopup ("Right-click active item:", rightClickInventory);
					}
				}
				else
				{
					dragDropThreshold = EditorGUILayout.Slider ("Minimum drag distance:", dragDropThreshold, 0f, 20f);
					if (inventoryInteractions == AC.InventoryInteractions.Single)
					{
						if (dragDropThreshold == 0f)
						{
							inventoryDropLook = EditorGUILayout.ToggleLeft ("Can drop an Item onto itself to Examine it?", inventoryDropLook);
						}
						else
						{
							inventoryDropLook = EditorGUILayout.ToggleLeft ("Clicking an Item without dragging Examines it?", inventoryDropLook);
						}
					}
				}
			}

			if (CanSelectItems (false) && !inventoryDragDrop)
			{
				inventoryDisableUnhandled = EditorGUILayout.ToggleLeft ("Unhandled interactions deselect active item?", inventoryDisableUnhandled);
				inventoryDisableLeft = EditorGUILayout.ToggleLeft ("Left-click deselects active item?", inventoryDisableLeft);

				if (movementMethod == MovementMethod.PointAndClick && !inventoryDisableLeft)
				{
					canMoveWhenActive = EditorGUILayout.ToggleLeft ("Can move player if an Item is active?", canMoveWhenActive);
				}
			}

			inventoryActiveEffect = (InventoryActiveEffect) EditorGUILayout.EnumPopup ("Active cursor FX:", inventoryActiveEffect);
			if (inventoryActiveEffect == InventoryActiveEffect.Pulse)
			{
				inventoryPulseSpeed = EditorGUILayout.Slider ("Active FX pulse speed:", inventoryPulseSpeed, 0.5f, 2f);
			}

			activeWhenUnhandled = EditorGUILayout.ToggleLeft ("Show Active FX when an Interaction is unhandled?", activeWhenUnhandled);
			canReorderItems = EditorGUILayout.ToggleLeft ("Items can be re-ordered in Menu?", canReorderItems);
			selectInventoryDisplay = (SelectInventoryDisplay) EditorGUILayout.EnumPopup ("Seleted item's display:", selectInventoryDisplay);
			activeWhenHover = EditorGUILayout.ToggleLeft ("Show Active FX when Cursor hovers over Item in Menu?", activeWhenHover);

			EditorGUILayout.Space ();
			if (assumeInputsDefined)
			{
				EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
			}
			else
			{
				EditorGUILayout.LabelField ("Available inputs:", EditorStyles.boldLabel);
			}
			EditorGUILayout.HelpBox ("The following inputs are available for the chosen interface settings:" + GetInputList (), MessageType.Info);
			assumeInputsDefined = EditorGUILayout.ToggleLeft ("Assume inputs are defined?", assumeInputsDefined);
			if (assumeInputsDefined)
			{
				EditorGUILayout.HelpBox ("Try/catch statements used when checking for input will be bypassed - this results in better performance, but all available inputs must be defined. Delegates in PlayerInput.cs will also be ignored", MessageType.Warning);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Movement settings", EditorStyles.boldLabel);
			
			if ((inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.PointAndClick) || movementMethod == MovementMethod.Drag)
			{
				dragWalkThreshold = EditorGUILayout.FloatField ("Walk threshold:", dragWalkThreshold);
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				
				if (inputMethod == InputMethod.TouchScreen && movementMethod == MovementMethod.FirstPerson)
				{
					freeAimTouchSpeed = EditorGUILayout.FloatField ("Freelook speed:", freeAimTouchSpeed);
				}
				
				drawDragLine = EditorGUILayout.Toggle ("Draw drag line?", drawDragLine);
				if (drawDragLine)
				{
					dragLineWidth = EditorGUILayout.FloatField ("Drag line width:", dragLineWidth);
					dragLineColor = EditorGUILayout.ColorField ("Drag line colour:", dragLineColor);
				}
			}
			else if (movementMethod == MovementMethod.Direct)
			{
				magnitudeAffectsDirect = EditorGUILayout.ToggleLeft ("Input magnitude affects speed?", magnitudeAffectsDirect);
				directTurnsInstantly = EditorGUILayout.ToggleLeft ("Turn instantly when under player control?", directTurnsInstantly);
				directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Direct-movement type:", directMovementType);
				if (directMovementType == DirectMovementType.RelativeToCamera)
				{
					limitDirectMovement = (LimitDirectMovement) EditorGUILayout.EnumPopup ("Movement limitation:", limitDirectMovement);
					if (cameraPerspective == CameraPerspective.ThreeD)
					{
						directMovementPerspective = EditorGUILayout.ToggleLeft ("Account for player's position on screen?", directMovementPerspective);
					}
				}
			}
			else if (movementMethod == MovementMethod.PointAndClick)
			{
				clickPrefab = (Transform) EditorGUILayout.ObjectField ("Click marker:", clickPrefab, typeof (Transform), false);
				walkableClickRange = EditorGUILayout.Slider ("NavMesh search %:", walkableClickRange, 0f, 1f);
				if (walkableClickRange > 0f)
				{
					navMeshSearchDirection = (NavMeshSearchDirection) EditorGUILayout.EnumPopup ("NavMesh search direction:", navMeshSearchDirection);
				}
				doubleClickMovement = EditorGUILayout.Toggle ("Double-click to move?", doubleClickMovement);
			}
			else if (movementMethod == MovementMethod.FirstPerson)
			{
				directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Turning type:", directMovementType);
			}

			if (movementMethod != MovementMethod.PointAndClick &&
				interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction &&
				selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				disableMovementWhenInterationMenusAreOpen = EditorGUILayout.Toggle ("Disable movement when Interaction menus are on?", disableMovementWhenInterationMenusAreOpen);
			}
			if (movementMethod == MovementMethod.StraightToCursor)
			{
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				singleTapStraight = EditorGUILayout.ToggleLeft ("Single-clicking also moves player?", singleTapStraight);
				if (singleTapStraight)
				{
					singleTapStraightPathfind = EditorGUILayout.ToggleLeft ("Pathfind when single-clicking?", singleTapStraightPathfind);
				}
			}
			if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson) && inputMethod != InputMethod.TouchScreen)
			{
				jumpSpeed = EditorGUILayout.Slider ("Jump speed:", jumpSpeed, 1f, 10f);
			}
			
			destinationAccuracy = EditorGUILayout.Slider ("Destination accuracy:", destinationAccuracy, 0f, 1f);
			if (destinationAccuracy == 1f)
			{
				experimentalAccuracy = EditorGUILayout.ToggleLeft ("Attempt to be super-accurate? (Experimental)", experimentalAccuracy);
			}
			pathfindUpdateFrequency = EditorGUILayout.Slider ("Pathfinding update time (s)", pathfindUpdateFrequency, 0f, 5f);

			if (inputMethod == InputMethod.TouchScreen)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Touch Screen settings", EditorStyles.boldLabel);

				if (movementMethod != MovementMethod.FirstPerson)
				{
					offsetTouchCursor = EditorGUILayout.Toggle ("Drag cursor with touch?", offsetTouchCursor);
				}
				else
				{
					firstPersonTouchScreen = (FirstPersonTouchScreen) EditorGUILayout.EnumPopup ("First person movement:", firstPersonTouchScreen);
				}
				doubleTapHotspots = EditorGUILayout.Toggle ("Double-tap Hotspots?", doubleTapHotspots);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
			
			cameraPerspective_int = (int) cameraPerspective;
			cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
			cameraPerspective = (CameraPerspective) cameraPerspective_int;
			if (movementMethod == MovementMethod.FirstPerson)
			{
				cameraPerspective = CameraPerspective.ThreeD;
			}
			if (cameraPerspective == CameraPerspective.TwoD)
			{
				movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
				if (movingTurning == MovingTurning.TopDown || movingTurning == MovingTurning.Unity2D)
				{
					verticalReductionFactor = EditorGUILayout.Slider ("Vertical movement factor:", verticalReductionFactor, 0.1f, 1f);
				}
			}
			
			forceAspectRatio = EditorGUILayout.Toggle ("Force aspect ratio?", forceAspectRatio);
			if (forceAspectRatio)
			{
				wantedAspectRatio = EditorGUILayout.FloatField ("Aspect ratio:", wantedAspectRatio);
				#if UNITY_IPHONE
				landscapeModeOnly = EditorGUILayout.Toggle ("Landscape-mode only?", landscapeModeOnly);
				#endif
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Hotspot settings", EditorStyles.boldLabel);
			
			hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);
			if (hotspotDetection == HotspotDetection.PlayerVicinity && (movementMethod == MovementMethod.Direct || IsInFirstPerson ()))
			{
				hotspotsInVicinity = (HotspotsInVicinity) EditorGUILayout.EnumPopup ("Hotspots in vicinity:", hotspotsInVicinity);
			}
			else if (hotspotDetection == HotspotDetection.MouseOver)
			{
				scaleHighlightWithMouseProximity = EditorGUILayout.ToggleLeft ("Highlight Hotspots based on cursor proximity?", scaleHighlightWithMouseProximity);
				if (scaleHighlightWithMouseProximity)
				{
					highlightProximityFactor = EditorGUILayout.FloatField ("Cursor proximity factor:", highlightProximityFactor);
				}
			}
			
			if (cameraPerspective != CameraPerspective.TwoD)
			{
				playerFacesHotspots = EditorGUILayout.ToggleLeft ("Player turns head to active Hotspot?", playerFacesHotspots);
				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && playerFacesHotspots)
				{
					onlyFaceHotspotOnSelect = EditorGUILayout.ToggleLeft ("Only turn head when select Hotspot?", onlyFaceHotspotOnSelect);
				}
			}
			
			hotspotIconDisplay = (HotspotIconDisplay) EditorGUILayout.EnumPopup ("Display Hotspot icons:", hotspotIconDisplay);
			if (hotspotIconDisplay != HotspotIconDisplay.Never)
			{
				hotspotDrawing = (ScreenWorld) EditorGUILayout.EnumPopup ("Draw icons in:", hotspotDrawing);
				if (cameraPerspective != CameraPerspective.TwoD)
				{
					occludeIcons = EditorGUILayout.ToggleLeft ("Don't show behind Colliders?", occludeIcons);
				}
				hotspotIcon = (HotspotIcon) EditorGUILayout.EnumPopup ("Hotspot icon type:", hotspotIcon);
				if (hotspotIcon == HotspotIcon.Texture)
				{
					hotspotIconTexture = (Texture2D) EditorGUILayout.ObjectField ("Hotspot icon texture:", hotspotIconTexture, typeof (Texture2D), false);
				}
				hotspotIconSize = EditorGUILayout.FloatField ("Hotspot icon size:", hotspotIconSize);
				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction &&
				    selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot &&
				    hotspotIconDisplay != HotspotIconDisplay.OnlyWhenFlashing)
				{
					hideIconUnderInteractionMenu = EditorGUILayout.ToggleLeft ("Hide when Interaction Menus are visible?", hideIconUnderInteractionMenu);
				}
			}

			#if UNITY_5
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Audio settings", EditorStyles.boldLabel);
			volumeControl = (VolumeControl) EditorGUILayout.EnumPopup ("Volume controlled by:", volumeControl);
			if (volumeControl == VolumeControl.AudioMixerGroups)
			{
				musicMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("Music mixer:", musicMixerGroup, typeof (AudioMixerGroup), false);
				sfxMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("SFX mixer:", sfxMixerGroup, typeof (AudioMixerGroup), false);
				speechMixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField ("Speech mixer:", speechMixerGroup, typeof (AudioMixerGroup), false);
				musicAttentuationParameter = EditorGUILayout.TextField ("Music atten. parameter:", musicAttentuationParameter);
				sfxAttentuationParameter = EditorGUILayout.TextField ("SFX atten. parameter:", sfxAttentuationParameter);
				speechAttentuationParameter = EditorGUILayout.TextField ("Speech atten. parameter:", speechAttentuationParameter);
			}
			#endif

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Raycast settings", EditorStyles.boldLabel);
			navMeshRaycastLength = EditorGUILayout.FloatField ("NavMesh ray length:", navMeshRaycastLength);
			hotspotRaycastLength = EditorGUILayout.FloatField ("Hotspot ray length:", hotspotRaycastLength);
			moveableRaycastLength = EditorGUILayout.FloatField ("Moveable ray length:", moveableRaycastLength);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Layer names", EditorStyles.boldLabel);
			
			hotspotLayer = EditorGUILayout.TextField ("Hotspot:", hotspotLayer);
			navMeshLayer = EditorGUILayout.TextField ("Nav mesh:", navMeshLayer);
			if (cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				backgroundImageLayer = EditorGUILayout.TextField ("Background image:", backgroundImageLayer);
			}
			deactivatedLayer = EditorGUILayout.TextField ("Deactivated:", deactivatedLayer);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Scene loading", EditorStyles.boldLabel);
			reloadSceneWhenLoading = EditorGUILayout.ToggleLeft ("Always reload scene when loading a save file?", reloadSceneWhenLoading);
			useAsyncLoading = EditorGUILayout.ToggleLeft ("Load scenes asynchronously?", useAsyncLoading);
			useLoadingScreen = EditorGUILayout.ToggleLeft ("Use loading screen?", useLoadingScreen);
			if (useLoadingScreen)
			{
				loadingSceneIs = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose loading scene by:", loadingSceneIs);
				if (loadingSceneIs == ChooseSceneBy.Name)
				{
					loadingSceneName = EditorGUILayout.TextField ("Loading scene name:", loadingSceneName);
				}
				else
				{
					loadingScene = EditorGUILayout.IntField ("Loading screen scene:", loadingScene);
				}
				if (useAsyncLoading)
				{
					loadingDelay = EditorGUILayout.Slider ("Delay before and after (s):", loadingDelay, 0f, 1f);
				}
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Options data", EditorStyles.boldLabel);

			optionsData = Options.LoadPrefsFromID (0, false, true);
			if (optionsData == null)
			{
				ACDebug.Log ("Saved new prefs");
				Options.SaveDefaultPrefs (optionsData);
			}

			defaultSpeechVolume = optionsData.speechVolume = EditorGUILayout.Slider ("Speech volume:", optionsData.speechVolume, 0f, 1f);
			defaultMusicVolume = optionsData.musicVolume = EditorGUILayout.Slider ("Music volume:", optionsData.musicVolume, 0f, 1f);
			defaultSfxVolume = optionsData.sfxVolume = EditorGUILayout.Slider ("SFX volume:", optionsData.sfxVolume, 0f, 1f);
			defaultShowSubtitles = optionsData.showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", optionsData.showSubtitles);
			defaultLanguage = optionsData.language = EditorGUILayout.IntField ("Language:", optionsData.language);

			Options.SaveDefaultPrefs (optionsData);

			if (GUILayout.Button ("Reset options data"))
			{
				optionsData = new OptionsData ();

				optionsData.language = 0;
				optionsData.speechVolume = 1f;
				optionsData.musicVolume = 0.6f;
				optionsData.sfxVolume = 0.9f;
				optionsData.showSubtitles = false;

				Options.SavePrefsToID (0, optionsData, true);
			}

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Debug settings", EditorStyles.boldLabel);
			showActiveActionLists = EditorGUILayout.ToggleLeft ("List active ActionLists in Game window?", showActiveActionLists);
			showHierarchyIcons = EditorGUILayout.ToggleLeft ("Show icons in Hierarchy window?", showHierarchyIcons);
			showDebugLogs = (ShowDebugLogs) EditorGUILayout.EnumPopup ("Show logs in Console:", showDebugLogs);
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		#endif


		private string SmartAddInput (string existingResult, string newInput)
		{
			if (!existingResult.Contains (newInput))
			{
				return existingResult + "\n" + newInput;
			}
			return existingResult;
		}
		
		
		private string GetInputList ()
		{
			string result = "";
			
			if (inputMethod != InputMethod.TouchScreen)
			{
				result = SmartAddInput (result, "InteractionA (Button)");
				result = SmartAddInput (result, "InteractionB (Button)");
				result = SmartAddInput (result, "CursorHorizontal (Axis)");
				result = SmartAddInput (result, "CursorVertical (Axis)");
			}
			
			if (movementMethod != MovementMethod.PointAndClick && movementMethod != MovementMethod.StraightToCursor)
			{
				result = SmartAddInput (result, "ToggleCursor (Button)");
			}
			
			if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson || inputMethod == InputMethod.KeyboardOrController)
			{
				if (inputMethod != InputMethod.TouchScreen)
				{
					result = SmartAddInput (result, "Horizontal (Axis)");
					result = SmartAddInput (result, "Vertical (Axis)");

					if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
					{
						result = SmartAddInput (result, "Run (Button/Axis)");
						result = SmartAddInput (result, "ToggleRun (Button)");
						result = SmartAddInput (result, "Jump (Button)");
					}
				}
				
				if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.MouseAndKeyboard)
				{
					result = SmartAddInput (result, "MouseScrollWheel (Axis)");
					result = SmartAddInput (result, "CursorHorizontal (Axis)");
					result = SmartAddInput (result, "CursorVertical (Axis)");
				}
				
				if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
				    && (hotspotDetection == HotspotDetection.PlayerVicinity && hotspotsInVicinity == HotspotsInVicinity.CycleMultiple))
				{
					result = SmartAddInput (result, "CycleHotspotsLeft (Button)");
					result = SmartAddInput (result, "CycleHotspotsRight (Button)");
					result = SmartAddInput (result, "CycleHotspots (Axis)");
				}
			}
			
			if (SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				result = SmartAddInput (result, "CycleInteractionsLeft (Button)");
				result = SmartAddInput (result, "CycleInteractionsRight (Button)");
				result = SmartAddInput (result, "CycleInteractions (Axis)");
			}
			if (SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				result = SmartAddInput (result, "CycleCursors (Button)");
				result = SmartAddInput (result, "CycleCursorsBack (Button)");
			}
			else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				result = SmartAddInput (result, "CycleCursors (Button)");
			}

			result = SmartAddInput (result, "FlashHotspots (Button)");
			if (AdvGame.GetReferences ().speechManager != null && AdvGame.GetReferences ().speechManager.allowSpeechSkipping)
			{
				result = SmartAddInput (result, "SkipSpeech (Button)");
			}
			result = SmartAddInput (result, "EndCutscene (Button)");
			result = SmartAddInput (result, "ThrowMoveable (Button)");
			result = SmartAddInput (result, "RotateMoveable (Button)");
			result = SmartAddInput (result, "RotateMoveableToggle (Button)");
			result = SmartAddInput (result, "ZoomMoveable (Axis)");

			if (AdvGame.GetReferences ().menuManager != null && AdvGame.GetReferences ().menuManager.menus != null)
			{
				foreach (Menu menu in AdvGame.GetReferences ().menuManager.menus)
				{
					if (menu.appearType == AppearType.OnInputKey && menu.toggleKey != "")
					{
						result = SmartAddInput (result, menu.toggleKey + " (Button)");
					}
				}
			}

			if (activeInputs != null)
			{
				foreach (ActiveInput activeInput in activeInputs)
				{
					if (activeInput.inputName != "")
					{
						result = SmartAddInput (result, activeInput.inputName + " (Button)");
					}
				}
			}

			if (runConversationsWithKeys)
			{
				result = SmartAddInput (result, "DialogueOption[1-9] (Buttons)");
			}

			return result;
		}
		

		/**
		 * <summary>Checks if the game is in 2D, and plays in screen-space (i.e. characters do not move towards or away from the camera).</summary>
		 * <returns>True if the game is in 2D, and plays in screen-space</returns>
		 */
		public bool ActInScreenSpace ()
		{
			if ((movingTurning == MovingTurning.ScreenSpace || movingTurning == MovingTurning.Unity2D) && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the game uses Unity 2D for its camera perspective.<summary>
		 * <returns>True if the game uses Unity 2D for its camera perspective</returns>
		 */
		public bool IsUnity2D ()
		{
			if (movingTurning == MovingTurning.Unity2D && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the game uses Top Down for its camera perspective.<summary>
		 * <returns>True if the game uses Top Down for its camera perspective</returns>
		 */
		public bool IsTopDown ()
		{
			if (movingTurning == MovingTurning.TopDown && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the game is in first-person, on touch screen, and dragging affects only the camera rotation.</summary>
		 * <returns>True if the game is in first-person, on touch screen, and dragging affects only the camera rotation.</returns>
		 */
		public bool IsFirstPersonDragRotation ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.TouchControlsTurningOnly)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Checks if the game is in first-person, on touch screen, and dragging one finger affects camera rotation, and two fingers affects player movement.</summary>
		 * <returns>True if the game is in first-person, on touch screen, and dragging one finger affects camera rotation, and two fingers affects player movement.</returns>
		 */
		public bool IsFirstPersonDragComplex ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToTurnAndTwoTouchesToMove)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the game is in first-person, on touch screen, and dragging affects player movement and camera rotation.</summary>
		 * <returns>True if the game is in first-person, on touch screen, and dragging affects player movement and camera rotation.</returns>
		 */
		public bool IsFirstPersonDragMovement ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToMoveAndTurn)
			{
				return true;
			}
			return false;
		}
		
		
		
		#if UNITY_EDITOR
		
		private void CreatePlayersGUI ()
		{
			playerSwitching = (PlayerSwitching) EditorGUILayout.EnumPopup ("Player switching:", playerSwitching);
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				player = (Player) EditorGUILayout.ObjectField ("Player:", player, typeof (Player), false);
			}
			else
			{
				shareInventory = EditorGUILayout.Toggle ("Share same Inventory?", shareInventory);
				
				foreach (PlayerPrefab _player in players)
				{
					EditorGUILayout.BeginHorizontal ();
					
					_player.playerOb = (Player) EditorGUILayout.ObjectField ("Player " + _player.ID + ":", _player.playerOb, typeof (Player), false);
					
					if (_player.isDefault)
					{
						GUILayout.Label ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (80f));
					}
					else
					{
						if (GUILayout.Button ("Make default", GUILayout.Width (80f)))
						{
							SetDefaultPlayer (_player);
						}
					}
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete player reference");
						players.Remove (_player);
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Add new player"))
				{
					Undo.RecordObject (this, "Add player");
					
					PlayerPrefab newPlayer = new PlayerPrefab (GetPlayerIDArray ());
					players.Add (newPlayer);
				}
			}
		}


		private int GetIconID (string label, int iconID, CursorManager cursorManager)
		{
			int iconInt = cursorManager.GetIntFromID (iconID);
			iconInt = EditorGUILayout.Popup (label, iconInt, cursorManager.GetLabelsArray ());
			iconID = cursorManager.cursorIcons[iconInt].id;
			return iconID;
		}

		#endif
		
		
		private int[] GetPlayerIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (PlayerPrefab player in players)
			{
				idArray.Add (player.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		

		/**
		 * <summary>Gets the ID number of the default Player prefab.</summary>
		 * <returns>The ID number of the default Player prefab</returns>
		 */
		public int GetDefaultPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}


		/**
		 * <summary>Gets a Player prefab with a given ID number.</summary>
		 * <param name = "ID">The ID number of the Player prefab to return</param>
		 * <returns>The Player prefab with the given ID number.</returns>
		 */
		public Player GetPlayer (int ID)
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.ID == ID)
				{
					return _player.playerOb;
				}
			}
			
			return null;
		}


		/**
		 * <summary>Gets the ID number of the first-assigned Player prefab.</summary>
		 * <returns>The ID number of the first-assigned Player prefab</returns>
		 */
		public int GetEmptyPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb == null)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}


		/**
		 * <summary>Gets the default Player prefab.</summary>
		 * <returns>The default player Player prefab</returns>
		 */
		public Player GetDefaultPlayer ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					if (_player.playerOb != null)
					{
						return _player.playerOb;
					}
					
					ACDebug.LogWarning ("Default Player has no prefab!");
					return null;
				}
			}
			
			ACDebug.LogWarning ("Cannot find default player!");
			return null;
		}
		
		
		private void SetDefaultPlayer (PlayerPrefab defaultPlayer)
		{
			foreach (PlayerPrefab _player in players)
			{
				if (_player == defaultPlayer)
				{
					_player.isDefault = true;
				}
				else
				{
					_player.isDefault = false;
				}
			}
		}
		

		/**
		 * <summary>Checks if the player can click off Interaction menus to disable them.</summary>
		 * <returns>True if the player can click off Interaction menus to disable them.</returns>
		 */
		public bool CanClickOffInteractionMenu ()
		{
			if (cancelInteractions == CancelInteractions.ClickOffMenu || !CanUseCursor ())
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the player brings up the Interaction Menu by hovering the mouse over a Hotspot.</summary>
		 * <returns>True if the player brings up the Interaction Menu by hovering the mouse over a Hotspot.</returns>
		 */
		public bool MouseOverForInteractionMenu ()
		{
			if (seeInteractions == SeeInteractions.CursorOverHotspot && CanUseCursor ())
			{
				return true;
			}
			return false;
		}


		private bool CanUseCursor ()
		{
			if (inputMethod != InputMethod.TouchScreen || CanDragCursor ())
			{
				return true;
			}
			return false;
		}
		

		private bool DoPlayerAnimEnginesMatch ()
		{
			AnimationEngine animationEngine = AnimationEngine.Legacy;
			bool foundFirst = false;
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb != null)
				{
					if (!foundFirst)
					{
						foundFirst = true;
						animationEngine = _player.playerOb.animationEngine;
					}
					else
					{
						if (_player.playerOb.animationEngine != animationEngine)
						{
							return false;
						}
					}
				}
			}
			
			return true;
		}
		

		/**
		 * <summary>Gets the method of selecting Interactions, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction.</summary>
		 * <returns>The method of selecting Interactions, if interactionMethod = AC_InteractionMethod.ChooseHotspotThenInteraction.</returns>
		 */
		public SelectInteractions SelectInteractionMethod ()
		{
			if (inputMethod != InputMethod.TouchScreen && interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				return selectInteractions;
			}
			return SelectInteractions.ClickingMenu;
		}
		

		/**
		 * <summary>Checks if the game is currently in a "loading" scene.<summary>
		 * <returns>True it the game is currently in a "loading" scene</returns>
		 */
		public bool IsInLoadingScene ()
		{
			if (useLoadingScreen)
			{
				if (loadingSceneIs == ChooseSceneBy.Name)
				{
					if (UnityVersionHandler.GetCurrentSceneName () != "" && UnityVersionHandler.GetCurrentSceneName () == loadingSceneName)
					{
						return true;
					}
				}
				else if (loadingSceneIs == ChooseSceneBy.Number)
				{
					if (UnityVersionHandler.GetCurrentSceneName () != "" && UnityVersionHandler.GetCurrentSceneNumber () == loadingScene)
					{
						return true;
					}
				}
			}
			return false;
		}
		
		
		/**
		 * <summary>Checks if the game is played in first-person.</summary>
		 * <returns>True if the game is played in first-person</returns>
		 */
		public bool IsInFirstPerson ()
		{
			if (movementMethod == MovementMethod.FirstPerson)
			{
				return true;
			}
			if (KickStarter.player != null && KickStarter.player.FirstPersonCamera != null)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Checks if the player is able to "give" inventory items to NPCs.</summary>
		 * <returns>True if the player is able to "give" inventory items to NPCs.</returns>
		 */
		public bool CanGiveItems ()
		{
			if (interactionMethod != AC_InteractionMethod.ContextSensitive && CanSelectItems (false))
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Checks if inventory items can be selected and then used on Hotspots or other items.</summary>
		 * <param name = "showError">If True, then a warning will be sent to the Console if this function returns False</param>
		 * <returns>Checks if inventory items can be selected and then used on Hotspots or other items</returns>
		 */
		public bool CanSelectItems (bool showError)
		{
			if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				return true;
			}
			if (!cycleInventoryCursors)
			{
				return true;
			}
			if (showError)
			{
				ACDebug.LogWarning ("Inventory items cannot be selected with this combination of settings - they are included in Interaction cycles instead.");
			}
			return false;
		}


		/**
		 * <summary>Checks if the cursor can be dragged on a touch-screen.</summary>
		 * <returns>True if the cursor can be dragged on a touch-screen</returns>
		 */
		public bool CanDragCursor ()
		{
			if (offsetTouchCursor && inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.FirstPerson)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Checks if Interactions are triggered by "clicking up" over a MenuInteraction element.</summary>
		 * <returns>True if Interactions are triggered by "clicking up" over a MenuInteraction element</returns>
		 */
		public bool ReleaseClickInteractions ()
		{
			if (inputMethod == InputMethod.TouchScreen)
			{
				return true;
			}

			if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction &&
				SelectInteractionMethod () == SelectInteractions.ClickingMenu &&
			    clickUpInteractions)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Gets the minimum distance that a character can be to its target to be considered "close enough".</summary>
		 * <param name = "offset">The calculation is 1 + offset - destinationAccuracy, so having a non-zero offset prevents the result ever being zero.</param>
		 * <returns>The minimum distance that a character can be to its target to be considered "close enough".</returns>
		 */
		public float GetDestinationThreshold (float offset = 0.1f)
		{
			return (1f + offset - destinationAccuracy);
		}


		#if UNITY_EDITOR

		private void AssignSaveScripts ()
		{
			bool canProceed = EditorUtility.DisplayDialog ("Add save scripts", "AC will now go through your game, and attempt to add 'Remember' components where appropriate.\n\nThese components are required for saving to function, and are covered in Section 9.2 of the Manual.\n\nAs this process cannot be undone without manually removing each script, it is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			string originalScene = UnityVersionHandler.GetCurrentSceneName ();

			if (UnityVersionHandler.SaveSceneIfUserWants ())
			{
				Undo.RecordObject (this, "Update speech list");
				
				string[] sceneFiles = AdvGame.GetSceneFiles ();

				// First look for lines that already have an assigned lineID
				foreach (string sceneFile in sceneFiles)
				{
					AssignSaveScriptsInScene (sceneFile);
				}

				AssignSaveScriptsInManagers ();

				if (originalScene == "")
				{
					UnityVersionHandler.NewScene ();
				}
				else
				{
					UnityVersionHandler.OpenScene (originalScene);
				}

				ACDebug.Log ("Process complete.");
			}
		}


		private void AssignSaveScriptsInScene (string sceneFile)
		{
			UnityVersionHandler.OpenScene (sceneFile);
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.AssetFile)
				{
					SaveActionListAsset (list.assetFile);
				}
				else
				{
					SaveActionList (list);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot.interactionSource == InteractionSource.AssetFile)
				{
					SaveActionListAsset (hotspot.useButton.assetFile);
					SaveActionListAsset (hotspot.lookButton.assetFile);
					SaveActionListAsset (hotspot.unhandledInvButton.assetFile);
					
					foreach (Button _button in hotspot.useButtons)
					{
						SaveActionListAsset (_button.assetFile);
					}
					
					foreach (Button _button in hotspot.invButtons)
					{
						SaveActionListAsset (_button.assetFile);
					}
				}
			}

			// Triggers
			AC_Trigger[] triggers = GameObject.FindObjectsOfType (typeof (AC_Trigger)) as AC_Trigger[];
			foreach (AC_Trigger trigger in triggers)
			{
				if (trigger.GetComponent <RememberTrigger>() == null)
				{
					RememberTrigger rememberTrigger = trigger.gameObject.AddComponent <RememberTrigger>();
					foreach (ConstantID constantIDScript in trigger.GetComponents <ConstantID>())
					{
						if (!(constantIDScript is Remember) && constantIDScript != rememberTrigger)
						{
							DestroyImmediate (constantIDScript);
						}
					}
				}
			}

			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					SaveActionListAsset (dialogOption.assetFile);
				}
			}
			
			// Save the scene
			UnityVersionHandler.SaveScene ();
			EditorUtility.SetDirty (this);
		}


		private void AssignSaveScriptsInManagers ()
		{
			// Settings
			SaveActionListAsset (actionListOnStart);
			if (activeInputs != null)
			{
				foreach (ActiveInput activeInput in activeInputs)
				{
					SaveActionListAsset (activeInput.actionListAsset);
				}
			}

			// Inventory
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			if (inventoryManager)
			{
				SaveActionListAsset (inventoryManager.unhandledCombine);
				SaveActionListAsset (inventoryManager.unhandledHotspot);
				SaveActionListAsset (inventoryManager.unhandledGive);

				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						SaveActionListAsset (item.useActionList);
						SaveActionListAsset (item.lookActionList);
						SaveActionListAsset (item.unhandledActionList);
						SaveActionListAsset (item.unhandledCombineActionList);

						foreach (ActionListAsset actionList in item.combineActionList)
						{
							SaveActionListAsset (actionList);
						}
					}
				}
				
				foreach (Recipe recipe in inventoryManager.recipes)
				{
					SaveActionListAsset (recipe.invActionList);
				}
			}

			// Cursor
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			if (cursorManager)
			{
				foreach (ActionListAsset actionListAsset in cursorManager.unhandledCursorInteractions)
				{
					SaveActionListAsset (actionListAsset);
				}
			}

			// Menu
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						SaveActionListAsset (menu.actionListOnTurnOff);
						SaveActionListAsset (menu.actionListOnTurnOn);
						
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								if (menuButton.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									SaveActionListAsset (menuButton.actionList);
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								SaveActionListAsset (menuSavesList.actionListOnSave);
							}
						}
					}
				}
			}
		}


		private void SaveActionListAsset (ActionListAsset actionListAsset)
		{
			if (actionListAsset != null)
			{
				SaveActions (actionListAsset.actions);
			}
		}
		
		
		private void SaveActionList (ActionList actionList)
		{
			if (actionList != null)
			{
				SaveActions (actionList.actions);
			}
			
		}
		
		
		private void SaveActions (List<Action> actions)
		{
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}

				action.AssignConstantIDs (true);

				if (action is ActionCheck)
				{
					ActionCheck actionCheck = (ActionCheck) action;
					if (actionCheck.resultActionTrue == ResultAction.RunCutscene)
					{
						SaveActionListAsset (actionCheck.linkedAssetTrue);
					}
					if (actionCheck.resultActionFail == ResultAction.RunCutscene)
					{
						SaveActionListAsset (actionCheck.linkedAssetFail);
					}
				}
				else if (action is ActionCheckMultiple)
				{
					ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
					foreach (ActionEnd ending in actionCheckMultiple.endings)
					{
						if (ending.resultAction == ResultAction.RunCutscene)
						{
							SaveActionListAsset (ending.linkedAsset);
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
							SaveActionListAsset (ending.linkedAsset);
						}
					}
				}
				else
				{
					if (action.endAction == ResultAction.RunCutscene)
					{
						SaveActionListAsset (action.linkedAsset);
					}
				}
			}
		}

		#endif

	}


	/**
	 * A data container for active inputs, which map ActionListAssets to input buttons.
	 */
	[System.Serializable]
	public class ActiveInput
	{

		/** The name of the Input button, as defined in the Input Manager */
		public string inputName;
		/** What state the game must be in for the actionListAsset to run (Normal, Cutscene, Paused, DialogOptions) */
		public GameState gameState;
		/** The ActionListAsset to run when the input button is pressed */
		public ActionListAsset actionListAsset;


		/**
		 * The default Constructor.
		 */
		public ActiveInput ()
		{
			inputName = "";
			gameState = GameState.Normal;
			actionListAsset = null;
		}

	}


	/**
	 * \mainpage Adventure Creator: Scripting guide
	 *
	 * Welcome to Adventure Creator's scripting guide!
	 * You can use this guide to get detailed descriptions on all of ACs public functions and variables.
	 * 
	 * Adventure Creator's scripts are written in C#, and use the 'AC' namespace, so you'll need to add the following at the top of any script that accesses them:
	 * 
	 * \code
	 * using AC;
	 * \endcode
	 * 
	 * Accessing ACs scripts is very simple: each component on the GameEngine and PersistentEngine prefabs, as well as all Managers, can be accessed by referencing their associated static variable in the KickStarter class, e.g.:
	 * 
	 * \code
	 * AC.KickStarter.settingsManager;
	 * AC.KickStarter.playerInput;
	 * \endcode
	 * 
	 * Additionally, the Player and MainCamera can also be accessed in this way:
	 * 
	 * \code
	 * AC.KickStarter.player;
	 * AC.KickStarter.mainCamera;
	 * \endcode
	 * 
	 * The KickStarter class also has functions that can be used to turn AC off or on completely:
	 * 
	 * \code
	 * AC.KickStarter.TurnOff ();
	 * AC.KickStarter.TurnOn ();
	 * \endcode
	 * 
	 * The variable that determines AC's main state is the gameState, inside StateHandler. Reading this variable can determine if the game is currently in a cutscene or not.
	 * 
	 * \code
	 * AC.KickStarter.stateHandler.gameState;
	 * \endcode
	 * 
	 * If you want to place the game in a scripted cutscene, the StateHandler has functions for that, too:
	 * 
	 * \code
	 * AC.KickStarter.stateHandler.StartCutscene ();
	 * AC.KickStarter.stateHandler.EndCutscene ();
	 * \endcode
	 * 
	 * The StateHandler is important because it updates all of AC's scene objects. It does this by taking a record of all the objects in the scene. Therefore, if you add or remove objects from a scene mid-game through script, you will need to update the StateHandler's internal list of objects to update.  You can do this by calling:
	 * 
	 * \code
	 * AC.KickStarter.stateHandler.GatherObjects ();
	 * \endcode
	 * 
	 * All-scene based ActionLists, inculding Cutscenes and Triggers, derive from the ActionList class.
	 * 
	 * You can run ActionListAsset assets from the AdvGame class, which contains a number of helpful general functions.
	 * 
	 * Global and Local variables can be read and written to with static functions in the GlobalVariables and LocalVariables classes respectively:
	 * 
	 * \code
	 * AC.GlobalVariables.GetBooleanValue (int _id);
	 * AC.LocalVariables.SetStringValue (int _id, string _value);
	 * \endcode
	 * 
	 * More common functions and variables can be found under Section 12.7 of the <a href="http://www.adventurecreator.org/files/Manual.pdf">AC Manual</a>.  Happy scripting!
	 */
	
}