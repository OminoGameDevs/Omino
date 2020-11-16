using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AutoSnap : MonoBehaviour
{
    private void Update()
    {
        if (Application.isPlaying)
            return;

        foreach (Transform child in transform)
            child.localPosition = child.localPosition.Round();
    }
}