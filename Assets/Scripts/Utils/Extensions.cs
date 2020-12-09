using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	public static Vector3 Orthogonalize(this Vector3 direction)
    {
        float xSize = Mathf.Abs(direction.x);
        float ySize = Mathf.Abs(direction.y);
        float zSize = Mathf.Abs(direction.z);
        float max = Mathf.Max(xSize, ySize, zSize);

        float newX = 0f, newY = 0f, newZ = 0f;

        if (xSize == max)
            newX = Mathf.Sign(direction.x);

        else if (ySize == max)
            newY = Mathf.Sign(direction.y);

        else if (zSize == max)
            newZ = Mathf.Sign(direction.z);

        return new Vector3(newX, newY, newZ);
    }

    public static Vector3 Round(this Vector3 vector, int decimals = 0)
    {
        float f = Mathf.Pow(10f, decimals);
        return new Vector3(Mathf.Round(vector.x * f) / f, Mathf.Round(vector.y * f) / f, Mathf.Round(vector.z * f) / f);
    }

    public static Quaternion Round(this Quaternion quat) => Quaternion.Euler(new Vector3(Mathf.Round(quat.eulerAngles.x / 90f) * 90f,
                                                                                         Mathf.Round(quat.eulerAngles.y / 90f) * 90f,
                                                                                         Mathf.Round(quat.eulerAngles.z / 90f) * 90f));

    public static void Reset(this Transform t, bool position = true, bool rotation = true, bool scale = true)
    {
        if (position) t.position   = Vector3.zero;
        if (rotation) t.rotation   = Quaternion.identity;
        if (scale)    t.localScale = Vector3.one;
    }
}
