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

    private GameObject editScreenGO;

    public GameObject marker;

    public Vector3 markerCenter
    {
        get
        {
            if (!marker|| marker.transform.childCount == 0)
                return transform.position;

            Vector3 result = Vector3.zero;
            foreach (Transform markerCube in marker.transform)
                result += markerCube.position;
            return result / marker.transform.childCount;
        }
    }

    public bool editing;

    private new Camera camera;

    private Transform cubes;

    Transform baseCube;

    private bool touching;
    private Vector3 touchPos;
    private Vector3 firstTouchPos;
    private Vector3 swipeDir;
    //private Vector3 currTouchPos;

    //private Vector3 lastTouchPos;

    private bool updateSel = false;
    private bool uiSwipe = false;

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

<<<<<<< HEAD
    private bool selectMode = false;
    private bool selectMode3D = false;

    private int xMag;
    private int zMag;

    int deltaX;
    int deltaZ;

    float totDelta;

    private bool uiTouching;
    private Vector2 uiTouchPos;
    private Vector2 uiSwipeDir;
    private GameObject activeObj;
    private bool delActive;

    private bool markersRemoved;

=======
    private bool selectMode;

#if UNITY_EDITOR
>>>>>>> main
    void Start()
    {
        camera = Game.instance.transform.Find("Camera").GetComponent<Camera>();
    }

 
    void Update()
    {
        if (editing)
        {
            if (!instance)
            {
                instance = this;
                editScreenGO = GameObject.Find("EditScreen");
            }
            if (!marker)
            {
<<<<<<< HEAD
                marker = new GameObject("Marker");
                marker.transform.parent = objects;
                GameObject markerCube = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/MarkerCube")) as GameObject;
                markerCube.transform.position = new Vector3(-2, -1, -1);
                markerCube.transform.parent = marker.transform;
                lastPos = markerCube.transform.position;
                baseCube = markerCube.transform;
=======
                marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Marker")) as GameObject;
                marker.transform.position = new Vector3(-2, -1, -1);
                marker.transform.parent = objectParent;
                lastPos = marker.transform.position;
>>>>>>> main
            }

            uiSwipe = false;

            if (Input.GetMouseButton(0))
            {
                markersRemoved = false;
                bool clickUI = false;
                uiSwipe = false;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                if (results.Count > 1)
                {
                    if (results[1].gameObject.layer == 5)
                    {
                        clickUI = true;
                        if (results[1].gameObject.tag == "EditObject")
                        {
                            if (results[1].gameObject.name != "DelBtn")
                                activeObj = results[1].gameObject;
                            if (!uiTouching)
                            {
                                uiTouching = true;
                                uiTouchPos = pointerData.position;
                            }
                            else
                            {
                                Vector3 uiDelta = pointerData.position - uiTouchPos;
                                if (uiDelta.magnitude > 50)
                                {
                                    uiTouchPos = pointerData.position;
                                    uiSwipeDir = uiDelta.Orthogonalize();
                                    uiSwipe = true;
                                }
                            }
                        }
                    }
                }
                if (!clickUI)
                {
                    foreach (RaycastHit hit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition)))
                    {
                        if (hit.collider.CompareTag("TouchSurface"))
                        {
                            if (!touching)
                            {
                                touching = true;
                                touchPos = hit.point;
                                firstTouchPos = hit.point;
                            }
                            else
                            {
                                Vector3 delta = hit.point - touchPos;
                                totDelta = (hit.point - firstTouchPos).magnitude;
                                if (delta.magnitude > 1f)
                                {
                                    updateSel = true;
                                    touchPos = hit.point;
                                    swipeDir = delta.normalized;
                                    deltaX = Mathf.RoundToInt(touchPos.x - firstTouchPos.x);
                                    deltaZ = Mathf.RoundToInt(touchPos.z - firstTouchPos.z);
                                    xMag = Mathf.RoundToInt(Mathf.Abs(deltaX));
                                    zMag = Mathf.RoundToInt(Mathf.Abs(deltaZ));
                                }
                                else
                                {
                                    updateSel = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                touching = false;
                uiTouching = false;
                swipeDir = Vector3.zero;
                uiSwipeDir = Vector2.zero;
                if (totDelta < 0.1)
                {
                    if (!markersRemoved && selectMode)
                    {
                        removeMarkers();
                        markersRemoved = true;
                    }
                        
                }
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

            if (uiSwipe)
            {
                if (uiTouching)
                {
                    if (uiSwipeDir == Vector2.up)
                    {
                        if (delActive)
                        {
                            disableDel();
                        }
                        else
                        {
                            enableRot();
                        }
                    }
                    else if (uiSwipeDir == Vector2.down)
                    {
                        enableDel();
                    }
                }
            }
            else
            {
                if (selectMode)
                {
                    if (dir != Vector3.zero)
                    {
                        if (updateSel)
                        {
                            if (selectMode3D)
                            {
                                addMarkers3D();
                            }
                            else
                            {
                                addMarkers();
                            }
                        }
                    }
                }
                else
                {
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
        }
    }

    private void enableRot()
    {

    }

    private void enableDel()
    {
        Transform cont = editScreenGO.transform.Find("ScrollView").Find("Viewport").Find("Content");
        Transform delBtn = editScreenGO.transform.Find("DelBtn");
        ScrollRect scroll = editScreenGO.transform.Find("ScrollView").GetComponent<ScrollRect>();
        scroll.horizontal = false;
        foreach (Transform child in cont)
        {
            if (child.gameObject == activeObj)
            {
                delBtn.position = child.position;
                delBtn.gameObject.SetActive(true);
                delActive = true;
            }
        }
    }

    private void disableDel()
    {
        Transform delBtn = editScreenGO.transform.Find("DelBtn");
        ScrollRect scroll = editScreenGO.transform.Find("ScrollView").GetComponent<ScrollRect>();
        scroll.horizontal = true;
        delBtn.gameObject.SetActive(false);
        delActive = false;
    }

    private void removeMarkers()
    {
        foreach (Transform markerCube in marker.transform)
        {
            if (markerCube.position != baseCube.position)
            {
                Destroy(markerCube.gameObject);
            }
        }
    }

    private void addMarkers()
    {
        int deltaXSign = Mathf.RoundToInt(Mathf.Sign(deltaX));
        int deltaZSign = Mathf.RoundToInt(Mathf.Sign(deltaZ));

        removeMarkers();

        for (int i = 0; i <= zMag; i++)
        {
            for (int j = 0; j <= xMag; j++)
            {
                int xVal = j * deltaXSign;
                int zVal = i * deltaZSign;
                Vector3 thisPos = baseCube.position + new Vector3(xVal, 0, zVal);
                if (baseCube.position != thisPos)
                {
                    GameObject markerCube = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/MarkerCube")) as GameObject;
                    markerCube.transform.position = thisPos;
                    markerCube.transform.parent = marker.transform;
                }
            }
        }
    }

    private void addMarkers3D()
    {
        int deltaXSign = Mathf.RoundToInt(Mathf.Sign(deltaX));
        int deltaZSign = Mathf.RoundToInt(Mathf.Sign(deltaZ));

        removeMarkers();

        for (int i = 0; i <= zMag; i++)
        {
            for (int j = 0; j <= xMag; j++)
            {
                for (int k = 0; k <= xMag; k++)
                {
                    int xVal = j * deltaXSign;
                    int yVal = i * deltaZSign;
                    int zVal = k * deltaXSign;
                    Vector3 thisPos = baseCube.position + new Vector3(xVal, yVal, zVal);
                    if (baseCube.position != thisPos)
                    {
                        GameObject markerCube = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/MarkerCube")) as GameObject;
                        markerCube.transform.position = thisPos;
                        markerCube.transform.parent = marker.transform;
                    }
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

<<<<<<< HEAD
    public void ScrapLevel()
    {
        stopEdit();
        foreach (Transform child in objects)
=======
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
>>>>>>> main
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

    public void LoadLevel(string name)
    {
        thisLevelName = name;
        Level lvlToLoad;
        Game game = Game.instance;
        if (!game.testPlaying)
        {
            Edit();
        }
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

    public void AddWorldItem(string name)
    {
        foreach (Transform markerCube in marker.transform)
        {
            var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
            obj.name = name;
            obj.transform.SetParent(Game.instance.level.world);
            obj.transform.position = markerCube.position.Round();
            obj.transform.rotation = markerCube.rotation;
            Slide(lastDir);
        }
    }

    public void AddObjectItem(string name)
    {
<<<<<<< HEAD
        foreach (Transform markerCube in marker.transform)
        {
            var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
            obj.name = name;
            obj.transform.SetParent(objects);
            obj.transform.position = markerCube.position.Round();
            obj.transform.rotation = markerCube.rotation;
            Slide(lastDir);
        }
=======
        var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
        obj.name = name;
        obj.transform.SetParent(world);
        obj.transform.position = marker.transform.position.Round();
        obj.transform.rotation = marker.transform.rotation;
        Slide(lastDir);
>>>>>>> main
    }

    public void RemoveItem()
    {
<<<<<<< HEAD
        string add = "Add";
        string theName = activeObj.name.Replace(add, "");
        foreach (Transform markerCube in marker.transform)
        {
            foreach (Transform child in objects)
            {
                
                if (child.position == markerCube.position && child.gameObject.name == theName)
                    Destroy(child.gameObject);
            }
            foreach (Transform child in world)
            {
                if (child.position == markerCube.position && child.gameObject.name == theName)
                    Destroy(child.gameObject);
            }
        }
=======
        var obj = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/" + name)) as GameObject;
        obj.name = name;
        obj.transform.SetParent(objectParent);
        obj.transform.position = marker.transform.position.Round();
        obj.transform.rotation = marker.transform.rotation;
        Slide(lastDir);
>>>>>>> main
    }

    public void stopEdit()
    {
        editing = false;
        marker.SetActive(false);
    }

    public void Edit()
    {
        editing = true;
        if (marker)
            marker.SetActive(true);
    }
<<<<<<< HEAD

    public void SelectToggle()
    {
        selectMode = !selectMode;
        GameObject toggle3D = editScreenGO.transform.Find("3DSelectToggle").gameObject;
        if (selectMode)
        {
            toggle3D.SetActive(true);
        }
        else
        {
            toggle3D.SetActive(false);
        }
    }

    public void selectToggle3D()
    {
        selectMode3D = !selectMode3D;
    }
=======
#endif
>>>>>>> main
}

