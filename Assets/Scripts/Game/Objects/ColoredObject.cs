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
    [SerializeField] private Color _color;
}