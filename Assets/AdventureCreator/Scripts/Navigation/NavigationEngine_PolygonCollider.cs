/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2016
 *	
 *	"NavigationEngine_PolygonCollider.cs"
 * 
 *	This script uses a Polygon Collider 2D to
 *	allow pathfinding in a scene. Since v1.37,
 *	it uses the Dijkstra algorithm, as found on
 *	http://rosettacode.org/wiki/Dijkstra%27s_algorithm
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class NavigationEngine_PolygonCollider : NavigationEngine
	{
		
		public static Collider2D[] results = new Collider2D[1];

		private int MAXNODES = 1000;
		private float[,] cachedGraph;
		private float searchRadius = 0.02f;


		public override void OnReset (NavigationMesh navMesh)
		{
			is2D = true;
			ResetHoles (navMesh);
		}


		public override void TurnOn (NavigationMesh navMesh)
		{
			if (navMesh == null) return;

			if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
			{
				ACDebug.LogError ("Can't find layer " + KickStarter.settingsManager.navMeshLayer + " - please define it in Unity's Tags Manager (Edit -> Project settings -> Tags and Layers).");
			}
			else if (KickStarter.settingsManager.navMeshLayer != "")
			{
				navMesh.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
			}
			
			if (navMesh.GetComponent <Collider2D>() == null)
			{
				ACDebug.LogWarning ("A 2D Collider component must be attached to " + navMesh.gameObject.name + " for pathfinding to work - please attach one.");
			}
		}


		public override Vector3[] GetPointsArray (Vector3 _originPos, Vector3 _targetPos, AC.Char _char = null)
		{
			PolygonCollider2D poly = KickStarter.sceneSettings.navMesh.transform.GetComponent <PolygonCollider2D>();
			CalcSearchRadius (KickStarter.sceneSettings.navMesh);

			AddCharHoles (poly, _char, KickStarter.sceneSettings.navMesh.characterEvasion);

			List<Vector3> pointsList3D = new List<Vector3> ();
			if (IsLineClear (_originPos, _targetPos))
			{
				pointsList3D.Add (_targetPos);
				return pointsList3D.ToArray ();
			}
			
			Vector2[] pointsList = vertexData;

			Vector2 originPos = GetNearestToMesh (_originPos, poly);
			Vector2 targetPos = GetNearestToMesh (_targetPos, poly);

			pointsList = AddEndsToList (pointsList, originPos, targetPos);

			bool useCache = (KickStarter.sceneSettings.navMesh.characterEvasion == CharacterEvasion.None) ? true : false;
			float[,] weight = pointsToWeight (pointsList, useCache);
			
			int[] precede = buildSpanningTree (0, 1, weight);
			if (precede == null)
			{
				ACDebug.LogWarning ("Pathfinding error");
				pointsList3D.Add (_targetPos);
				return pointsList3D.ToArray ();
			}
			int[] _path = getShortestPath (0, 1, precede);
			
			foreach (int i in _path)
			{
				Vector3 vertex = new Vector3 (pointsList[i].x, pointsList[i].y, _originPos.z);
				pointsList3D.Insert (0, vertex);
			}
			
			if (pointsList3D[0] == _originPos || (pointsList3D[0].x == originPos.x && pointsList3D[0].y == originPos.y))
			{
				pointsList3D.RemoveAt (0);	// Remove origin point from start
			}

			return pointsList3D.ToArray ();
		}


		public override void ResetHoles (NavigationMesh navMesh)
		{
			ResetHoles (navMesh, true);
		}


		private void ResetHoles (NavigationMesh navMesh, bool rebuild)
		{
			if (navMesh == null || navMesh.GetComponent <PolygonCollider2D>() == null) return;

			CalcSearchRadius (navMesh);

			PolygonCollider2D poly = navMesh.GetComponent <PolygonCollider2D>();
			poly.pathCount = 1;
			
			if (navMesh.polygonColliderHoles.Count == 0)
			{
				if (rebuild)
				{
					RebuildVertexArray (navMesh.transform, poly);
					CreateCache ();
				}
				return;
			}
			
			Vector2 scaleFac = new Vector2 (1f / navMesh.transform.localScale.x, 1f / navMesh.transform.localScale.y);
			foreach (PolygonCollider2D hole in navMesh.polygonColliderHoles)
			{
				if (hole != null)
				{
					poly.pathCount ++;
					
					List<Vector2> newPoints = new List<Vector2>();
					foreach (Vector2 holePoint in hole.points)
					{
						Vector2 relativePosition = hole.transform.TransformPoint (holePoint) - navMesh.transform.position;
						newPoints.Add (new Vector2 (relativePosition.x * scaleFac.x, relativePosition.y * scaleFac.y));
					}
					
					poly.SetPath (poly.pathCount-1, newPoints.ToArray ());
					hole.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
					hole.isTrigger = true;
				}
			}

			if (rebuild)
			{
				RebuildVertexArray (navMesh.transform, poly);
				CreateCache ();
			}
		}
		
		
		private int[] buildSpanningTree (int source, int destination, float[,] weight)
		{
			int n = (int) Mathf.Sqrt (weight.Length);
			
			bool[] visit = new bool[n];
			float[] distance = new float[n];
			int[] precede = new int[n];
			
			for (int i=0 ; i<n ; i++)
			{
				distance[i] = Mathf.Infinity;
				precede[i] = 100000;
			}
			distance[source] = 0;
			
			int current = source;
			while (current != destination)
			{
				if (current < 0)
				{
					return null;
				}
				
				float distcurr = distance[current];
				float smalldist = Mathf.Infinity;
				int k = -1;
				visit[current] = true;
				
				for (int i=0; i<n; i++)
				{
					if (visit[i])
					{
						continue;
					}
					
					float newdist = distcurr + weight[current,i];
					if (weight[current,i] == -1f)
					{
						newdist = Mathf.Infinity;
					}
					if (newdist < distance[i])
					{
						distance[i] = newdist;
						precede[i] = current;
					}
					if (distance[i] < smalldist)
					{
						smalldist = distance[i];
						k = i;
					}
				}
				current = k;
			}
			
			return precede;
		}
		
		
		private int[] getShortestPath (int source, int destination, int[] precede)
		{
			int i = destination;
			int finall = 0;
			int[] path = new int[MAXNODES];
			
			path[finall] = destination;
			finall++;
			while (precede[i] != source)
			{
				i = precede[i];
				path[finall] = i;
				finall ++;
			}
			path[finall] = source;
			
			int[] result = new int[finall+1];
			
			for (int j=0; j<finall+1; j++)
			{
				result[j] = path[j];
			}
			
			return result;
		}
		

		private float[,] pointsToWeight (Vector2[] points, bool useCache = false)
		{
			int n = points.Length;
			int m = n;

			float[,] graph = new float [n, n];

			if (useCache)
			{
				graph = cachedGraph;
				n = 2;
			}

			for (int i=0; i<n; i++)
			{
				for (int j=i; j<m; j++)
				{
					if (i==j)
					{
						graph[i,j] = -1f;
					}
					else if (!IsLineClear (points[i], points[j]))
					{
						graph[i,j] = graph[j,i] = -1f;
					}
					else
					{
						graph[i,j] = graph[j,i] = (points[i] - points[j]).magnitude; // Sqr?
					}
				}
			}
			return graph;
		}


		private Vector2 GetNearestToMesh (Vector2 vertex, PolygonCollider2D poly)
		{
			// Test to make sure starting on the collision mesh
			RaycastHit2D hit = Physics2D.Raycast (vertex - new Vector2 (0.005f, 0f), new Vector2 (1f, 0f), 0.01f, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer);
			if (!hit)
			{
				Transform t = KickStarter.sceneSettings.navMesh.transform;
				float minDistance = -1;
				Vector2 nearestPoint = vertex;

				for (int i=0; i<poly.pathCount; i++)
				{
					Vector2[] path = poly.GetPath (i);

					for (int j=0; j<path.Length; j++)
					{
						Vector2 startPoint = t.TransformPoint (path[j]);
						Vector2 endPoint = Vector2.zero;
						if (j==path.Length-1)
						{
							endPoint = t.TransformPoint (path[0]);
						}
						else
						{
							endPoint = t.TransformPoint (path[j+1]);
						}

						Vector2 direction = endPoint - startPoint;
						for (float k=0f; k<=1f; k+=0.1f)
						{
							float distance = (vertex - (startPoint + (direction * k))).magnitude; // Sqr?

							if (distance < minDistance || minDistance < 0f)
							{
								minDistance = distance;
								nearestPoint = startPoint + (direction * k);
							}
						}
					}
				}
				return nearestPoint;
			}
			return (vertex);	
		}

		
		private Vector2[] AddEndsToList (Vector2[] points, Vector2 originPos, Vector2 targetPos, bool checkForDuplicates = true)
		{
			List<Vector2> newPoints = new List<Vector2>();

			foreach (Vector2 point in points)
			{
				if ((point != originPos && point != targetPos) || !checkForDuplicates)
				{
					newPoints.Add (point);
				}
			}

			newPoints.Insert (0, targetPos);
			newPoints.Insert (0, originPos);
			
			return newPoints.ToArray ();
		}


		private bool IsLineClear (Vector2 startPos, Vector2 endPos)
		{
			// This will test if points can "see" each other, by doing a circle overlap check along the line between them
			Vector2 actualPos = startPos;
			Vector2 direction = (endPos - startPos).normalized;
			float magnitude = (endPos - startPos).magnitude;

			//float searchRadius = 0.02f;// (endPos - startPos).magnitude * 0.02f;
			//float searchRadius * 2f
			float searchStep = 100f * searchRadius * searchRadius; // squared so that gap between circles increases with radius

			for (float i=0f; i<magnitude; i+= searchStep)
			{
				actualPos = startPos + (direction * i);
				if (Physics2D.OverlapCircleNonAlloc (actualPos, searchRadius, NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 1)
				{
					return false;
				}
			}

			return true;
		}


		private Vector2 GetLineIntersect (Vector2 startPos, Vector2 endPos)
		{
			// Important: startPos is considered to be outside the NavMesh

			Vector2 actualPos = startPos;
			Vector2 direction = (endPos - startPos).normalized;
			float magnitude = (endPos - startPos).magnitude;

			int numInside = 0;
			
			float radius = magnitude * 0.02f;
			for (float i=0f; i<magnitude; i+= (radius * 2f))
			{
				actualPos = startPos + (direction * i);

				if (Physics2D.OverlapCircleNonAlloc (actualPos, radius, NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
				{
					numInside ++;
				}
				if (numInside == 2)
				{
					return actualPos;
				}
			}
			return Vector2.zero;
		}
		

		public override string GetPrefabName ()
		{
			return ("NavMesh2D");
		}
		
		
		public override void SetVisibility (bool visibility)
		{
			#if UNITY_EDITOR
			NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
			Undo.RecordObjects (navMeshes, "Navigation visibility");
			
			foreach (NavigationMesh navMesh in navMeshes)
			{
				navMesh.showInEditor = visibility;
				UnityVersionHandler.CustomSetDirty (navMesh, true);
			}
			#endif
		}
		
		
		public override void SceneSettingsGUI ()
		{
			#if UNITY_EDITOR
			KickStarter.sceneSettings.navMesh = (NavigationMesh) EditorGUILayout.ObjectField ("Default NavMesh:", KickStarter.sceneSettings.navMesh, typeof (NavigationMesh), true);
			if (AdvGame.GetReferences ().settingsManager && !AdvGame.GetReferences ().settingsManager.IsUnity2D ())
			{
				EditorGUILayout.HelpBox ("This method is only compatible with 'Unity 2D' mode.", MessageType.Warning);
			}
			#endif
		}


		private void AddCharHoles (PolygonCollider2D navPoly, AC.Char charToExclude, CharacterEvasion characterEvasion)
		{
			if (characterEvasion == CharacterEvasion.None)
			{
				return;
			}

			ResetHoles (KickStarter.sceneSettings.navMesh, false);

			Vector2 navPosition = navPoly.transform.position;
			AC.Char[] characters = GameObject.FindObjectsOfType (typeof (AC.Char)) as AC.Char[];

			foreach (AC.Char character in characters)
			{
				CircleCollider2D circleCollider2D = character.GetComponent <CircleCollider2D>();
				if (circleCollider2D != null &&
				    (character.charState == CharState.Idle || characterEvasion == CharacterEvasion.AllCharacters) &&
				    (charToExclude == null || character != charToExclude) && 
				    Physics2D.OverlapPointNonAlloc (character.transform.position, NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
				{
					circleCollider2D.isTrigger = true;
					List<Vector2> newPoints3D = new List<Vector2>();
					
					#if UNITY_5
					Vector2 centrePoint = character.transform.TransformPoint (circleCollider2D.offset);
					#else
					Vector2 centrePoint = character.transform.TransformPoint (circleCollider2D.center);
					#endif
					
					float radius = circleCollider2D.radius * character.transform.localScale.x;
					
					newPoints3D.Add (centrePoint + Vector2.up * radius);
					newPoints3D.Add (centrePoint + Vector2.right * radius);
					newPoints3D.Add (centrePoint - Vector2.up * radius);
					newPoints3D.Add (centrePoint - Vector2.right * radius);
					
					navPoly.pathCount ++;
					
					List<Vector2> newPoints = new List<Vector2>();
					for (int i=0; i<newPoints3D.Count; i++)
					{
						// Only add a point if it is on the NavMesh
						if (Physics2D.OverlapPointNonAlloc (newPoints3D[i], NavigationEngine_PolygonCollider.results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
						{
							newPoints.Add (newPoints3D[i] - navPosition);
						}
						else
						{
							Vector2 altPoint = GetLineIntersect (newPoints3D[i], centrePoint);
							if (altPoint != Vector2.zero)
							{
								newPoints.Add (altPoint - navPosition);
							}
						}
					}

					if (newPoints.Count > 1)
					{
						navPoly.SetPath (navPoly.pathCount-1, newPoints.ToArray ());
					}
				}
			}

			RebuildVertexArray (navPoly.transform, navPoly);
		}


		private void RebuildVertexArray (Transform navMeshTransform, PolygonCollider2D poly)
		{
			List<Vector2> _vertexData = new List<Vector2>();
			
			for (int i=0; i<poly.pathCount; i++)
			{
				Vector2[] _vertices = poly.GetPath (i);
				foreach (Vector2 _vertex in _vertices)
				{
					Vector3 vertex3D = navMeshTransform.TransformPoint (new Vector3 (_vertex.x, _vertex.y, navMeshTransform.position.z));
					_vertexData.Add (new Vector2 (vertex3D.x, vertex3D.y));
				}
			}
			vertexData = _vertexData.ToArray ();
		}


		private void CalcSearchRadius (NavigationMesh navMesh)
		{
			searchRadius = 0.1f - (0.08f * navMesh.accuracy);
		}
		

		private void CreateCache ()
		{
			if (!Application.isPlaying)
			{
				return;
			}
		
			// Create table of weights with "dummy" start/end vertices, as these are at the front anyway - so anything below will be the same anyway
			Vector2[] pointsList = vertexData;
			
			Vector2 originPos = Vector2.zero;
			Vector2 targetPos = Vector2.zero;

			pointsList = AddEndsToList (pointsList, originPos, targetPos, false);
			
			cachedGraph = pointsToWeight (pointsList, false);

			#if UNITY_ANDROID || UNITY_IOS
			if (KickStarter.sceneSettings.navMesh != null && KickStarter.sceneSettings.navMesh.characterEvasion != CharacterEvasion.None)
			{
				ACDebug.Log ("The NavMesh's 'Character evasion' setting should be set to 'None' for best performance on mobile devices.");
			}
			#endif
		}


		#if UNITY_EDITOR

		public override NavigationMesh NavigationMeshGUI (NavigationMesh _target)
		{
			_target = base.NavigationMeshGUI (_target);

			_target.characterEvasion = (CharacterEvasion) EditorGUILayout.EnumPopup ("Character evasion:", _target.characterEvasion);
			if (_target.characterEvasion != CharacterEvasion.None)
			{
				EditorGUILayout.HelpBox ("Note: Characters can only be avoided if they have a Circle Collider 2D (no Trigger) component on their base.\n\n" +
					"For best results, set a non-zero 'Pathfinding update time' in the Settings Manager.", MessageType.Info);

				#if UNITY_ANDROID || UNITY_IOS
				EditorGUILayout.HelpBox ("This is an expensive calculation - consider setting this to 'None' for mobile platforms.", MessageType.Warning);
				#endif
			}

			_target.accuracy = EditorGUILayout.Slider ("Accuracy:", _target.accuracy, 0f, 1f);

			int numOptions = _target.polygonColliderHoles.Count;
			numOptions = EditorGUILayout.IntField ("Number of holes:", _target.polygonColliderHoles.Count);
			if (numOptions < 0)
			{
				numOptions = 0;
			}
			
			if (numOptions < _target.polygonColliderHoles.Count)
			{
				_target.polygonColliderHoles.RemoveRange (numOptions, _target.polygonColliderHoles.Count - numOptions);
			}
			else if (numOptions > _target.polygonColliderHoles.Count)
			{
				if (numOptions > _target.polygonColliderHoles.Capacity)
				{
					_target.polygonColliderHoles.Capacity = numOptions;
				}
				for (int i=_target.polygonColliderHoles.Count; i<numOptions; i++)
				{
					_target.polygonColliderHoles.Add (null);
				}
			}
			
			for (int i=0; i<_target.polygonColliderHoles.Count; i++)
			{
				_target.polygonColliderHoles [i] = (PolygonCollider2D) EditorGUILayout.ObjectField ("Hole #" + i.ToString () + ":", _target.polygonColliderHoles [i], typeof (PolygonCollider2D), true);
			}

			return _target;
		}


		public override void DrawGizmos (GameObject navMeshOb)
		{
			if (navMeshOb != null && navMeshOb.GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (navMeshOb.transform, navMeshOb.GetComponent <PolygonCollider2D>(), Color.white);
			}
		}

		#endif

	}
	
}