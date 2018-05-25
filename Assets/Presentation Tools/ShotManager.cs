using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PresentaionsTools
{
	[ExecuteInEditMode]
	public class ShotManager : EditorWindow
	{
		ShotObject storage;

		public ShotObject Storage
		{
			get
			{
				if (storage != null)
					return storage;

				storage = ShotObject.InitializeStorage();
				return storage;
			}
			set
			{
				storage = value;
			}
		}

		Vector2 _v = new Vector2();

		[MenuItem("Window/Presentation Tools/Shot Manager")]
		static void showShotManager()
		{
			GetWindow<ShotManager>("Shot Manager").Show();
		}

		[MenuItem("Window/Presentation Tools/Shot Viewer")]
		static void showShotViewer()
		{
			GetWindow<ShotViewer>("Shot Viewer").Show();
		}

		[MenuItem("Window/Presentation Tools/Display Shot One %1")]
		static void slot1()
		{
			GetWindow<ShotManager>().ApplyShot(1);
		}
		[MenuItem("Window/Presentation Tools/Display Shot Two %2")]
		static void slot2()
		{
			GetWindow<ShotManager>().ApplyShot(2);
		}
		[MenuItem("Window/Presentation Tools/Display Shot Three %3")]
		static void slot3()
		{
			GetWindow<ShotManager>().ApplyShot(3);
		}

		void OnGUI()
		{
			if (Storage != null)
			{
				RenderShotInspector();

				//Render new shot buttons
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("New Shot"))
					Storage.shots.Add(new Shot("New Shot"));

				if (GUILayout.Button("Capture Shot"))
				{
					Shot shot = new Shot("New Shot");
					RecordSceneCamera(shot);
					RecordSelection(shot);
					RecordTimeStamp(shot);
					Storage.shots.Add(shot);
				}

				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			Storage = (ShotObject)EditorGUILayout.ObjectField(Storage, typeof(ShotObject), allowSceneObjects: false);

			if (GUILayout.Button("PreWarm", GUILayout.Width(70)))
			{
				ShotManager.PreWarm(Storage);
			}
			GUILayout.EndHorizontal();

			if (Storage != null)
				EditorUtility.SetDirty(Storage);
		}

		void RenderShotInspector()
		{
			_v = EditorGUILayout.BeginScrollView(_v);
			foreach (var shot in Storage.shots)
			{
				//Shot and delete buttons
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(shot.name))
					ApplyShot(shot, Storage);

				if (GUILayout.Button("↑", GUILayout.Width(30)))
				{
					MoveShot(shot, true);
					return;
				}
				if (GUILayout.Button("↓", GUILayout.Width(30)))
				{
					MoveShot(shot, false);
					return;
				}
				if (GUILayout.Button("X", GUILayout.Width(30)) && EditorUtility.DisplayDialog("Confirm", "Confirm deletion of Shot entry", "Delete", "Cancel"))
				{
					Storage.shots.Remove(shot);
					return;
				}
				GUILayout.EndHorizontal();

				//Render selection buttons
				if (shot.selectedObjs.Count > 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Selections: ", GUILayout.Width(80));
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					for (int i = 0; i < shot.selectedObjs.Count; i++)
					{
						if (i != 0 && i % 2 == 0)
						{
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
						}

						if (GUILayout.Button(shot.selectedObjs[i].name, GUILayout.Width(105)))
							ApplySelection(shot.selectedObjs[i].SelectedObj);
					}
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				shot.isEditing = EditorGUILayout.Foldout(shot.isEditing, "Click to Edit: " + shot.name, true);
				if (!shot.isEditing)
					continue;

				//Fields
				shot.name = EditorGUILayout.TextField("Shot Name", shot.name);
				shot.position = EditorGUILayout.Vector3Field("Cam Position", shot.position);
				shot.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Cam Rotation", shot.rotation.eulerAngles));
				shot.size = EditorGUILayout.FloatField("Cam Distance", shot.size);
				shot.playableTimestamp = EditorGUILayout.DoubleField("Playable Time", shot.playableTimestamp);

				//Time, camera, and selection buttons
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Store Time"))
					RecordTimeStamp(shot);

				if (GUILayout.Button("Store Camera"))
					RecordSceneCamera(shot);

				if (GUILayout.Button("Store Selection") && Selection.activeTransform != null)
					RecordSelection(shot);
				GUILayout.EndHorizontal();

				//Render the select items
				foreach (ShotSelection selection in shot.selectedObjs)
				{
					GUILayout.BeginHorizontal();
					selection.name = EditorGUILayout.TextField("Selection", selection.name);
					if (GUILayout.Button("X", GUILayout.Width(30)))
					{
						shot.selectedObjs.Remove(selection);
						return;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.LabelField("Object: " + selection.name);
				}

				GUILayout.Space(10);
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
				GUILayout.Space(10);
			}
			EditorGUILayout.EndScrollView();
		}

		void RecordSceneCamera(Shot shot)
		{
			shot.position = SceneView.lastActiveSceneView.pivot;
			shot.rotation = SceneView.lastActiveSceneView.rotation;
			shot.size = SceneView.lastActiveSceneView.size;

		}

		void RecordSelection(Shot shot)
		{
			Transform t = Selection.activeTransform;
			if (t != null)
			{
				//Make sure selection isn't already added
				foreach (ShotSelection selection in shot.selectedObjs)
					if (selection.SelectedObj == t)
						return;

				//Add selection
				shot.selectedObjs.Add(new ShotSelection() { name = t.name, SelectedObj = t });

				//Add scene to build settings
				List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
				string scenePath = t.gameObject.scene.path;
				if (!string.IsNullOrEmpty(scenePath))
				{
					foreach (var scene in editorBuildSettingsScenes)
					{
						if (scene.path == scenePath)
							return;
					}
					editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, false));
					EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
				}
			}
		}

		void RecordTimeStamp(Shot shot)
		{
			if (Storage.Director != null)
				shot.playableTimestamp = Storage.Director.time;
		}

		void ApplyShot(int index)
		{
			if (index > Storage.shots.Count)
				return;

			ShotManager.ApplyShot(Storage.shots[index - 1], Storage);
		}

		public static void ApplyShot(Shot shot, ShotObject shotObj)
		{
			SceneView.lastActiveSceneView.pivot = shot.position;
			SceneView.lastActiveSceneView.rotation = shot.rotation;
			SceneView.lastActiveSceneView.size = shot.size;
			SceneView.lastActiveSceneView.Repaint();

			if (shot.selectedObjs.Count > 0)
				ShotManager.ApplySelection(shot.selectedObjs[0].SelectedObj);
			else
				ShotManager.ApplySelection(null);

			if (shotObj != null && shotObj.Director != null)
			{
				shotObj.Director.time = shot.playableTimestamp;
				shotObj.Director.Evaluate();
			}
		}

		public static void ApplySelection(Transform selection)
		{
			Selection.activeObject = selection;
		}

		public static void PreWarm(ShotObject shotObj)
		{
			if (shotObj == null || shotObj.shots.Count <= 0)
				return;

			var shots = shotObj.shots;

			for (int i = shots.Count - 1; i >= 0; i--)
			{
				ShotManager.ApplyShot(shots[i], shotObj);

				var selections = shots[i].selectedObjs;
				for (int j = selections.Count - 1; j >= 0; j--)
					ShotManager.ApplySelection(selections[j].SelectedObj);
			}
		}

		void MoveShot(Shot shot, bool moveUp)
		{
			int index = Storage.shots.IndexOf(shot);
			int newIndex = index + (moveUp ? -1 : 1);
			if (newIndex < 0 || newIndex >= Storage.shots.Count)
				return;

			Storage.shots.Remove(shot);
			Storage.shots.Insert(newIndex, shot);
		}
	}
}