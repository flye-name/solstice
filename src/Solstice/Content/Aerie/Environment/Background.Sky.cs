using Daybreak.Common.Features.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

/// <summary>
/// Provides arrays of colors the sky uses in specific circumstances.<para/>
/// Indices for the arrays correspond to points on a vertical gradient: Top = 0, Middle = 1, Bottom = 2
/// </summary>
public static class PresetSkyColors
{
    public static readonly ImmutableArray<Color> DefaultDay =
    [
        new(170, 202, 249),
        new(254, 153, 156),
        new(255, 220, 177)
    ];
}

public sealed partial class AerieBackground : ModSurfaceBackgroundStyle
{
    /// <summary>
    /// The sky gradient in the Aerie.
    /// </summary>
    public static Color[] SkyColor = PresetSkyColors.DefaultDay.ToArray();

    /// <summary>
    /// Value of <see cref="Main.ColorOfTheSkies"/>.
    /// </summary>
    public static Color SkyTint => Color.Lerp(SkyMiddleColor, SkyBottomColor, 0.3f);

    public static ref Color SkyTopColor => ref SkyColor[0];
    public static ref Color SkyMiddleColor => ref SkyColor[1];
    public static ref Color SkyBottomColor => ref SkyColor[2];
    
    
    private static void DrawSky()
    {
        var sky = TextureAssets.MagicPixel.Value;
        var skyShader = Data.Instance.AerieSkyShader;
        var skyDest = new Rectangle(0, 0, Main.screenWidth, Math.Max(Main.screenHeight, sky.Height));
        
        skyShader.Parameters.TopColor =    SkyColor[0].ToVector4(); 
        skyShader.Parameters.MiddleColor = SkyColor[1].ToVector4();
        skyShader.Parameters.BottomColor = SkyColor[2].ToVector4();
        skyShader.Apply();
        Main.spriteBatch.Draw(sky, skyDest, Color.White);
    }
}

public class AerieSkyTint : ModSystem
{
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!AerieSubworld.Active)
            return;
        
        tileColor = Color.Lerp(AerieBackground.SkyTopColor, AerieBackground.SkyMiddleColor, 0.3f);
        backgroundColor = AerieBackground.SkyTint;
        Main.ColorOfTheSkies = backgroundColor;
    }
}