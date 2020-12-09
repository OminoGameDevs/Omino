using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class OrientationEditor : Editor
{
    private Editor defaultEditor;
    private Transform transform;

    private void OnEnable()
    {
        defaultEditor = Editor.CreateEditor(targets, System.Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
        transform = target as Transform;
    }

    private void OnDisable()
    {
        MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (disableMethod != null)
            disableMethod.Invoke(defaultEditor, null);
        DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI() => defaultEditor.OnInspectorGUI();

    private void OnSceneGUI()
    {
        if (!transform.GetComponentInParent<Level>())
            return;

        if (Tools.current == Tool.Rotate || Tools.current == Tool.Scale)
            Tools.current = Tool.Move;

        switch (transform.GetOrientableDirections())
        {
            case Directions.XYZ:
                Handles.DrawWireDisc(transform.position, transform.forward, 0.5f);
                Handles.DrawWireDisc(transform.position, transform.right,   0.5f);

                if (Camera.current)
                    Handles.DrawWireDisc(transform.position, Camera.current.transform.position - transform.position, 0.5f);

                SetOrientation( transform.up);
                SetOrientation(-transform.up);

                goto case Directions.XZ;

            case Directions.XZ:
                Handles.DrawWireDisc(transform.position, transform.up, 0.5f);

                SetOrientation( transform.forward, true);
                SetOrientation(-transform.forward);
                SetOrientation( transform.right);
                SetOrientation(-transform.right);

                break;

            default: break;
        }
    }

    private void SetOrientation(Vector3 dir, bool primary = false)
    {
        float size = primary ? 1f : 0.5f;
        if (Handles.Button(transform.position + dir * 0.5f, Quaternion.LookRotation(dir), size, size, Handles.ArrowHandleCap) && !primary)
        {
            Undo.RecordObject(transform, "Change Orientation");
            transform.forward = dir;
        }
    }
}
