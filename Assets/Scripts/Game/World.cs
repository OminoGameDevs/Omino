using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class World : MonoBehaviour
{
    private void OnDisable()
    {
        foreach (Transform child in transform)
            child.gameObject.hideFlags = HideFlags.None;
    }

    public Level level {
        get {
            if (!_level)
                _level = transform.parent.GetComponent<Level>();
            return _level;
        }
    }
    private Level _level;

    
}
