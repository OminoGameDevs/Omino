using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using Pixelplacement;
using Unity.VectorGraphics;

public class LevelEditor : MonoBehaviour
{
    // Instance variables
    public static LevelEditor instance { get; private set; }

    // Scene variables
    public Level thisLevel;
    private string thisLevelName;
    private static int editLevelNumber;
    private new Camera camera;
    public GameObject followReference;
    private static int newLevelIndex;
    public GameObject grid;
    public int gridSize = 10;
    private static Transform levelObjects => Game.instance.level ? Game.instance.level.levelObjects : null;
    private static Transform world => Game.instance.level ? Game.instance.level.world : null;
    private static Transform editorObjects => Game.instance.level ? Game.instance.level.editorObjects : null;
    private GameObject movableObjects;
    public Vector3 movableObjectsCenter { get; private set; }
    public GameObject unconfirmedObjects;
    private GameObject markers;
    public GameObject activeObject;
    public string activeObjectName;
    private int verticalAdjustment;
    public int altitude = -1;
    private bool cameraMoved;
    private const float cooldown = 0.05f;
    public bool sliding { get; set; }
    public bool slidingUi { get; set; }
    public bool switchButtonAnimation { get; set; }
    public Vector3 selectionMovement = Vector3.zero;
    private Vector3 selectionMoveDirection = Vector3.zero;
    private Vector3 selectionDirection;
    private int maxAltitude = 4;
    private int minAltitude = -6;

    private List<GameObject> objectsRemovedFromSelection;

    public List<Transform> unconfirmedSwitches = new List<Transform>();
    public List<Transform> unconfirmedDoors = new List<Transform>();

    private int placementCount = 0;

    public Vector3 unconfirmedCenter
    {
        get
        {
            if (!unconfirmedObjects || unconfirmedObjects.transform.childCount == 0)
                return followReference.transform.position;

            Vector3 result = Vector3.zero;
            foreach (Transform obj in unconfirmedObjects.transform)
                result += obj.position;
            return result / unconfirmedObjects.transform.childCount;
        }
    }

    public float forwardMostUnconfirmed
    {
        get
        {
            if (!unconfirmedObjects || unconfirmedObjects.transform.childCount == 0)
                return 0f;

            float result = -10f;
            foreach (Transform obj in unconfirmedObjects.transform)
            {
                if (obj.position.z > result)
                    result = obj.position.z;
            }
            return result;
        }
    }

    public float backMostUnconfirmed
    {
        get
        {
            if (!unconfirmedObjects || unconfirmedObjects.transform.childCount == 0)
                return 0f;

            float result = 10f;
            foreach (Transform obj in unconfirmedObjects.transform)
            {
                if (obj.position.z < result)
                    result = obj.position.z;
            }
            return result;
        }
    }

    public float rightMostUnconfirmed
    {
        get
        {
            if (!unconfirmedObjects || unconfirmedObjects.transform.childCount == 0)
                return 0f;

            float result = -10f;
            foreach (Transform obj in unconfirmedObjects.transform)
            {
                if (obj.position.x > result)
                    result = obj.position.x;
            }
            return result;
        }
    }

    public float leftMostUnconfirmed
    {
        get
        {
            if (!unconfirmedObjects || unconfirmedObjects.transform.childCount == 0)
                return 0f;

            float result = 10f;
            foreach (Transform obj in unconfirmedObjects.transform)
            {
                if (obj.position.x < result)
                    result = obj.position.x;
            }
            return result;
        }
    }

    // Game state variables
    public bool editing;

    // Touch state variables
    private bool touching;
    private bool touchingUI;
    public Vector3 firstTouchPosition;
    public Vector3 delta;
    public bool placedObject;
    public bool newPlacement;


    public struct nullableVectors
    {
        public Vector3? lastDelta;
    }
    private nullableVectors[] nv = new nullableVectors[1];

    // UI variables
    public GameObject editScreenGameObject { get; private set; }
    private GameObject placingDisplay;
    public bool rotateMode { get; private set; }
    Transform switchButton;
    Transform moveRotateSwitch;
    GameObject moveOn;
    GameObject rotateOn;
    GameObject switchOptions;
    GameObject doorOptions;

    void Start()
    {
        camera = Game.instance.transform.Find("Camera").GetComponent<Camera>();
    }

