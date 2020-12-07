using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[RequireComponent(typeof(MeshRenderer))]
public class OminoCreature : MonoBehaviour
{
    private const float wobbleRadius = 0.15f;
    private const float wobblePeriod = 1f;
    private const float spinSpeed = 0.5f;
    private const float minBlinkInterval = 2f;
    private const float maxBlinkInterval = 5f;
    private const float blinkTime = 0.25f;

    [SerializeField] private Color detachedColor = Color.grey;
    [SerializeField] private Color attachedColor = Color.red;

    public bool attached => transform.GetComponentInParent<Omino>();

    private Transform eyes;
    private Material material;

    private float yaw;
    private int wobbleDir;
    private Vector3 topOffset;
    private Vector3 bottomOffset;
    private Vector3 wobbleOffset;
    private float wobbleTime;

    private Vector3 lastParentPos;
    private Vector3 pushOffset;
    private float interp;

    private void Awake()
    {
        eyes = transform.GetChild(0);
        material = new Material(ResourceLoader.Get<Shader>("Plain"));
        GetComponent<Renderer>().sharedMaterial = material;

        wobbleDir = Random.Range(0f, 1f) > 0.5f ? 1 : -1;
        topOffset    = new Vector3(Random.Range(-1f, 1f),  1, Random.Range(-1f, 1f)).normalized * wobbleRadius;
        bottomOffset = new Vector3(Random.Range(-1f, 1f), -1, Random.Range(-1f, 1f)).normalized * wobbleRadius;
        wobbleTime = Random.Range(0f, 1f);

        lastParentPos = transform.parent.position;

        Blink();
    }

    private void LateUpdate()
    {
        // Color based on attachment
        material.color = (attached ? attachedColor : detachedColor);

        // Face the direction of movement, or face the omino if not part of it
        float targetYaw = Quaternion.LookRotation(attached ? Omino.instance.direction : Vector3.Scale(Omino.instance.center - transform.position, new Vector3(1,0,1)).normalized, Vector3.up).eulerAngles.y;
        float s = 0f;
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref s, 0.05f);
        transform.eulerAngles = Vector3.up * yaw;

        Vector3 dir = (transform.parent.position - lastParentPos).normalized;
        lastParentPos = transform.parent.position;

        // Push against direction of movement if moving
        if (dir != Vector3.zero)
            pushOffset = dir * wobbleRadius;

        // Wobble back and forth if not moving
        wobbleOffset = Vector3.Lerp(bottomOffset, topOffset, Mathf.Pow(Mathf.Sin(0.5f * Mathf.PI * wobbleTime), 2f));
        wobbleTime += wobbleDir * (Time.deltaTime / wobblePeriod);
        if ((wobbleDir > 0 && wobbleTime >= 1f) || (wobbleDir < 0 && wobbleTime <= 0f))
        {
            wobbleDir *= -1;
            if (wobbleDir > 0)
                topOffset = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized * wobbleRadius;
            else
                bottomOffset = new Vector3(Random.Range(-1f, 1f), -1, Random.Range(-1f, 1f)).normalized * wobbleRadius;
        }

        s = 0f;
        interp = Mathf.SmoothDamp(interp, dir == Vector3.zero ? 0f : 1f, ref s, 0.05f);
        transform.position = transform.parent.position + Vector3.Lerp(wobbleOffset, pushOffset, interp);
    }

    private void Blink()
    {
        Tween.Value(
            startValue: -1f,
            endValue:    1f,
            duration:    blinkTime,
            delay:       Random.Range(minBlinkInterval, maxBlinkInterval),
            easeCurve:   Tween.EaseOut,
            valueUpdatedCallback: t => eyes.localScale = new Vector3(1f, Mathf.Abs(t), 1f),
            completeCallback: Blink
        );
    }
}