using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Conveyor : Activatable
{
    private Vector3 direction => _direction.ToVector();
    [SerializeField] private DirectionXZ _direction = DirectionXZ.Forward;
    [SerializeField] private bool startActive = true;

    private new Renderer renderer;
    private BoxCollider trigger;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        trigger = GetComponent<BoxCollider>();

        //if (Application.isPlaying && startOpen)
        //{
        //    open = true;
        //    SetOffset(1f);
        //    mesh.gameObject.layer = 0;
        //}
    }

    private void Update()
    {
        if (!renderer) renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.color = colorValue;

        //if (Application.isPlaying)
        //{
        //    if (state == State.Opening && !blocked)
        //    {
        //        state = State.Idle;
        //        mesh.gameObject.layer = 0;
        //        Tween.Stop(GetInstanceID());
        //        Tween.Value(
        //            startValue: 0f,
        //            endValue: 1f,
        //            duration: Constants.transitionTime,
        //            delay: 0f,
        //            easeCurve: Tween.EaseIn,
        //            valueUpdatedCallback: SetOffset
        //        );
        //    }
        //    else if (state == State.Closing && !(blocked && (_direction != DirectionXYZ.Down || Omino.instance.moving)))
        //    {
        //        if (blocked && _direction == DirectionXYZ.Down)
        //            Omino.instance.Slide(Vector3.up, Tween.EaseIn);

        //        state = State.Idle;
        //        mesh.gameObject.layer = LayerMask.NameToLayer("Obstacle");
        //        Tween.Stop(GetInstanceID());
        //        Tween.Value(
        //            startValue: 1f,
        //            endValue:   0f,
        //            duration:   Constants.transitionTime,
        //            delay:      0f,
        //            easeCurve:  Tween.EaseIn,
        //            valueUpdatedCallback: SetOffset
        //        );
        //    }
        //}
        //else
        //    transform.localRotation = Quaternion.identity;
    }

    //protected override void OnActivate()
    //{
    //    if (startOpen) Close();
    //    else Open();
    //}

    //protected override void OnDeactivate()
    //{
    //    if (startOpen) Open();
    //    else Close();
    //}

    //private void OnOminoEnter(Omino.CubeStack stack)
    //{
    //    blocked = true;
    //}

    //private void OnOminoExit(Omino.CubeStack stack)
    //{
    //    blocked = false;
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireMesh(ResourceLoader.Get<Mesh>("Arrow"), transform.position, Quaternion.LookRotation(direction));
    //}
}
