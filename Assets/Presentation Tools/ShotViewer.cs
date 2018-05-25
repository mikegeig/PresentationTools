using UnityEditor;
using UnityEngine;

namespace PresentaionsTools
{
	[ExecuteInEditMode]
	public class ShotViewer : EditorWindow
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

		void OnGUI()
		{
			if (Storage != null)
			{
				RenderShotInspector();
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
				if (GUILayout.Button(shot.name))
					ShotManager.ApplyShot(shot, Storage);

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
							ShotManager.ApplySelection(shot.selectedObjs[i].SelectedObj);
					}
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				GUILayout.Space(10);
			}
			EditorGUILayout.EndScrollView();
		}
	}
}