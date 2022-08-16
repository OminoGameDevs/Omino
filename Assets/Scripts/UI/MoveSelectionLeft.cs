using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSelectionLeft : MonoBehaviour
    , IPointerClickHandler
{
    private bool placedObject => LevelEditor.instance ? LevelEditor.instance.placedObject : false;
    private float leftMost => LevelEditor.instance ? LevelEditor.instance.leftMostUnconfirmed : 0f;
    private Vector3 unconfirmedCenter => LevelEditor.instance ? LevelEditor.instance.unconfirmedCenter : Vector3.zero;
    private int altitude => LevelEditor.instance ? LevelEditor.instance.altitude : -1;

    void Update()
    {
        if (placedObject)
        {
            Vector3 position = new Vector3(Mathf.Min(leftMost-1f, unconfirmedCenter.x - 2f), altitude, unconfirmedCenter.z);
            var worldToScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            gameObject.transform.position = worldToScreenPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LevelEditor.instance)
        {
            if (LevelEditor.instance.rotateMode)
            {
                LevelEditor.instance.RotateSelection(Vector3.left);
            }
            else
            {
                LevelEditor.instance.moveSelection(Vector3.left);
            }
        }
    }
}
