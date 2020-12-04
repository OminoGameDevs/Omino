using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Forward,
    Right,
    Up,
    Back,
    Left,
    Down
}

public static class DirectionExtension
{
    public static Vector3 ToVector(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Forward: return Vector3.forward;
            case Direction.Right:   return Vector3.right;
            case Direction.Up:      return Vector3.up;
            case Direction.Back:    return Vector3.back;
            case Direction.Left:    return Vector3.left;
            case Direction.Down:    return Vector3.down;
            default:                return default(Vector3);
        }
    }

    public static bool IsVertical(this Direction dir) => dir == Direction.Up || dir == Direction.Down;
}
