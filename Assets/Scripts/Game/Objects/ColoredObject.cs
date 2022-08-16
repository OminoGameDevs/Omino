using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ColoredObject : MonoBehaviour
{
    public enum Color
    {
        Red,
        Gold,
        Green,
        Blue,
        Pink
    }

    public Color color => _color;
    public UnityEngine.Color colorValue {
        get {
            switch (color) {
                case Color.Red:   return new UnityEngine.Color(1f,    0f,    0f);
                case Color.Gold:  return new UnityEngine.Color(1f,    0.9f, 0f);
                case Color.Green: return new UnityEngine.Color(0f,    0.9f,  0f);
                case Color.Blue:  return new UnityEngine.Color(0f,    0.75f, 1f);
                case Color.Pink:  return new UnityEngine.Color(1f,    0f,    1f);
                default: return default(UnityEngine.Color);
            }
        }
    }
    public Color _color;

    public Color TransformIndexToColor(int value)
    {
        switch (value)
        {
            case 0: return Color.Red;
            case 1: return Color.Gold;
            case 2: return Color.Green;
            case 3: return Color.Blue;
            case 4: return Color.Pink;
            default: return Color.Red;
        }
    }

    public int TransformColorToIndex(Color value)
    {
        switch (value)
        {
            case Color.Red: return 0;
            case Color.Gold: return 1;
            case Color.Green: return 2;
            case Color.Blue: return 3;
            case Color.Pink: return 4;
            default: return 0;
        }
    }
}