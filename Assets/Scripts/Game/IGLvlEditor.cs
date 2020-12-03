using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IGLvlEditor : MonoBehaviour
{

    private static Level level;
    private static int newLevelIndex;
    private static Transform objects => level ? level.transform.Find("Objects") : null;
    private static Transform world => level ? level.transform.Find("World") : null;
    //List<string> lvlList;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLevel()
    {
        const string nameRoot = "New Level";
        string name = nameRoot;
        for (; Resources.Load<Level>(ResourceLoader.Path<Level>() + name); name = nameRoot + " (" + ++newLevelIndex + ")") ;
        level = new GameObject(name).AddComponent<Level>();

        var objects = new GameObject("Objects").transform;
        objects.SetParent(level.transform);

        var world = new GameObject("World").transform;
        world.SetParent(level.transform);

        var omino = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Omino")) as GameObject;
        omino.transform.SetParent(objects);

        for (int x = -1; x <= 1; ++x)
            for (int z = -1; z <= 1; ++z)
                AddWall(new Vector3(x, -1, z));

        PrefabUtility.SaveAsPrefabAsset(level.gameObject, ResourceLoader.Path<Level>(true) + name + ".prefab");
        Object.DestroyImmediate(level.gameObject);

        LoadLevel(name);
    }

    public void ScrapLevel()
    {
        Level[] lvlList = ResourceLoader.GetAll<Level>();
        string name = lvlList[lvlList.Length-1].name;
        //Debug.Log("Omino: " + objects.GetComponentInChildren<Omino>().gameObject);
        AssetDatabase.DeleteAsset("Assets/Resources/Prefabs/Levels/" + name + ".prefab");
        Destroy(objects.GetComponentInChildren<Omino>().gameObject);
        Game.instance.LoadLevel(Game.instance.levelNumber);
        --newLevelIndex;
    }

    private static void LoadLevel(string name)
    {
        //Debug.Log(name);
        Level lvlToLoad;
        Level[] lvlList = ResourceLoader.GetAll<Level>();
        bool foundLevel = false;
        foreach (Level lvl in lvlList)
        {
            if (lvl.gameObject.name == name)
            {
                lvlToLoad = lvl;
                foundLevel = true;
                Selection.objects = new Object[0];
                Game game = Game.instance;
                level = game.level;
                if (level)
                    Destroy(level.gameObject);
                level = Instantiate(lvlToLoad);
                level.transform.SetParent(game.transform);
            }
            //Debug.Log(lvl.gameObject.name);
            //Debug.Log("ResourceLoader: " + ResourceLoader.Get<Level>(name));
        }
        if (!foundLevel)
            Debug.LogError("Failed to load level \"" + name + "\"");
      
    }

    private static void AddWall(Vector3 position, bool undoable = false)
    {
        var wall = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Wall")) as GameObject;
        wall.name = "Wall";
        wall.transform.SetParent(world);
        wall.transform.position = position.Round();

        if (undoable)
            Undo.RegisterCreatedObjectUndo(wall.gameObject, "Create Object");
    }
}

