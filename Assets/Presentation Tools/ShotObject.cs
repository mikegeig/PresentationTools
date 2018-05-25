using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace PresentaionsTools
{
	[System.Serializable]
	public class ShotObject : ScriptableObject
	{
		public List<Shot> shots;
		public string playableObjectName = "GameController";

		[SerializeField] string pathToObj;
		[SerializeField] string sceneName;
		[SerializeField] string rootObjName;

		[SerializeField] int directorID;
		PlayableDirector director;
		public PlayableDirector Director
		{
			get {
				if (director != null)
					return director;

				if (sceneName != "")
				{
					var obj = ShotHelper.LoadSelection(sceneName, rootObjName, pathToObj);
					if (obj != null)
					{
						director = obj.GetComponent<PlayableDirector>();
						return director;
					}
				}
				else
				{
					var obj = GameObject.Find(playableObjectName);
					if (obj != null)
					{
						Director = obj.GetComponent<PlayableDirector>();
						return director;
					}
				}

				return null;
			}
			set {

				director = value;
				if(director != null)
					ShotHelper.RecordObjPath(value.transform, out sceneName, out rootObjName, out pathToObj);
			}
		}

		void Init()
		{
			shots = new List<Shot>();
			if (Director == null)
			{
				var obj = GameObject.Find(playableObjectName);
				if(obj != null)
					Director = obj.GetComponent<PlayableDirector>();
			}
		}

		public static ShotObject InitializeStorage()
		{
			var guids = AssetDatabase.FindAssets("t: ShotObject");
			if (guids.Length > 0)
			{
				string p = AssetDatabase.GUIDToAssetPath(guids[0]);
				ShotObject obj = AssetDatabase.LoadAssetAtPath(p, typeof(ShotObject)) as ShotObject;

				if (obj != null)
					return obj;
			}

			ShotObject asset = CreateInstance<ShotObject>();
			asset.Init();

			string path = "Assets/Shot Storage.asset";
			AssetDatabase.CreateAsset(asset, path);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return asset;
		}
	}

	[System.Serializable]
	public class Shot
	{
		public string name;
		public Vector3 position;
		public Quaternion rotation;
		public float size;
		public List<ShotSelection> selectedObjs;
		public double playableTimestamp;
		public bool isEditing;

		public Shot(string ID)
		{
			name = ID;
			selectedObjs = new List<ShotSelection>();
		}
	}

	[System.Serializable]
	public class ShotSelection
	{
		public string name;

		[SerializeField] string pathToObj;
		[SerializeField] string sceneName;
		[SerializeField] string rootObjName;

		Transform selectedObj;
		public Transform SelectedObj
		{
			get
			{
				if (selectedObj != null)
					return selectedObj;

				if (sceneName != "")
				{
					selectedObj = ShotHelper.LoadSelection(sceneName, rootObjName, pathToObj);
					return selectedObj;
				}

				return null;
			}
			set
			{
				selectedObj = value;
				if (value != null)
					ShotHelper.RecordObjPath(value, out sceneName, out rootObjName, out pathToObj);
			}
		}
	}

	public class ShotHelper
	{
		public static void RecordObjPath(Transform obj, out string sceneName, out string rootObjName, out string pathToObj)
		{
			sceneName = obj.gameObject.scene.name;

			string path = "";
			while (obj.parent != null)
			{
				path = (obj.parent.parent == null ? "" : "/") + obj.name + path;

				obj = obj.parent;
			}

			rootObjName = obj.name;
			pathToObj = path;
		}

		public static Transform LoadSelection(string sceneName, string rootObjName, string pathToObj)
		{
			Scene scene = SceneManager.GetSceneByName(sceneName);
			if (scene.buildIndex == -1)
				return null;

			GameObject[] roots = scene.GetRootGameObjects();
			if (roots == null || roots.Length == 0)
				return null;

			foreach (GameObject obj in roots)
			{
				if (obj.name == rootObjName)
				{
					if (pathToObj == "")
						return obj.transform;

					Transform t = obj.transform.Find(pathToObj);
					if (t != null)
						return t;
				}
			}

			return null;
		}
	}
}