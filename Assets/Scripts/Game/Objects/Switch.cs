using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Pixelplacement;
using System.Linq;

[Orientable(Directions.XYZ)]
[ExecuteInEditMode]
public class Switch : Activator
{
    public bool stayPressed;

    private Transform mesh;
    private new Renderer renderer;

    private Switch[] others;
    private bool allActivated => activated && !others.Any(sw => !sw.activated);

    private bool triggeredSettings;

    private void Awake()
    {
        mesh = transform.GetChild(0);
    }

    private void Start()
    {
        if (Application.isPlaying)
            others = (from sw in Game.instance.level.GetObjectsOfType<Switch>() where sw != this && sw.color == color select sw).ToArray();
    }

    private void Update()
    {
        if (!mesh) mesh = transform.GetChild(0);
        if (!renderer) renderer = mesh.GetComponent<Renderer>();
        renderer.sharedMaterial.color = colorValue;
        if (Application.isPlaying && !triggeredSettings)
        {
            if (Game.instance.playing)
            {
                if (LevelEditor.instance?.editing != true)
                {
                    others = (from sw in Game.instance.level.GetObjectsOfType<Switch>() where sw != this && sw.color == color select sw).ToArray();
                    triggeredSettings = true;
                }
            }
        }
    }

    protected override void OnActivate()
    {
        Tween.Stop(mesh.GetInstanceID());
        mesh.localScale = new Vector3(1f, 0.001f, 1f);
        AudioManager.PlaySound("switch");
    }

    protected override void OnDeactivate()
    {
        Tween.Stop(mesh.GetInstanceID());
        Tween.LocalScale(
            target:    mesh,
            endValue:  Vector3.one,
            duration:  Constants.transitionTime,
            delay:     0f,
            easeCurve: Tween.EaseOut
        );
    }

    private void OnOminoEnter(Omino.CubeStack stack) => Activate();

    private void OnOminoExit(Omino.CubeStack stack)
    {
        if (allActivated && !stayPressed)
            Deactivate();
        else if (!allActivated)
            Deactivate(0f);
    }

    public void ChangeColorWithString(string stringValue)
    {
        int value = int.Parse(stringValue);
        _color = TransformIndexToColor(value);
    }

    public void ChangeColorWithColor(Color col)
    {
        _color = col;
    }

    public void SetStayPressed(bool value)
    {
        stayPressed = value;
    }

    public void SetResetTime(int value)
    {
        resetTime = value;
    }

    public void UpdateSwitchSettings()
    {
        others = (from sw in Game.instance.level.GetObjectsOfType<Switch>() where sw != this && sw.color == color select sw).ToArray();
    }
}
