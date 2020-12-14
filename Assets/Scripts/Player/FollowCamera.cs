using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offset;

    private bool editAngleSet = false;

    private void Update()
    {
        Camera camera = GetComponent<Camera>();
        Vector3 v = default(Vector3);
        if (Game.instance && Game.instance.level)
        {
            if (IGLvlEditor.instance && IGLvlEditor.instance.editing)
            {
                camera.orthographic = true;
                if (IGLvlEditor.instance.marker != null)
                {
                    if (!editAngleSet)
                    {
                        editAngleSet = true;
                        offset.x = -10;
                        offset.y = 10;
                        offset.z = -10;
                        transform.position = Vector3.SmoothDamp(transform.position, IGLvlEditor.instance.markerCenter + offset, ref v, 0.1f);
                        var markers = GameObject.FindGameObjectsWithTag("Marker");
                        if (markers.Length > 0)
                        {
                            Vector3 target = IGLvlEditor.instance.markerCenter.Round();
                            transform.position = target + offset;
                            transform.LookAt(target, Vector3.up);
                        }
                    }
                    transform.position = Vector3.SmoothDamp(transform.position, IGLvlEditor.instance.markerCenter + offset, ref v, 0.1f);
                }
            }
            else
            {
                if (editAngleSet)
                {
                    editAngleSet = false;
                    offset.x = -10;
                    offset.y = 10;
                    offset.z = -10;
                    var omino = FindObjectOfType<Omino>();
                    if (omino)
                    {
                        Vector3 target = omino.center.Round();
                        transform.position = target + offset;
                        transform.LookAt(target, Vector3.up);
                    }
                }
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
                Vector3 target = IGLvlEditor.instance.markerCenter.Round();
                transform.position = target + offset;
                transform.LookAt(target, Vector3.up);
            }
        }
    }
}
