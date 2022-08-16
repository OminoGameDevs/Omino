using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public string coloredObj = "Switch";
    private List<Color> activeColors = new List<Color>(){
        new Color(1f, 0f, 0f),
        new Color(1f, 0.9f, 0f),
        new Color(0f, 0.9f, 0f),
        new Color(0f, 0.75f, 1f),
        new Color(1f, 0f, 1f)
    };

    private List<Color> inactiveColors = new List<Color>(){
        new Color(0.6f, 0f, 0f),
        new Color(0.6f, 0.54f, 0f),
        new Color(0f, 0.54f, 0f),
        new Color(0f, 0.45f, 0.6f),
        new Color(0.6f, 0f, 0.6f)
    };

    public void UpdateColorPickerColor(string color)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Image>().color = inactiveColors[i];
        }
        if (color != "")
        {
            int intColor = int.Parse(color);
            transform.GetChild(intColor).GetComponent<Image>().color = activeColors[intColor];
        }
    }

    public void ChangeSwitchColor(string color)
    {
        UpdateColorPickerColor(color);
        if (coloredObj == "Switch")
        {
            foreach (Transform obj in LevelEditor.instance.unconfirmedSwitches)
            {
                obj.GetComponent<Switch>().ChangeColorWithString(color);
            }
        }
        else if (coloredObj == "Door")
        {
            foreach (Transform obj in LevelEditor.instance.unconfirmedDoors)
            {
                obj.GetComponent<Door>().ChangeColorWithString(color);
            }
        }
    }
}