    void Update()
    {
        if (editing)
        {
            cameraMoved = true;
            if (!instance)
            {
                instance = this;
                editScreenGameObject = GameObject.Find("EditScreen");
                placingDisplay = editScreenGameObject.transform.Find("PlacingDisplay").gameObject;
                switchOptions = editScreenGameObject.transform.Find("SwitchOptions").gameObject;
                doorOptions = editScreenGameObject.transform.Find("DoorOptions").gameObject;

                movableObjects = new GameObject("MovableObjects");
                movableObjects.transform.parent = editorObjects;

                markers = new GameObject("Markers");
                markers.transform.parent = movableObjects.transform;

                unconfirmedObjects = new GameObject("UnconfirmedObjects");
                unconfirmedObjects.transform.parent = movableObjects.transform;

                followReference = new GameObject("FollowReference");
                followReference.transform.position = new Vector3(-2, 0, -1);
                followReference.transform.parent = editorObjects;

                switchButton = placingDisplay.transform.Find("MoveRotateSwitch").Find("SwitchButton");
                moveRotateSwitch = placingDisplay.transform.Find("MoveRotateSwitch");
                moveOn = placingDisplay.transform.Find("MoveRotateSwitch").Find("MoveOn").gameObject;
                rotateOn = placingDisplay.transform.Find("MoveRotateSwitch").Find("RotateOn").gameObject;

                applyGrid();
            }

            getEditInput();

            if (touching)
            {
                if (delta != nv[0].lastDelta || nv[0].lastDelta == null)
                {
                    if (activeObjectName == "")
                        freeMoveCamera(-delta);
                    else if (activeObjectName == "Marker")
                        addMarkers();
                    else
                        addObjects();
                    nv[0].lastDelta = delta;
                }
            }
            else
            {
                if (placedObject)
                {
                    if (!placingDisplay.activeSelf && !switchOptions.activeSelf && !doorOptions.activeSelf)
                    {
                        selectionDirection = Vector3.up;
                        if (activeObjectName == "Marker")
                            placeMarkers();
                        else
                            placeObjects();
                        if (placementCount > 0)
                        {
                            placingDisplay.SetActive(true);
                            if (rotateMode)
                            {
                                rotateOn.SetActive(true);
                                moveOn.SetActive(false);
                            }
                            else
                            {
                                rotateOn.SetActive(false);
                                moveOn.SetActive(true);
                            }
                        }
                        else placedObject = false;
                    }
                    if (!cameraMoved)
                    {
                        moveCameraTo(unconfirmedCenter);
                        cameraMoved = true;
                    }
                }
                else
                {
                    placingDisplay.SetActive(false);
                }
            }
        }
    }

