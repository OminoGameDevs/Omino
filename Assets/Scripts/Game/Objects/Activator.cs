using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Activator : ColoredObject
{
    [SerializeField, Range(0, 600)] public int resetTime;

    public bool activated { get; private set; }
    public float timeLeft { get; private set; }

    private void FixedUpdate()
    {
        if (timeLeft > 0f)
        {
            timeLeft -= Time.fixedDeltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                if (activated)
                    Deactivate(0f);
            }
        }
    }

    public void Activate()
    {
        if (timeLeft > 0f)
            timeLeft = 0f;

        if (activated) return;

        activated = true;
        OnActivate();
    }

    public void Deactivate(float time)
    {
        if (!activated) return;

        if (time <= 0f)
        {
            timeLeft = 0f;
            activated = false;

            OnDeactivate();
        }
        else if (timeLeft == 0f)
            timeLeft = time;
    }
    public void Deactivate() => Deactivate(resetTime);

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }
}
