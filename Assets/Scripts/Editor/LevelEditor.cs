using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class LevelEditor : EditorWindow
{
    private static Level level;
    private static Transform objects => level ? level.transform.Find("Objects") : null;
    private static Transform world => level ? level.transform.Find("World") : null;
    private static int newLevelIndex;

    [MenuItem("Window/Level Editor")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        var editor = GetWindow<LevelEditor>();
        editor.titleContent = new GUIContent("Level Editor");
    }

    private void Awake()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private void OnGUI()
    {
        SceneView.duringSceneGui -= OnScene;
        SceneView.duringSceneGui += OnScene;

        // Select Level
        GUILayout.Label("Select Level");
        var lvlList = new List<string>();
        foreach (var lvl in ResourceLoader.GetAll<Level>())
            lvlList.Add(lvl.name);
        lvlList.Add("Add New Level");

        int index = (level ? lvlList.IndexOf(level.name) : -1);
        int newIndex = EditorGUILayout.Popup(index, lvlList.ToArray());
        if (newIndex != index && newIndex >= 0)
        {
            if (newIndex == lvlList.Count-1)
                AddLevel();
            else
                LoadLevel(lvlList[newIndex]);
        }

        // Delete Level
        if (index != -1)
        {
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Level")) DeleteLevel(lvlList[index]);
        }
    }

    private void OnScene(SceneView sceneview)
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null && prefabStage.scene.isLoaded)
            level = prefabStage.scene.GetRootGameObjects()[0].GetComponent<Level>();
        if (level == null) return;

        Event e = Event.current;

        if (e.modifiers != EventModifiers.Control) return;

        Vector3? point = null;
        GameObject hovered = null;
        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (level.gameObject.scene.GetPhysicsScene().Raycast(ray.origin, ray.direction, out hit))
        {
            if (hit.collider.CompareTag("Level") || hit.collider.CompareTag("World"))
            {
                point = (hit.point + hit.normal * 0.5f).Round();
                Handles.DrawWireCube(point.Value, Vector3.one);

                if (hit.collider.CompareTag("World"))
                    hovered = hit.collider.gameObject;
            }
        }

        switch (e.type)
        {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(0);
                break;

            case EventType.MouseMove:
                SceneView.RepaintAll();
                break;

            case EventType.MouseDown:
                if (e.button == 0 && e.isMouse && point.HasValue)
                    AddWall(point.Value, true);
                else if (e.button == 1 && hovered)
                    RemoveWall(hovered, true);
                break;
        }
    }

    private static void AddLevel()
    {
        const string nameRoot = "New Level";
        string name = nameRoot + (newLevelIndex > 0 ? " (" + newLevelIndex + ")" : "");
        for (; Resources.Load<Level>(ResourceLoader.Path<Level>() + name); name = nameRoot + " (" + ++newLevelIndex + ")") ;
        var levelObj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/LevelTemplate")) as GameObject;
        PrefabUtility.UnpackPrefabInstance(levelObj, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        PrefabUtility.SaveAsPrefabAsset(levelObj, ResourceLoader.Path<Level>(true) + name + ".prefab");
        DestroyImmediate(levelObj);
        ++newLevelIndex;

        LoadLevel(name);
    }

    private static void LoadLevel(string name)
    {
        Level[] levels = ResourceLoader.GetAll<Level>();
        bool levelFound = false;
        foreach (Level lvl in levels)
        {
            if (lvl.gameObject.name == name)
            {
                AssetDatabase.OpenAsset(lvl);
                levelFound = true;
            }
        }
            if (levelFound)
                Selection.objects = new Object[0];
            else
                Debug.LogError("Failed to load level \"" + name + "\"");
    }

    private static void DeleteLevel(string name)
    {
        if (EditorUtility.DisplayDialog("Delete Level", "Are you sure you want to delete " + name + "?\nThis cannot be undone.", "Yes", "No"))
            AssetDatabase.DeleteAsset("Assets/Resources/Prefabs/Levels/" + name + ".prefab");
    }

    private static void AddWall(Vector3 position, bool undoable = false)
    {
        var wall = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Objects/Wall")) as GameObject;
        wall.name = "Wall";
        wall.transform.SetParent(world);
        wall.transform.position = position.Round();

        if (undoable)
            Undo.RegisterCreatedObjectUndo(wall.gameObject, "Create Object");
    }

    private static void RemoveWall(GameObject wall, bool undoable = false)
    {
        if (!wall.CompareTag("World"))
            return;

        if (undoable)
            Undo.DestroyObjectImmediate(wall);
        else
            DestroyImmediate(wall);
    }

}
