using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[Orientable]
[RequireComponent(typeof(BoxCollider))]
public class Conveyor : MonoBehaviour
{
    private BoxCollider trigger;

    private void Awake()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnOminoEnter(Omino.CubeStack stack)
    {
        Omino.instance.Slide(transform.forward);
    }
}
