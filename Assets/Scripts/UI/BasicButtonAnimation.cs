using UnityEngine;
using Pixelplacement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasicButtonAnimation : MonoBehaviour
    ,IPointerDownHandler
    ,IPointerUpHandler
{
    // Start is called before the first frame update
    RectTransform tf;
    public Vector2 initalSize;
    void Start()
    {
    }

    private void OnEnable()
    {
        tf = GetComponent<RectTransform>();
        if (gameObject.activeSelf)
            tf.sizeDelta = initalSize;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Tween.Size(
                target: tf,
                endValue: new Vector2(initalSize.x * 1.25f, initalSize.y * 1.25f),
                duration: 0.3f,
                delay: 0f,
                easeCurve: Tween.EaseInOutBack
                );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Tween.Size(
        target: tf,
        endValue: new Vector2(initalSize.x, initalSize.y),
        duration: 0.3f,
        delay: 0f,
        easeCurve: Tween.EaseOut
        );
    }
}
