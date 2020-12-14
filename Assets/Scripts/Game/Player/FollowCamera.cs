using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class FollowCamera : MonoBehaviour
{
    private static readonly float dropDistance = 50;
    private static readonly Vector3 offset = new Vector3(-10,10,-10);

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

    private void Update()
    {
        if (!starting && !ending)
        {
            Vector3 v = default(Vector3);
            transform.position = Vector3.SmoothDamp(transform.position, Game.instance.level.omino.bottom + offset, ref v, 0.1f);
        }
    }
}
