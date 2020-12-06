using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        Camera camera = GetComponent<Camera>();
        Vector3 v = default(Vector3);
        if (Game.instance && Game.instance.level)
        {
            if (IGLvlEditor.instance && IGLvlEditor.instance.editing)
            {
                camera.orthographic = true;
                transform.position = Vector3.SmoothDamp(transform.position, IGLvlEditor.instance.marker.transform.position + offset, ref v, 0.1f);
            }
            else
            {
                camera.orthographic = false;
                transform.position = Vector3.SmoothDamp(transform.position, Game.instance.level.omino.center + offset, ref v, 0.1f);
            }
        }
        else
        {
            var omino = FindObjectOfType<Omino>();
            if (omino)
            {
                Vector3 target = omino.center.Round();
                transform.position = target + offset;
                transform.LookAt(target, Vector3.up);
            }
            var markers = GameObject.FindGameObjectsWithTag("Marker");
            GameObject marker;
            if (markers.Length > 0)
            {
                camera.orthographic = true;
                marker = markers[0];
                Vector3 target = marker.transform.position.Round();
                transform.position = target + offset;
                transform.LookAt(target, Vector3.up);
            }
        }
    }
}
