﻿using System.Collections;
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
            else if (state == State.Closing && !(blocked && (transform.forward != -Vector3.up || Omino.instance.moving)))
            {
                if (blocked && transform.forward == -Vector3.up)
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
        mesh.localPosition = Vector3.forward * offset * openDistance;
        float size = Mathf.Lerp(1f, 0.99f, offset);
        mesh.localScale = new Vector3(
            transform.forward.x == 0 ? size : 1f,
            transform.forward.y == 0 ? size : 1f,
            transform.forward.z == 0 ? size : 1f
        );

        if (transform.forward.y == 0)
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
        Gizmos.DrawWireMesh(ResourceLoader.Get<Mesh>("Arrow"), transform.position, Quaternion.LookRotation(transform.forward));
    }
}