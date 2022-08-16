using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSelectionForward : MonoBehaviour
    ,IPointerClickHandler
{
    private bool placedObject => LevelEditor.instance ? LevelEditor.instance.placedObject : false;
    private float forwardMost => LevelEditor.instance ? LevelEditor.instance.forwardMostUnconfirmed : 0f;
    private Vector3 unconfirmedCenter => LevelEditor.instance ? LevelEditor.instance.unconfirmedCenter : Vector3.zero;
    private int altitude => LevelEditor.instance ? LevelEditor.instance.altitude : -1;

    void Update()
    {
        if (placedObject)
        {
            Vector3 position = new Vector3(unconfirmedCenter.x, altitude, Mathf.Max(forwardMost+1f, unconfirmedCenter.z + 2));
            var worldToScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            gameObject.transform.position = worldToScreenPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LevelEditor.instance.rotateMode)
        {
            LevelEditor.instance.RotateSelection(Vector3.forward);
        }
        else
        {
            LevelEditor.instance.moveSelection(Vector3.forward);
        }
    }
}
