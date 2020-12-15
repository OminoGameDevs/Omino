using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reflector : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.position = transform.parent.position + Vector3.up;
    }
}
