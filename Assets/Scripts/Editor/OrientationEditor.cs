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
            case Directions.XZ:
                Handles.DrawWireDisc(transform.position, transform.up, 0.5f);

                SetOrientationXZ( transform.forward, true);
                SetOrientationXZ(-transform.forward);
                SetOrientationXZ( transform.right);
                SetOrientationXZ(-transform.right);

                break;

            case Directions.XYZ:
                SetOrientationXYZ( transform.forward);
                SetOrientationXYZ(-transform.forward);
                SetOrientationXYZ( transform.right);
                SetOrientationXYZ(-transform.right);
                SetOrientationXYZ( transform.up, true);
                SetOrientationXYZ(-transform.up);

                break;
        }
    }

    private void SetOrientationXZ(Vector3 dir, bool primary = false)
    {
        float size = primary ? 1f : 0.5f;
        if (Handles.Button(transform.position + dir * 0.5f, Quaternion.LookRotation(dir), size, size, Handles.ArrowHandleCap) && !primary)
        {
            Undo.RecordObject(transform, "Change Orientation");
            transform.forward = dir;
        }
    }

    private void SetOrientationXYZ(Vector3 dir, bool primary = false)
    {
        if (Handles.Button(transform.position - dir * 0.5f, Quaternion.LookRotation(dir), 0.2f, 0.2f, Handles.RectangleHandleCap) && !primary)
        {
            Undo.RecordObject(transform, "Change Orientation");
            transform.up = dir;
        }
        Handles.DrawWireDisc(transform.position - transform.up * 0.5f, transform.up, 0.15f);
    }
}
