using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Microsoft.Xna.Framework;
using Solstice.Core;
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
    public static readonly ImmutableArray<Color> BASIC =
    [
        new(170, 202, 249),
        new(254, 153, 156),
        new(255, 220, 177)
    ];
    
    public static readonly ImmutableArray<Color> BASIC_ALTERNATE =
    [
        new(140, 122, 249),
        new(255, 100, 106),
        new(255, 220, 177)
    ];
    
    public static readonly ImmutableArray<Color> RED_THUNDERSTORM =
    [
        new(20, 0, 0),
        new(105, 40, 20),
        new(255, 60, 100)
    ];
    
    public static readonly ImmutableArray<Color> RED_THUNDERSTORM_FLASH =
    [
        new(255, 205, 205),
        new(255, 205, 205),
        new(255, 205, 205)
    ];
}

public sealed partial class AerieBackground : ModSurfaceBackgroundStyle
{
    private static void DrawSky()
    {
        var sky = TextureAssets.MagicPixel.Value;
        var skyShader = Data.Instance.AerieSkyShader;
        var skyDest = new Rectangle(0, 0, Main.screenWidth, Math.Max(Main.screenHeight, sky.Height));
        
        skyShader.Parameters.TopColor =    SkyManagement.SkyColor[0].ToVector4(); 
        skyShader.Parameters.MiddleColor = SkyManagement.SkyColor[1].ToVector4();
        skyShader.Parameters.BottomColor = SkyManagement.SkyColor[2].ToVector4();

        skyShader.Apply();

        Main.spriteBatch.Draw(sky, skyDest, Color.White);
    }

    [ModSystemHooks.ModifySunLightColor]
    private static void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!AerieSubworld.Active)
        {
            return;
        }

        tileColor = Color.OklabLerp(SkyManagement.SkyTopColor, SkyManagement.SkyMiddleColor, 0.3f);
        backgroundColor = SkyManagement.SkyTint;
        Main.ColorOfTheSkies = backgroundColor;
    }
}