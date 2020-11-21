using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class Level : MonoBehaviour
{

    public Omino omino {
        get {
            if (!_omino)
                _omino = objects.GetComponentInChildren<Omino>();
            return _omino;
        }
    }
    private Omino _omino;

#if UNITY_EDITOR

    private Transform objects => transform.Find("Objects");
    private Transform world => transform.Find("World");

    private void Update()
    {
        if (Application.isPlaying) return;

        transform.Reset();
        objects.Reset();
        world.Reset();

        foreach (Transform child in objects)
            child.localPosition = child.localPosition.Round();

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
        }

        var wallMat = ResourceLoader.Get<Material>("Wall");
        wallMat.SetFloat("_yMin", minWallY);
        wallMat.SetFloat("_yMax", maxWallY);
    }

#endif
}
