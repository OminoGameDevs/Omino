using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Activatable : ColoredObject
{
    public bool activated { get; private set; }

    private Activator[] activators;

    private void FixedUpdate()
    {
        if (activators == null)
            activators = Game.instance.level.GetObjectsOfType<Activator>().Where(a => a.color == color).ToArray();

        bool shouldBeActivated = activators.Length > 0 && !activators.Any(a => !a.activated);
        if (shouldBeActivated && !activated)
        {
            activated = true;
            OnActivate();
        }
        else if (!shouldBeActivated && activated)
        {
            activated = false;
            OnDeactivate();
        }
    }

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }
}
