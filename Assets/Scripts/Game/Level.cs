using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

[ExecuteInEditMode]
public class Level : MonoBehaviour
{
    private Transform objectParent => transform.Find("Objects");
    private Transform world => transform.Find("World");

    public Omino omino {
        get {
            if (!_omino)
                _omino = objectParent.GetComponentInChildren<Omino>();
            return _omino;
        }
    }
    private Omino _omino;

    public System.Collections.ObjectModel.ReadOnlyCollection<GameObject> objects => System.Array.AsReadOnly(_objects);
    private GameObject[] _objects;

    public T[] GetObjectsOfType<T>() => (from obj in _objects where obj.GetComponent<T>() != null select obj.GetComponent<T>()).ToArray<T>();

    private void Awake()
    {
        if (!Application.isPlaying) return;

        var objList = new List<GameObject>();
        foreach (Transform t in objectParent)
            objList.Add(t.gameObject);
        _objects = objList.ToArray();
        RefreshWorld();
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (Application.isPlaying) return;

        transform.Reset();
        objectParent.Reset();
        world.Reset();

        foreach (Transform child in transform)
            if (child != world && child != objectParent)
                child.SetParent(child.CompareTag("World") ? world : objectParent);

        foreach (Transform child in objectParent)
        {
            child.localPosition = child.localPosition.Round();
            if (child.CompareTag("World"))
                child.SetParent(world);
        }

        RefreshWorld();
    }

#endif

    private void RefreshWorld()
    {
        float minWallY = float.PositiveInfinity;
        float maxWallY = float.NegativeInfinity;
        foreach (Transform child in world)
        {
            child.localPosition = child.localPosition.Round();
            if (child.CompareTag("World"))
            {
                //child.gameObject.hideFlags = (editWorldDirectly ? HideFlags.None : HideFlags.HideInHierarchy);
                if (child.localPosition.y - 0.5f < minWallY) minWallY = child.localPosition.y - 0.5f;
                if (child.localPosition.y + 0.5f > maxWallY) maxWallY = child.localPosition.y + 0.5f;
            }
            else
                child.SetParent(objectParent);
        }

        var wallMat = ResourceLoader.Get<Material>("Wall");
        wallMat.SetFloat("_yMin", minWallY);
        wallMat.SetFloat("_yMax", maxWallY);
    }
}
