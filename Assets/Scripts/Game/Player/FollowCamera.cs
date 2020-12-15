using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    private static readonly float dropDistance = 50;
    private static readonly Vector3 offset = new Vector3(-10, 10, -10);

    private bool starting;
    private bool ending;

    private void OnLevelStart()
    {
        transform.position = Game.instance.level.omino.bottom + offset + Vector3.up * dropDistance;
        starting = true;
        Tween.Position(
            target:    transform,
            endValue:  Game.instance.level.omino.bottom + offset,
            duration:  Constants.fadeOutTime,
            delay:     0f,
            easeCurve: Tween.EaseOutStrong,
            completeCallback: () => starting = false
        );
    }

    private void OnLevelEnd()
    {
        ending = true;
        Tween.Position(
            target:    transform,
            endValue:  transform.position - Vector3.up * dropDistance,
            duration:  Constants.fadeOutTime,
            delay:     0f,
            easeCurve: Tween.EaseInStrong,
            completeCallback: () => ending = false
        );
    }

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
                    var omino = FindObjectOfType<Omino>();
                    if (omino)
                    {
                        Vector3 target = omino.center.Round();
                        transform.position = target + offset;
                        transform.LookAt(target, Vector3.up);
                    }
                }
                camera.orthographic = false;
                if (!starting && !ending)
                {
                    v = default(Vector3);
                    if (Game.instance && Game.instance.level && Game.instance.level.omino)
                        transform.position = Vector3.SmoothDamp(transform.position, Game.instance.level.omino.bottom + offset, ref v, 0.1f);
                }
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
