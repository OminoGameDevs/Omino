using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public bool startOpen;

    private Transform mesh;
    private new Renderer renderer;
    private BoxCollider trigger;

    private bool open;
    private bool blocked;
    private State state;

    private bool triggeredSettings;
    private bool displayArrow;

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
            if (LevelEditor.instance?.editing == true)
            {
                if (!displayArrow)
                {
                    transform.Find("Arrow").gameObject.SetActive(true);
                    displayArrow = true;
                }
            }
            else
            {
                if (displayArrow)
                {
                    /*
                    if (!triggeredSettings)
                    {
                        triggeredSettings = true;
                        mesh = transform.GetChild(0);
                        renderer = GetComponent<Renderer>();
                        trigger = GetComponent<BoxCollider>();
                        open = startOpen;
                        if (open)
                        {
                            SetOffset(1f);
                            mesh.gameObject.layer = 0;
                        }
                    }
                    */
                    transform.Find("Arrow").gameObject.SetActive(false);
                    displayArrow = false;
                }
            }
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

    public void ChangeColor()
    {
        int value = GameObject.Find("DoorOptionsDropdown").GetComponent<Dropdown>().value;
        Color newColor;
        switch (value)
        {
            case 0: newColor = Color.Red; break;
            case 1: newColor = Color.Gold; break;
            case 2: newColor = Color.Green; break;
            case 3: newColor = Color.Blue; break;
            case 4: newColor = Color.Pink; break;
            default: newColor = Color.Red; break;
        }
        _color = newColor;
    }

    public void SetStartOpen(bool value)
    {
        startOpen = value;
    }

    public void ChangeColorWithString(string stringValue)
    {
        int value = int.Parse(stringValue);
        _color = TransformIndexToColor(value);
    }

    public void ChangeColorWithColor(Color col)
    {
        _color = col;
    }

    public void UpdateDoorSettings()
    {
        mesh = transform.GetChild(0);
        renderer = GetComponent<Renderer>();
        trigger = GetComponent<BoxCollider>();
        open = startOpen;
        if (open)
        {
            SetOffset(1f);
            mesh.gameObject.layer = 0;
        }
        UpdateActivateableSettings();
    }
}
