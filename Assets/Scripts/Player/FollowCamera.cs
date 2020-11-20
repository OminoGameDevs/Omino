using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        if (Omino.instance)
        {
            Vector3 v = default(Vector3);
            transform.position = Vector3.SmoothDamp(transform.position, Omino.instance.center + offset, ref v, 0.1f);
        }
        else
        {
            var omino = FindObjectOfType<Omino>();
            if (omino)
            {
                Vector3 target = omino.center;
                transform.position = target + offset;
                transform.LookAt(target, Vector3.up);
            }
        }
    }
}
