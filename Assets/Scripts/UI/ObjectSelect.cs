using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSelect : MonoBehaviour
    , IPointerUpHandler
    , IPointerDownHandler
{
    Vector2 dragStartPosition;
    public string objectName;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        dragStartPosition = eventData.position;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - dragStartPosition;
        if (delta.y > 8)
        {
            Debug.Log(delta.y);
        }
        if (delta.y < -8)
        {
            Debug.Log(delta.y);
        }
    }
}
