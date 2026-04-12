using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Godseeker.Common;

public static class GodseekerTileSets
{
    /// <summary>
    /// Whether to use the tile's alternate <see cref="TileObjectData"/>s'
    /// <see cref="TileObjectData.RandomStyleRange"/> for calculating the
    /// style of the placed tile.
    /// </summary>
    public static bool[] UseAlternateTileObjectDataRandomStyles { get; private set; } = [];

    /// <summary>
    /// The tile to swap to when lightly hit by a tool, similar to grass or moss;
    /// will use the tiles hit effects over the target tiles.
    /// </summary>
    public static int[] TransformTo { get; private set; } = [];

    private static Mod Mod => ModContent.GetInstance<Godseeker>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        UseAlternateTileObjectDataRandomStyles = CreateSet(nameof(UseAlternateTileObjectDataRandomStyles), false);

        TransformTo = CreateSet(nameof(TransformTo), -1);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return TileID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

#region Edits
    [OnLoad]
    private static void Load()
    {
        On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill_TransformTo;

        // Replicating logic like vanilla's moss tiles is impossible with ModTile.KillTile
        // since hit sounds are played after TileLoader.KillTile (same with particles.)
        On_WorldGen.KillTile += KillTile_TransformTo;

        // CanPlace does not account for alternate TileObjectData's RandomStyleRange nor
        // SpecificRandomStyles -- although irrelevant since this tile does not use it --
        // unlike GetTileData(Tile) which does for some unknown reason.
        // Additionally, I cannot understand the logic TileObject.CanPlace uses to find
        // the tiles alternate TileObjectData, so I use my own (this is a hack, I am not
        // nearly talented enough to correctly do this.)
        IL_TileObject.CanPlace += CanPlace_UseCorrectTileObjectDataForRandomStyles;
    }

#region Transform To
    private static bool DoesPickTargetTransformOnKill_TransformTo(On_Player.orig_DoesPickTargetTransformOnKill orig, Player self, HitTile hitCounter, int damage, int x, int y, int pickPower, int bufferIndex, Tile tileTarget)
    {
        int swapType = TransformTo[tileTarget.type];

        if (swapType >= 0 && hitCounter.AddDamage(bufferIndex, damage, updateAmount: false) >= 100)
        {
            return true;
        }

        return orig(self,  hitCounter, damage, x, y,  pickPower, bufferIndex, tileTarget);
    }

    private static void KillTile_TransformTo(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
    {
        orig(i, j, fail, effectOnly, noItem);

        Tile tile = Framing.GetTileSafely(i, j);

        if (!tile.HasTile)
        {
            return;
        }

        int swapType = TransformTo[tile.type];

        fail |= WorldGen.CheckTileBreakability(i, j) == 1;

        if (fail && !effectOnly && swapType != -1)
        {
            Main.tile[i, j].TileType = (ushort)swapType;
        }
    }
#endregion

#region Use Alternate TileObjectData Random Styles
    private static void CanPlace_UseCorrectTileObjectDataForRandomStyles(ILContext il)
    {
        var c = new ILCursor(il);

        int typeIndex = -1; // arg
        int styleIndex = -1; // arg

        int tileDataIndex = -1; // loc

        c.GotoNext(
            i => i.MatchCall<TileObjectData>(nameof(TileObjectData.GetTileData)),
            i => i.MatchStloc(out tileDataIndex)
        );

        c.GotoPrev(
            i => i.MatchLdarg(out typeIndex), // type
            i => i.MatchLdarg(out styleIndex) // style
        );

        while (c.TryGotoNext(
                   MoveType.After,
                   i => i.MatchLdloc(tileDataIndex),
                   i => i.MatchCallvirt<TileObjectData>($"get_{nameof(TileObjectData.RandomStyleRange)}")
               ))
        {
            c.EmitLdarg(typeIndex);
            c.EmitLdarg(styleIndex);

            c.EmitDelegate(
                static (int randomStyleRange, int type, int style) =>
                {
                    if (!UseAlternateTileObjectDataRandomStyles[type])
                    {
                        return randomStyleRange;
                    }

                    TileObjectData alternateTileData = GetAlternateTileObjectData(type, style);

                    return alternateTileData.RandomStyleRange;
                }
            );
        }
    }

    private static TileObjectData GetAlternateTileObjectData(int type, int style)
    {
        TileObjectData tileObjectData = TileObjectData._data[type];

        // TODO: SubTile support (no)

        if (tileObjectData._alternates == null)
        {
            return tileObjectData;
        }

        foreach (TileObjectData data in tileObjectData.Alternates)
        {
            if (data != null &&
                style >= data.Style &&
                style <= data.Style + data.RandomStyleRange)
            {
                return data;
            }
        }
        return tileObjectData;
    }
#endregion
#endregion
}
