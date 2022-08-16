using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacingDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    private bool placedObject => LevelEditor.instance ? LevelEditor.instance.placedObject : false;
    private Vector3 unconfirmedCenter => LevelEditor.instance ? LevelEditor.instance.unconfirmedCenter : Vector3.zero;

    void Start()
    {
        //placedObject = LevelEditor.instance.GetComponent<LevelEditor>().placedObject;
    }

    private void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (placedObject)
        {
            // var worldToScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, unconfirmedCenter + new Vector3(0f, 0f, 0f));
            // gameObject.transform.position = worldToScreenPosition;
        }
        else
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

    public void RemovePlacement()
    {
        gameObject.SetActive(false);
        LevelEditor.instance.RemovePlacement();
    }
}
