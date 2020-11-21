using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HideChildren : MonoBehaviour
{
    private void OnEnable()
    {
        foreach (Transform child in transform)
            child.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }

    private void OnDisable()
    {
        foreach (Transform child in transform)
            child.gameObject.hideFlags = HideFlags.None;
    }
}
