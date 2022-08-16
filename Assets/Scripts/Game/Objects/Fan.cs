using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    Vector3 rotation = new Vector3(0, 0, 10f);

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Rotate(rotation, Space.Self);
    }
}
