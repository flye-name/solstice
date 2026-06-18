using Microsoft.Xna.Framework;
using Solstice.Core;
using System;
using System.Linq;
using Terraria;

namespace Solstice.Content.Aerie;

public class NormalSky : SkyModifier
{
    public override SkyModifierPriority Priority => SkyModifierPriority.Normal;
    public override bool IsActive => true;

    public override void UpdateSky()
    {
        Color[] colors = Color.ArrayLerp(PresetSkyColors.BASIC, PresetSkyColors.BASIC_ALTERNATE, (MathF.Sin((float)Main.timeForVisualEffects * 0.001f) + 1) * 0.5f);
        SkyManagement.LerpSkyColors(colors, TransitionTime = MathF.Min(TransitionTime + 0.001f, 1f));
    }
    
    public override void ResetSkyModifierInformation()
    {
        base.ResetSkyModifierInformation();
        Main.NewText("Resetting Normal Sky");
    }
}