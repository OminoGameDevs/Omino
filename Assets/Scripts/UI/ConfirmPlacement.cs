using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmPlacement : MonoBehaviour
{
    private bool placedObject => LevelEditor.instance ? LevelEditor.instance.placedObject : false;
    private Vector3 unconfirmedCenter => LevelEditor.instance ? LevelEditor.instance.unconfirmedCenter : Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        if (placedObject)
        {
            Vector3 position = unconfirmedCenter + new Vector3(0f, 0.5f, 0f);
            Vector2 worldToScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            gameObject.transform.position = worldToScreenPosition + new Vector2(50f, 0f);
        }
    }
}