    private void getEditInput()
    {
        if (Input.GetMouseButton(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            if (results.Count > 1 && results[1].gameObject.layer == 5)
                touchingUI = true;  
            else
                touchingUI = false;
            if (!touchingUI && !placedObject)
            {
                foreach (RaycastHit hit in Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition)))
                {
                    if (hit.collider.gameObject.name == "GridSquare")
                    {
                        if (!touching)
                        {
                            touching = true;
                            firstTouchPosition = hit.collider.gameObject.transform.position;
                            selectionMovement = Vector3.zero;
                            movableObjects.transform.position = Vector3.zero;
                        }
                        delta = hit.point - firstTouchPosition;
                        delta.x = Mathf.RoundToInt(delta.x);
                        delta.z = Mathf.RoundToInt(delta.z);
                    }
                }
            }
        }
        else
        {
            if (touching)
            {
                if (activeObjectName != "")
                {
                    objectsRemovedFromSelection = new List<GameObject>();
                    newPlacement = true;
                    placedObject = true;
                    cameraMoved = false;
                }
            }
            touching = false;
            touchingUI = false;
            nv[0].lastDelta = null;
        }
    }

    // Help functions
    public void setEditing(bool isEditing)
    {
        editing = isEditing;
    }

    // Create scene
    private void applyGrid()
    {
        grid = new GameObject("Grid");
        grid.transform.parent = editorObjects;
        for (float j = 0; j <= gridSize; j++)
        {
            for (float i = 0; i <= gridSize; i++)
            {
                int thisX = Mathf.RoundToInt(j - gridSize / 2);
                int thisZ = Mathf.RoundToInt(i - gridSize / 2);
                Vector3 thisPos = new Vector3(thisX, altitude, thisZ);
                GameObject gridSquare = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/GridSquare")) as GameObject;
                gridSquare.transform.position = thisPos;
                gridSquare.transform.parent = grid.transform;
            }
        }
    }

    //Edit scene
    private void addMarkers()
    {
        removeMarkers();
        for (int x = 0; x <= Mathf.Abs(delta.x); x++)
        {
            for (int z = 0; z <= Mathf.Abs(delta.z); z++)
            {
                GameObject marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Objects/Marker")) as GameObject;
                marker.transform.position = firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), 0, z * Mathf.Sign(delta.z));
                marker.transform.parent = markers.transform;
            }
        }
    }

    private void addObjects()
    {
        removeUnconfirmedObjects();

        string prefix = "Prefabs/Objects/";

        if (activeObjectName == "Omino")
        {
            prefix = "Prefabs/Characters/";
        }

        for (int x = 0; x <= Mathf.Abs(delta.x); x++)
        {
            for (int z = 0; z <= Mathf.Abs(delta.z); z++)
            {

                var occupiedSpot = false;
                foreach (Transform obj in world.transform)
                {
                    if (obj.position == firstTouchPosition + new Vector3(x * Mathf.Sign(delta.x), 0f, z * Mathf.Sign(delta.z)))
                        occupiedSpot = true;
                }
                foreach (Transform obj in levelObjects.transform)
                {
                    if (obj.position == firstTouchPosition + new Vector3(x * Mathf.Sign(delta.x), 0f, z * Mathf.Sign(delta.z)))
                        occupiedSpot = true;
                }
                if (!occupiedSpot)
                {
                    GameObject objectToAdd = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>(prefix + activeObjectName)) as GameObject;
                    objectToAdd.transform.position = firstTouchPosition + new Vector3(x * Mathf.Sign(delta.x), 0f, z * Mathf.Sign(delta.z));
                    objectToAdd.transform.parent = unconfirmedObjects.transform;
                }
            }
        }
    }

    private void updateObjects()
    {
        removeUnconfirmedObjects();
        removeMarkers();

        string prefix = "Prefabs/Objects/";

        if (activeObjectName == "Omino")
        {
            prefix = "Prefabs/Characters/";
        }
        if (activeObjectName == "Marker")
        {
            for (int x = 0; x <= Mathf.Abs(delta.x); x++)
            {
                for (int z = 0; z <= Mathf.Abs(delta.z); z++)
                {
                    foreach (Transform obj in world.transform)
                    {
                        if (obj.position == firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), 0f, z * Mathf.Sign(delta.z)))
                        {
                            obj.parent = unconfirmedObjects.transform;
                            GameObject marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Objects/Marker")) as GameObject;
                            marker.transform.position = firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), 0, z * Mathf.Sign(delta.z));
                            marker.transform.parent = markers.transform;
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x <= Mathf.Abs(delta.x); x++)
            {
                for (int z = 0; z <= Mathf.Abs(delta.z); z++)
                {
                    var occupiedSpot = false;
                    foreach (Transform obj in world.transform)
                    {

                    }
                    if (!occupiedSpot)
                    {
                        GameObject objectToAdd = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>(prefix + activeObjectName)) as GameObject;
                        objectToAdd.transform.position = firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), verticalAdjustment, z * Mathf.Sign(delta.z));
                        objectToAdd.transform.eulerAngles = selectionDirection;
                        objectToAdd.transform.parent = unconfirmedObjects.transform;
                        GameObject marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Objects/Marker")) as GameObject;
                        marker.transform.position = firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), 0, z * Mathf.Sign(delta.z));
                        marker.transform.parent = markers.transform;
                    }
                }
            }
        }
    }

    private void updateOnMoveObjects()
    {
        string prefix = "Prefabs/Objects/";

        if (activeObjectName == "Omino")
        {
            prefix = "Prefabs/Characters/";
        }
        for (int x = 0; x <= Mathf.Abs(delta.x); x++)
        {
            for (int z = 0; z <= Mathf.Abs(delta.z); z++)
            {
                var occupiedSpot = false;
                var selectionPosition = firstTouchPosition + selectionMovement + new Vector3(x * Mathf.Sign(delta.x), 0f, z * Mathf.Sign(delta.z));
                foreach (Transform obj in world.transform)
                {
                    if (obj.position == selectionPosition)
                    {
                        occupiedSpot = true;
                        break;
                    }
                }
                if (!occupiedSpot)
                {
                    var reActivate = true;
                    foreach (Transform obj in unconfirmedObjects.transform)
                    {
                        if (obj.gameObject.activeSelf)
                        {
                            if (obj.position == selectionPosition)
                            {
                                reActivate = false;
                                break;
                            }
                        }
                    }
                    if (reActivate)
                    {
                        foreach (Transform marker in markers.transform)
                        {
                            if (marker.position == selectionPosition)
                            {
                                marker.gameObject.SetActive(true);
                                break;
                            }
                        }
                        foreach (GameObject obj in objectsRemovedFromSelection)
                        {
                            if (selectionPosition == obj.transform.position)
                            {
                                obj.SetActive(true);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Transform obj in unconfirmedObjects.transform)
                    {
                        if (obj.position == selectionPosition)
                        {
                            objectsRemovedFromSelection.Add(obj.gameObject);
                            obj.gameObject.SetActive(false);
                            break;
                        }
                    }
                    foreach (Transform marker in markers.transform)
                    {
                        if (marker.position == selectionPosition)
                        {
                            marker.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void placeMarkers()
    {
        List<Transform> listOfMarkers = new List<Transform>();
        foreach (Transform marker in markers.transform)
        {
            var matchAny = false;
            foreach (Transform obj in world.transform)
            {
                if (marker.position == obj.position)
                {
                    matchAny = true;
                    obj.parent = unconfirmedObjects.transform;
                }
            }
            foreach (Transform obj in levelObjects.transform)
            {
                if (marker.position == obj.position)
                {
                    matchAny = true;
                    obj.parent = unconfirmedObjects.transform;
                }
            }
            if (!matchAny)
                Destroy(marker.gameObject);
            else
            {
                listOfMarkers.Add(marker);
            }
            //SetExtremePositions(marker);
        }
        placementCount = listOfMarkers.Count;
        followReference.transform.position = unconfirmedCenter;
        // setExtremePositions(listOfMarkers);
    }

    private void placeObjects()
    {
        var count = 0;
        foreach (Transform obj in unconfirmedObjects.transform)
        {
            GameObject marker = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/Objects/Marker")) as GameObject;
            marker.transform.position = obj.position;
            marker.transform.parent = markers.transform;
            count++;
        }
        placementCount = count;
    }

    public void moveSelection(Vector3 direction)
    {
        selectionMoveDirection = direction;
        selectionMovement += direction;
        SlideObject(direction, movableObjects);
    }

    private void SlideObject(Vector3 dir, GameObject objectToSlide)
    {
        sliding = true;
        Tween.Position(objectToSlide.transform, objectToSlide.transform.position + dir, Constants.transitionTime, 0.0f, completeCallback: () => OnSlideObjectDone(objectToSlide));
    }

    private void OnSlideObjectDone(GameObject objectToSlide)
    {
        objectToSlide.transform.position = objectToSlide.transform.position.Round();
        moveCameraToward(selectionMoveDirection);
        updateOnMoveObjects();
        Invoke("EndSlideObject", cooldown);
    }


    private void EndSlideObject()
    {
        sliding = false;
    }

    public void RotateSelection(Vector3 direction)
    {
        foreach (Transform obj in unconfirmedObjects.transform)
            RotateObject(direction, obj);
    }

    private void RotateObject(Vector3 direction, Transform objectToRotate)
    {
        if (objectToRotate.name == "Switch" || objectToRotate.name == "Adhesive" || objectToRotate.name == "Door")
        {
            Vector3 euler = new Vector3(direction.z, 0f, -direction.x);
            Tween.Rotate(
                target: objectToRotate,
                space: Space.World,
                amount: euler * 90f,
                duration: Constants.transitionTime,
                delay: 0f,
                easeCurve: Tween.EaseIn,
                completeCallback: () => OnRotateDone(objectToRotate)
                );
        }
        else if (objectToRotate.name == "Conveyor")
        {
            Vector3 euler = new Vector3(0f, (direction.z - Mathf.Abs(direction.z)) * 90f + direction.x * 90f, 0f);
            Tween.Rotation(
            target: objectToRotate,
            endValue: euler,
            duration: Constants.transitionTime,
            delay: 0f,
            easeCurve: Tween.EaseIn,
            completeCallback: () => OnRotateDone(objectToRotate)
            );
        }
        // TODO: make this work
        else if (objectToRotate.name == "Omino")
        {
            Vector3 euler = new Vector3(0f, (direction.z - Mathf.Abs(direction.z)) * 90f + direction.x * 90f, 0f);
            Tween.Rotation(
            target: objectToRotate,
            endValue: euler,
            duration: Constants.transitionTime,
            delay: 0f,
            easeCurve: Tween.EaseIn,
            completeCallback: () => OnRotateDone(objectToRotate)
            );
            Game.instance.level.omino.direction = direction;
        }
    }

    private void OnRotateDone(Transform objectToRotate)
    {
        selectionDirection = objectToRotate.eulerAngles;
    }

    private void SwitchAltitude(Vector3 dir)
    {
        sliding = true;
        Tween.Position(editorObjects, editorObjects.position + dir, Constants.transitionTime, 0.0f, completeCallback: () => OnSwitchAltitudeDone(dir));
    }

    private void OnSwitchAltitudeDone(Vector3 dir)
    {
        editorObjects.position = editorObjects.position.Round();
        moveCameraToward(dir);
        Invoke("EndSwitchAltitude", cooldown);
    }

    private void EndSwitchAltitude()
    {
        sliding = false;
    }

    public void removeUnconfirmedObjects()
    {
        foreach (Transform obj in unconfirmedObjects.transform)
        {
            Destroy(obj.gameObject);
        }
    }

    public void removeMarkers()
    {
        foreach (Transform obj in markers.transform)
        {
            Destroy(obj.gameObject);
        }
    }

    public void placeUnconfirmedObjects()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform obj in unconfirmedObjects.transform)
        {
            if (obj.gameObject.activeSelf)
                children.Add(obj);
            else Destroy(obj.gameObject);
        }
        foreach (Transform obj in children)
        {
            if (obj.gameObject.name == "Wall")
                obj.parent = world.transform;
            else obj.parent = levelObjects.transform;
        }
    }

    private void moveCameraTo(Vector3 position)
    {
        followReference.transform.position = position;
    }

    private void moveCameraToward(Vector3 direction)
    {
        followReference.transform.position += direction;
    }

    private void freeMoveCamera(Vector3 direction)
    {
        float followX = followReference.transform.position.x + Mathf.Round(direction.x);
        float followZ = followReference.transform.position.z + Mathf.Round(direction.z);

        float adjustedFollowX = Mathf.Sign(followX) < 0 ? Mathf.Max(followX, gridSize * -1 / 2) : Mathf.Min(followX, gridSize / 2);
        float adjustedFollowZ = Mathf.Sign(followZ) < 0 ? Mathf.Max(followZ, gridSize * -1 / 2) : Mathf.Min(followZ, gridSize / 2);
        followReference.transform.position = new Vector3(adjustedFollowX, 0f, adjustedFollowZ);
    }

    // UI functions
    public void LoadLevel(string name)
    {
        thisLevelName = name;
        Game game = Game.instance;
        if (!game.testPlaying)
        {
            setEditing(true);
        }
        if (game.level)
            Destroy(game.level.gameObject);
        var lvlToLoad = PrefabUtility.InstantiatePrefab(ResourceLoader.Get<GameObject>("Prefabs/LevelTemplate")) as GameObject;
        game.level = Instantiate(lvlToLoad.GetComponent<Level>());
        Destroy(lvlToLoad);
        thisLevel = game.level;
        game.level.transform.SetParent(game.transform);

    }

    public void ScrapLevel()
    {
        setEditing(false);
        foreach (Transform child in levelObjects)
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

    public void setActiveObject(string objectToActivate)
    {
        if (activeObjectName != objectToActivate)
        {
            if (activeObjectName != "")
            {
                Sprite deselectedSprite = ResourceLoader.Get<Sprite>("Sprites/" + activeObjectName + "Unselected");
                GameObject.Find("Add" + activeObjectName).GetComponent<SVGImage>().sprite = deselectedSprite;
                GameObject.Find("Add" + activeObjectName).GetComponentInChildren<Text>().color = new Color(255, 255, 255);
            }
            activeObjectName = objectToActivate;
            Sprite selectedSprite = ResourceLoader.Get<Sprite>("Sprites/" + objectToActivate + "Selected");
            GameObject.Find("Add" + objectToActivate).GetComponent<SVGImage>().sprite = selectedSprite;
            GameObject.Find("Add" + objectToActivate).GetComponentInChildren<Text>().color = new Color(0,0,0);
        }
        else
        {
            Sprite deselectedSprite = ResourceLoader.Get<Sprite>("Sprites/" + objectToActivate + "Unselected");
            GameObject.Find("Add" + objectToActivate).GetComponent<SVGImage>().sprite = deselectedSprite;
            GameObject.Find("Add" + objectToActivate).GetComponentInChildren<Text>().color = new Color(255, 255, 255);
            activeObjectName = "";
        }
    }

    public void goUp()
    {
        if (altitude < maxAltitude)
        {
            if (altitude == minAltitude)
            {
                GameObject.Find("DownBtn").GetComponent<Button>().interactable = true;
            }
            SwitchAltitude(Vector3.up);
            altitude++;
            if (altitude == maxAltitude)
            {
                GameObject.Find("UpBtn").GetComponent<Button>().interactable = false;
            }
         }
    }

    public void goDown()
    {
        if (altitude > minAltitude)
        {
            if (altitude == maxAltitude)
            {
                GameObject.Find("UpBtn").GetComponent<Button>().interactable = true;
            }
            SwitchAltitude(Vector3.down);
            altitude--;
            if (altitude == minAltitude)
            {
                GameObject.Find("DownBtn").GetComponent<Button>().interactable = false;
            }
        }
    }

    public void SetRotateMode()
    {
        rotateMode = !rotateMode;
        moveRotateSwitch.GetComponent<MoveRotateSwitch>().AnimateSwitch(rotateMode);
    }

    private void OnSwitchButtonAnimationDone(GameObject activate)
    {
        activate.SetActive(true);
        Invoke("EndSwitchButtonAnimation", cooldown);
    }

    private void EndSwitchButtonAnimation()
    {

        switchButtonAnimation = false;
    }

    public void ExtraStepsOrConfirm()
    {
        unconfirmedSwitches.Clear();
        unconfirmedDoors.Clear();
        foreach (Transform obj in unconfirmedObjects.transform)
        {
            if (obj.gameObject.name == "Switch")
            {
                unconfirmedSwitches.Add(obj);
            }
            else if (obj.gameObject.name == "Door")
            {
                unconfirmedDoors.Add(obj);
            }
        }
        if (unconfirmedSwitches.Count > 0)
            ShowSwitchOptions();
        else if (unconfirmedDoors.Count > 0)
            ShowDoorOptions();
        else
        {
            placingDisplay.SetActive(false);
            ConfirmPlacement();
        }
    }

    public void SwitchOptionsConfirm()
    {
        if (unconfirmedDoors.Count > 0)
            ShowDoorOptions();
        else
        {
            placingDisplay.SetActive(false);
            ConfirmPlacement();
        }
    }

    public void ConfirmPlacement()
    {
        placeUnconfirmedObjects();
        removeMarkers();
        placedObject = false;
    }

    public void CancelPlacement()
    {
        removeUnconfirmedObjects();
        removeMarkers();
        placingDisplay.SetActive(false);
        placedObject = false;
    }

    public void RemovePlacement()
    {
        removeUnconfirmedObjects();
        removeMarkers();
        placedObject = false;
    }

    public void TestPlay()
    {
        editing = false;
        foreach (Transform obj in Game.instance.level.levelObjects)
        {
            if (obj.gameObject.name == "Switch")
            {
                obj.GetComponent<Switch>().UpdateSwitchSettings();
            }
        }
        foreach (Transform obj in Game.instance.level.levelObjects)
        {
            if (obj.gameObject.name == "Door")
            {
                obj.GetComponent<Door>().UpdateDoorSettings();
            }
        }
        editorObjects.gameObject.SetActive(false);
        Game.instance.TestPlay();
    }

    public void Edit()
    {
        editing = true;
        editorObjects.gameObject.SetActive(true);
    }

    public void ShowSwitchOptions()
    {
        placingDisplay.SetActive(false);
        switchOptions.SetActive(true);
    }

    public void ShowDoorOptions()
    {
        placingDisplay.SetActive(false);
        doorOptions.SetActive(true);
    }
}
