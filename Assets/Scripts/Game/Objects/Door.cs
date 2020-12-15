using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[Orientable(Directions.XYZ)]
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Door : Activatable
{
    private enum State
    {
        Idle,
        Opening,
        Closing
    }

    private const float openDistance = 0.999f;

    [SerializeField] private bool startOpen;

    private Transform mesh;
    private new Renderer renderer;
    private BoxCollider trigger;

    private bool open;
    private bool blocked;
    private State state;

    private void Awake()
    {
        mesh = transform.GetChild(0);
        renderer = GetComponent<Renderer>();
        trigger = GetComponent<BoxCollider>();

        if (Application.isPlaying && startOpen)
        {
            open = true;
            SetOffset(1f);
            mesh.gameObject.layer = 0;
        }
    }

    private void Update()
    {
        if (!mesh) mesh = transform.GetChild(0);
        if (!renderer) renderer = mesh.GetComponent<Renderer>();
        renderer.sharedMaterial.color = colorValue;

        if (Application.isPlaying)
        {
            if (state == State.Opening && !blocked)
            {
                state = State.Idle;
                mesh.gameObject.layer = 0;
                Tween.Stop(GetInstanceID());
                Tween.Value(
                    startValue: 0f,
                    endValue: 1f,
                    duration: Constants.transitionTime,
                    delay: 0f,
                    easeCurve: Tween.EaseIn,
                    valueUpdatedCallback: SetOffset
                );
            }
            else if (state == State.Closing && !(blocked && (transform.up != Vector3.up || Omino.instance.moving)))
            {
                if (blocked && transform.up == Vector3.up)
                    Omino.instance.Slide(Vector3.up, Tween.EaseIn);

                state = State.Idle;
                mesh.gameObject.layer = LayerMask.NameToLayer("Obstacle");
                Tween.Stop(GetInstanceID());
                Tween.Value(
                    startValue: 1f,
                    endValue:   0f,
                    duration:   Constants.transitionTime,
                    delay:      0f,
                    easeCurve:  Tween.EaseIn,
                    valueUpdatedCallback: SetOffset
                );
            }
        }
    }

    private void Open()
    {
        if (open) return;
        open = true;
        state = State.Opening;
    }

    private void Close()
    {
        if (!open) return;
        open = false;
        state = State.Closing;
    }

    private void SetOffset(float offset)
    {
        mesh.localPosition = -Vector3.up * offset * openDistance;
        float size = Mathf.Lerp(1f, openDistance, offset);
        mesh.localScale = new Vector3(size, 1f, size);

        if (transform.up.y == 1)
            trigger.center = Vector3.up * (1-offset);
    }

    protected override void OnActivate()
    {
        if (startOpen) Close();
        else Open();
    }

    protected override void OnDeactivate()
    {
        if (startOpen) Open();
        else Close();
    }

    private void OnOminoEnter(Omino.CubeStack stack)
    {
        blocked = true;
    }

    private void OnOminoExit(Omino.CubeStack stack)
    {
        blocked = false;
    }

    private void OnDrawGizmos()
    {
        const float outerSize = 0.9f;
        const float innerSize = 0.8f;
        const float outerDist = 0.5f;
        float innerDist = (startOpen ? outerDist : Mathf.Pow(outerDist, 2) * (1 - innerSize));
        Vector3 outerExt = (Vector3.one - transform.up.Abs()) * outerSize;
        Vector3 innerExt = (Vector3.one - transform.up.Abs()) * innerSize;
        if (!startOpen)
            innerExt += transform.up.Abs() * 2f * (outerDist - innerDist);

        Gizmos.DrawWireCube(transform.position - transform.up * outerDist, outerExt);
        Gizmos.DrawWireCube(transform.position - transform.up * innerDist, innerExt);
    }
}
