using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DoorOptions : MonoBehaviour
{
    List<ColoredObject.Color> initialColors;

    private void OnEnable()
    {
        bool allStartOpenSame = true;
        bool allColorsSame = false;

        initialColors = new List<ColoredObject.Color>();

        Door firstDoor;

        if (LevelEditor.instance.unconfirmedDoors.Count > 0)
        {
            foreach (Transform door in LevelEditor.instance.unconfirmedDoors)
            {
                initialColors.Add(door.GetComponent<Door>().color);
            }
            firstDoor = LevelEditor.instance.unconfirmedDoors[0].GetComponent<Door>();
            allColorsSame = LevelEditor.instance.unconfirmedDoors.All(t => t.GetComponent<Door>().color == firstDoor.color);
            ColorPicker colorPicker = transform.Find("Panel").Find("ColorPicker").GetComponent<ColorPicker>();
            if (allColorsSame)
            {
                colorPicker.UpdateColorPickerColor(firstDoor.TransformColorToIndex(firstDoor.color).ToString());
            }
            else
            {
                colorPicker.UpdateColorPickerColor("");
            }
        }

        if (LevelEditor.instance.unconfirmedDoors.Count > 0)
        {
            allStartOpenSame = LevelEditor.instance.unconfirmedDoors.All(t => t.GetComponent<Door>().startOpen == LevelEditor.instance.unconfirmedDoors[0].GetComponent<Door>().startOpen);
        }
        if (allStartOpenSame)
        {
            transform.Find("Panel").Find("DoorOptionsToggle").GetComponent<Toggle>().isOn = LevelEditor.instance.unconfirmedDoors[0].GetComponent<Door>().startOpen;
        }
        else
        {
            transform.Find("Panel").Find("DoorOptionsToggle").GetComponent<Toggle>().isOn = true;
        }
    }

    public void ConfirmDoorOptions()
    {
        bool toggleValue = transform.Find("Panel").Find("DoorOptionsToggle").GetComponent<Toggle>().isOn;
        foreach (Transform obj in LevelEditor.instance.unconfirmedDoors)
        {
            obj.GetComponent<Door>().SetStartOpen(toggleValue);
        }
        gameObject.SetActive(false);
        LevelEditor.instance.ConfirmPlacement();
    }

    public void CancelDoorOptions()
    {
        gameObject.SetActive(false);
        var index = 0;
        foreach (Transform sw in LevelEditor.instance.unconfirmedSwitches)
        {
            sw.GetComponent<Switch>().ChangeColorWithColor(initialColors[index]);
            index++;
        }
        LevelEditor.instance.ConfirmPlacement();
    }
}
