namespace Solstice.Core;

public enum SkyModifierPriority : byte
{
    Normal,
    Weather,
    StrongWeather,
    Bossfight
}

/// <summary>
/// For updating sky colors without conflicting with other sources doing that
/// </summary>
public abstract class SkyModifier
{
    public virtual SkyModifierPriority Priority => SkyModifierPriority.Normal;
    public virtual bool IsActive => false;

    public float TransitionTime;
    
    public abstract void UpdateSky();

    public virtual void ResetSkyModifierInformation()
    {
        TransitionTime = 0;
    }
}