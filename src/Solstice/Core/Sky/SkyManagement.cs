using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Solstice.Content.Aerie;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
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
    
    #region drawblack fixes
    // https://github.com/gold-meridian/sky-projects/blob/master/src/ZenSkies/Common/Systems/Sky/SkyColoration.cs
    [OnLoad]
    private static void LoadEdits()
    {
        IL_Main.DrawBlack += DrawBlack_NonSolid;

        IL_TileDrawing.DrawSingleTile += DrawSingleTile_NonSolid;

        IL_WallDrawing.DrawWalls += DrawWalls_NonSolid;
    }
    private static void DrawBlack_NonSolid(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel? breakTarget = c.DefineLabel();

        int tileXIndex = -1; // loc
        int tileYIndex = -1; // loc

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdloc(out _),
            i => i.MatchBrtrue(out _),
            i => i.MatchLdloc(out _),
            i => i.MatchBrfalse(out breakTarget),
            i => i.MatchLdsfld<Main>(nameof(Main.drawToScreen))
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchLdsflda<Main>(nameof(Main.tile)),
            i => i.MatchLdloc(out tileXIndex),
            i => i.MatchLdloc(out tileYIndex),
            i => i.MatchCall<Tilemap>("get_Item"),
            i => i.MatchStloc(out _)
        );

        c.EmitLdloc(tileXIndex);
        c.EmitLdloc(tileYIndex);

        c.EmitDelegate(Tile.IgnoresDrawBlack);

        c.EmitBrtrue(breakTarget);
    }

    private static void DrawSingleTile_NonSolid(ILContext il)
    {
        var c = new ILCursor(il);

        int tileXIndex = -1; // arg
        int tileYIndex = -1; // arg

        c.GotoNext(
            i => i.MatchLdsflda<Main>(nameof(Main.tile)),
            i => i.MatchLdarg(out tileXIndex),
            i => i.MatchLdarg(out tileYIndex),
            i => i.MatchCall<Tilemap>("get_Item")
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdarg(out _),
            i => i.MatchLdflda<TileDrawInfo>(nameof(TileDrawInfo.tileLight)),
            i => i.MatchCall<Color>($"get_{nameof(Color.R)}"),
            i => i.MatchLdcI4(1),
            i => i.MatchBge(out _)
        );

        c.GotoPrev(
            MoveType.Before,
            i => i.MatchStloc(out _)
        );

        c.EmitPop();

        c.EmitLdarg(tileXIndex);
        c.EmitLdarg(tileYIndex);

        c.EmitDelegate(Tile.IgnoresDrawBlack);
    }

    private static void DrawWalls_NonSolid(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel? jumpColorCheckTarget = c.DefineLabel();

        int tileXIndex = -1; // loc
        int tileYIndex = -1; // loc

        int colorIndex = -1; // loc

        c.GotoNext(
            i => i.MatchLdloc(out tileXIndex),
            i => i.MatchLdloc(out tileYIndex),
            i => i.MatchCall<Terraria.Lighting>(nameof(Terraria.Lighting.GetColor)),
            i => i.MatchStloc(out colorIndex)
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdloca(colorIndex),
            i => i.MatchCall<Color>($"get_{nameof(Color.R)}"),
            i => i.MatchBrtrue(out jumpColorCheckTarget)
        );

        c.MoveAfterLabels();

        c.EmitLdloc(tileXIndex);
        c.EmitLdloc(tileYIndex);

        c.EmitDelegate(Tile.IgnoresDrawBlack);

        c.EmitBrtrue(jumpColorCheckTarget);
    }
    #endregion
}