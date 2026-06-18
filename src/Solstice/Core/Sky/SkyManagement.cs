using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Solstice.Content.Aerie;
using Solstice.Content.Aerie.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.Core;

namespace Solstice.Core;
public class SkyManagement
{
    #region colors
    /// <summary>
    /// The sky gradient in the Aerie.
    /// </summary>
    public static readonly Color[] SkyColor = PresetSkyColors.BASIC.ToArray();

    /// <summary>
    /// Value of <see cref="Main.ColorOfTheSkies"/>.
    /// </summary>
    public static Color SkyTint => Color.Lerp(SkyMiddleColor, SkyBottomColor, 0.3f);

    public static ref Color SkyTopColor => ref SkyColor[0];

    public static ref Color SkyMiddleColor => ref SkyColor[1];

    public static ref Color SkyBottomColor => ref SkyColor[2];
    #endregion
    
    #region utils
    public static void SetSkyColors(Color[] newColors)
    {
        for (int i = 0; i < SkyColor.Length; i++)
            SkyColor[i] = newColors[i];
    }
    
    public static void LerpSkyColors(Color[] newColors, float t)
    {
        for (int i = 0; i < SkyColor.Length; i++)
            SkyColor[i] = Color.Lerp(SkyColor[i], newColors[i], t);
    }
    
    public static void LerpSkyColors(Color[] colors1, Color[] colors2, float t)
    {
        for (int i = 0; i < SkyColor.Length; i++)
            SkyColor[i] = Color.Lerp(colors1[i], colors2[i], t);
    }
    #endregion
    
    #region modifiers
    public static readonly List<SkyModifier> Modifiers = new();

    [OnLoad]
    public static void LoadModifiers()
    {
        // Load all classes that inherit SkyModifier 
        var types = AssemblyManager.GetLoadableTypes(typeof(Solstice).Assembly)
            .Where(t => !t.IsAbstract)
            .Where(t => t.IsAssignableTo(typeof(SkyModifier)));

        foreach (var t in types)
        {
            var instance = Activator.CreateInstance(t);
            if (instance is not null)
                Modifiers.Add((SkyModifier)instance);
        }
    }

    [ModSystemHooks.PostUpdateEverything]
    public static void UpdateModifiers()
    {
        if (!AerieSubworld.Active)
            return;
              
        var activeModifiers = Modifiers.Where(x => x.IsActive).ToList();
        if (!activeModifiers.Any())
            return;

        // update the highest priority modifier  
        var leadingModifier = activeModifiers.Aggregate((x, y) => x.Priority > y.Priority ? x : y);
        leadingModifier.UpdateSky();
        
        // reset all modifier fields (ie transition time) for all inactive modifiers
        var inactiveModifiers = Modifiers.Where(x => !x.Equals(leadingModifier)).ToList();
        inactiveModifiers.ForEach(x => x.ResetSkyModifierInformation());
    }
    #endregion
}