using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionXYZ
{
    Forward,
    Right,
    Up,
    Back,
    Left,
    Down
}

public enum DirectionXZ
{
    Forward,
    Right,
    Back,
    Left
}

public static class DirectionExtension
{
    public static Vector3 ToVector(this DirectionXYZ dir)
    {
        switch (dir)
        {
            case DirectionXYZ.Forward: return Vector3.forward;
            case DirectionXYZ.Right:   return Vector3.right;
            case DirectionXYZ.Up:      return Vector3.up;
            case DirectionXYZ.Back:    return Vector3.back;
            case DirectionXYZ.Left:    return Vector3.left;
            case DirectionXYZ.Down:    return Vector3.down;
            default:                   return default(Vector3);
        }
    }

    public static Vector3 ToVector(this DirectionXZ dir)
    {
        switch (dir)
        {
            case DirectionXZ.Forward: return Vector3.forward;
            case DirectionXZ.Right:   return Vector3.right;
            case DirectionXZ.Back:    return Vector3.back;
            case DirectionXZ.Left:    return Vector3.left;
            default:                  return default(Vector3);
        }
    }

    public static bool IsVertical(this DirectionXYZ dir) => dir == DirectionXYZ.Up || dir == DirectionXYZ.Down;
}
