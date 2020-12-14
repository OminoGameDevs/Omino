using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OrientableExtension
{
    public static Directions GetOrientableDirections(this Component c)
    {
        var or = System.Attribute.GetCustomAttribute(c.GetType(), typeof(OrientableAttribute)) as OrientableAttribute;
        if (or != null) return or.directions;
        else return GetOrientableDirections(c.gameObject);
    }

    public static Directions GetOrientableDirections(this GameObject g)
    {
        foreach (var c in g.GetComponents<Component>())
        {
            var or = System.Attribute.GetCustomAttribute(c.GetType(), typeof(OrientableAttribute)) as OrientableAttribute;
            if (or != null) return or.directions;
        }
        return Directions.None;
    }
}
