using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SwitchOptions : MonoBehaviour
{
    List<ColoredObject.Color> initialColors;
    private void OnEnable()
    {
        bool allStayPressedSame = true;
        bool allResetTimeSame = true;
        bool allColorsSame = false;
        Switch firstSwitch;
        initialColors = new List<ColoredObject.Color>();

        if (LevelEditor.instance.unconfirmedSwitches.Count > 0)
        {
            foreach (Transform sw in LevelEditor.instance.unconfirmedSwitches)
            {
                initialColors.Add(sw.GetComponent<Switch>().color);
            }
            firstSwitch = LevelEditor.instance.unconfirmedSwitches[0].GetComponent<Switch>();
            allColorsSame = LevelEditor.instance.unconfirmedSwitches.All(t => t.GetComponent<Switch>().color == firstSwitch.color);
            ColorPicker colorPicker = transform.Find("Panel").Find("ColorPicker").GetComponent<ColorPicker>();

            allStayPressedSame = LevelEditor.instance.unconfirmedSwitches.All(t => t.GetComponent<Switch>().stayPressed == LevelEditor.instance.unconfirmedSwitches[0].GetComponent<Switch>().stayPressed);
            allResetTimeSame = LevelEditor.instance.unconfirmedSwitches.All(t => t.GetComponent<Switch>().resetTime == LevelEditor.instance.unconfirmedSwitches[0].GetComponent<Switch>().resetTime);

            if (allColorsSame)
            {
                colorPicker.UpdateColorPickerColor(firstSwitch.TransformColorToIndex(firstSwitch.color).ToString());
            }
            else
            {
                colorPicker.UpdateColorPickerColor("");
            }

            if (allStayPressedSame)
            {
                transform.Find("Panel").Find("SwitchOptionsToggle").GetComponent<Toggle>().isOn = LevelEditor.instance.unconfirmedSwitches[0].GetComponent<Switch>().stayPressed;
            }
            else
            {
                transform.Find("Panel").Find("SwitchOptionsToggle").GetComponent<Toggle>().isOn = true;
            }

            if (allResetTimeSame)
            {
                transform.Find("Panel").Find("SwitchOptionsSlider").GetComponent<Slider>().value = LevelEditor.instance.unconfirmedSwitches[0].GetComponent<Switch>().resetTime;
            }
            else
            {
                transform.Find("Panel").Find("SwitchOptionsSlider").GetComponent<Slider>().value = 0f;
            }
        }
    }

    public void ConfirmSwitchOptions()
    {
        bool toggleValue = transform.Find("Panel").Find("SwitchOptionsToggle").GetComponent<Toggle>().isOn;
        int sliderValue = (int)transform.Find("Panel").Find("SwitchOptionsSlider").GetComponent<Slider>().value;
        foreach (Transform obj in LevelEditor.instance.unconfirmedSwitches)
        {
            obj.GetComponent<Switch>().SetStayPressed(toggleValue);
            obj.GetComponent<Switch>().SetResetTime(sliderValue);
        }
        gameObject.SetActive(false);
        if (LevelEditor.instance.unconfirmedDoors.Count > 0)
            LevelEditor.instance.ShowDoorOptions();
        else
        {
            LevelEditor.instance.ConfirmPlacement();
        }
    }

    public void CancelSwitchOptions()
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

    public void HandleSliderChange()
    {
        transform.Find("Panel").Find("ResetTime").GetComponent<Text>().text = transform.Find("Panel").Find("SwitchOptionsSlider").GetComponent<Slider>().value.ToString() + " s";
    }
}
