using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class MoveRotateSwitch : MonoBehaviour
{
    private bool placedObject => LevelEditor.instance ? LevelEditor.instance.placedObject : false;
    private Vector3 unconfirmedCenter => LevelEditor.instance ? LevelEditor.instance.unconfirmedCenter : Vector3.zero;
    private bool rotateMode => LevelEditor.instance ? LevelEditor.instance.rotateMode : false;
    private Transform switchButton;
    private GameObject moveOn;
    private GameObject rotateOn;
    private bool switchButtonAnimation { get; set; }
    private const float cooldown = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        switchButton = transform.Find("SwitchButton");
        moveOn = transform.Find("MoveOn").gameObject;
        rotateOn = transform.Find("RotateOn").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (placedObject)
        {
            Vector3 position = unconfirmedCenter + new Vector3(0f, 0.5f, 0f);
            Vector2 worldToScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
            gameObject.transform.position = worldToScreenPosition + new Vector2(0f, -200f);
        }
    }

    public void AnimateSwitch(bool rotateMode)
    {
        switchButtonAnimation = true;
        var multiplier = 1;
        GameObject deactivate;
        GameObject activate;
        if (rotateMode)
        {
            deactivate = moveOn;
            activate = rotateOn;
        }
        else
        {
            multiplier = -1;
            deactivate = rotateOn;
            activate = moveOn;
        }
        deactivate.SetActive(false);
        Tween.LocalPosition(switchButton, new Vector3(54f * multiplier, 0, 0), 0.15f, 0.0f, completeCallback: () => OnSwitchButtonAnimationDone(activate));
    }

    private void OnSwitchButtonAnimationDone(GameObject activate)
    {
        activate.SetActive(true);
        Invoke("EndSwitchButtonAnimation", cooldown);
    }

    private void EndSwitchButtonAnimation()
    {

        switchButtonAnimation = false;
    }
}
