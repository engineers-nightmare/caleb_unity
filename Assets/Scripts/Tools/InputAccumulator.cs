using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

enum BuildToolInputType
{
    Primary,
    Secondary,
    Tertiary,
    None,
}

class BuildToolInputAccumulator
{
    private BuildToolInputType _inputType;

    public float Duration { get; private set; }

    public bool Increment(BuildToolInputType type,
        float deltaTime, float targetTime)
    {
        if (_inputType != type)
        {
            Duration = 0.0f;
        }
        _inputType = type;
        Duration += deltaTime;

        if (targetTime <= Duration)
        {
            Duration = 0.0f;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        Duration = 0.0f;
    }
}