using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IGLvlEditor : MonoBehaviour
{
    private const float cooldown = 0.05f;

    public Level thisLevel;
    private string thisLevelName;
    private static int newLevelIndex;
    public static int editLevelNumber;
    private static Transform objectParent => Game.instance.level ? Game.instance.level.objectParent : null;
    private static Transform world => Game.instance.level ? Game.instance.level.world : null;

    public GameObject marker;
    public bool editing;

    private new Camera camera;

    private Transform cubes;

    private bool touching;
    private Vector3 touchPos;
    private Vector3 swipeDir;

    private Vector3 lastDir;
    private Vector3 lastPos;

    private float slideDuration = 0.2f;

    public bool myVar = true;
    private bool rejected;

    public bool sliding { get; private set; }
    public static IGLvlEditor instance { get; private set; }
    public Vector3 slide { get; private set; }

    GraphicRaycaster m_Raycaster;
    EventSystem m_EventSystem;

    private bool selectMode;

#if UNITY_EDITOR
    void Start()
    {
        camera = Game.instance.transform.Find("Camera").GetComponent<Camera>();
    }

 
    void Update()
    {
        if (editing)
        {
            if (!instance)
                instance = this;
            if (!marker)
            {
                marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Marker")) as GameObject;
                marker.transform.position = new Vector3(-2, -1, -1);
                marker.transform.parent = objectParent;
                lastPos = marker.transform.position;
            }

            if (Input.GetMouseButton(0))
            {
                bool clickUI = false;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    if (results[0].gameObject.layer == 5)
                        clickUI = true;
                }
                foreach (RaycastHit hit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition)))
                {
                    if (!clickUI)
                    {
                        if (hit.collider.CompareTag("TouchSurface"))
                        {
                            if (!touching)
                            {
                                touching = true;
                                touchPos = hit.point;
                            }
                            else
                            {
                                Vector3 delta = hit.point - touchPos;
                                if (delta.magnitude > 1f)
                                {
                                    touchPos = hit.point;
                                    swipeDir = delta.normalized;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                touching = false;
                swipeDir = Vector3.zero;
            }
            Vector3 dir = Vector3.zero;

            if (swipeDir != Vector3.zero)
                dir = swipeDir.Orthogonalize();
            else if (Input.GetKey(KeyCode.RightArrow))
                dir = Vector3.right;
            else if (Input.GetKey(KeyCode.LeftArrow))
                dir = Vector3.left;
            else if (Input.GetKey(KeyCode.DownArrow))
                dir = Vector3.back;
            else if (Input.GetKey(KeyCode.UpArrow))
                dir = Vector3.forward;

            if (lastDir != dir)
            {
                if (dir != Vector3.zero)
                    lastDir = dir;
            }

            if (!sliding)
            {
                if (dir != Vector3.zero)
                {
                    Slide(dir);
                }
            }
            
        }
    }

    private void Slide(Vector3 dir)
    {
        sliding = true;
        Tween.Position(marker.transform, marker.transform.position + dir * 1, slideDuration, 0.0f, completeCallback: OnSlideDone);
    }

    private void OnSlideDone()
    {
        marker.transform.position = marker.transform.position.Round();
        lastPos = marker.transform.position;
        Invoke("EndSlide", cooldown);
    }

    private void EndSlide()
    {
        sliding = false;
    }

    public void goUp()
    {
        lastDir = new Vector3(0, 1, 0);
        Slide(lastDir);
    }

    public void goDown()
    {
        lastDir = new Vector3(0, -1, 0);
        Slide(lastDir);
    }

    public void AddLevel()
    {
        const string nameRoot = "New Level";
        string name = nameRoot + (newLevelIndex > 0 ? " (" + newLevelIndex + ")" : "");
        for (; Resources.Load<Level>(ResourceLoader.Path<Level>() + name); name = nameRoot + " (" + ++newLevelIndex + ")") ;
        var levelObj = Instantiate(ResourceLoader.Get<GameObject>("Prefabs/LevelTemplate"));
        levelObj.name = name;
        thisLevelName = name;
        thisLevel = levelObj.GetComponent<Level>();
        PrefabUtility.SaveAsPrefabAsset(levelObj, ResourceLoader.Path<Level>(true) + name + ".prefab");
        Object.DestroyImmediate(levelObj);
        ++newLevelIndex;

        Edit();
        LoadLevel(name);
    }

    public void SaveLevel()
    {
        PrefabUtility.SaveAsPrefabAsset(thisLevel.gameObject, ResourceLoader.Path<Level>(true) + thisLevelName + ".prefab");
    }

    public void SaveAndQuit()
    {
        stopEdit();
        Game.instance.LoadLevel(Game.instance.levelNumber);
    }

    public void ScrapLevel()
    {
        Level[] lvlList = ResourceLoader.GetAll<Level>();
        string name = lvlList[lvlList.Length-1].name;
        editing = false;
        AssetDatabase.DeleteAsset("Assets/Resources/Prefabs/Levels/" + name + ".prefab");
        //Destroy(objects.GetComponentInChildren<Omino>().gameObject);
        foreach (Transform child in objectParent)
        {
           Destroy(child.gameObject);
        }
        foreach (Transform child in world)
        {
            Destroy(child.gameObject);
        }
        Game.instance.LoadLevel(Game.instance.levelNumber);
        --newLevelIndex;
    }

    public void RestartEditLvl() => LoadLevel(thisLevelName);

    private void LoadLevel(string name)
    {
        //Debug.Log(name);
        Level lvlToLoad;
        Game game = Game.instance;
        game.levels = ResourceLoader.GetAll<Level>();
        bool foundLevel = false;
        editLevelNumber = game.levels.Length;
        foreach (Level lvl in game.levels)
        {
            if (lvl.gameObject.name == name)
            {
                lvlToLoad = lvl;
                foundLevel = true;
                if (game.level)
                    Destroy(game.level.gameObject);
                game.level = Instantiate(lvlToLoad);
                thisLevel = game.level;
                game.level.transform.SetParent(game.transform);
                if (!editing)
                    game.Play();
            }
        }
        if (!foundLevel)
            Debug.LogError("Failed to load level \"" + name + "\"");
      
    }

    private static void AddWall(Vector3 position, Transform world)
    {
        var wall = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Wall")) as GameObject;
        wall.name = "Wall";
        wall.transform.SetParent(world);
        wall.transform.position = position.Round();
    }

    public void AddWorldItem(string name)
    {
        var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
        obj.name = name;
        obj.transform.SetParent(world);
        obj.transform.position = marker.transform.position.Round();
        obj.transform.rotation = marker.transform.rotation;
        Slide(lastDir);
    }

    public void AddObjectItem(string name)
    {
        var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
        obj.name = name;
        obj.transform.SetParent(objectParent);
        obj.transform.position = marker.transform.position.Round();
        obj.transform.rotation = marker.transform.rotation;
        Slide(lastDir);
    }

    public void stopEdit()
    {
        editing = false;
        marker.SetActive(false);
        SaveLevel();
    }

    public void Edit()
    {
        editing = true;
        if (marker)
            marker.SetActive(true);
    }
#endif
}

