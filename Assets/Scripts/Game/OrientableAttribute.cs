using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class OrientableAttribute : System.Attribute
{
    public readonly Directions directions;
    public OrientableAttribute(Directions directions = Directions.XZ) => this.directions = directions;
}
