using Daybreak.Common.Features.Hooks;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace GodseekerBoss.Common.IDs;

public static class GodseekerTileSets
{
    public static bool[] UseAlternateTileObjectDataRandomStyles { get; private set; } = [];

    private static Mod Mod => ModContent.GetInstance<GodseekerBoss>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        UseAlternateTileObjectDataRandomStyles = CreateSet<bool>(nameof(UseAlternateTileObjectDataRandomStyles), false);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return TileID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    [OnLoad]
    private static void Load()
    {
        // CanPlace does not account for alternate TileObjectData's RandomStyleRange nor
        // SpecificRandomStyles -- although irrelevant since this tile does not use it --
        // unlike GetTileData(Tile) which does for some unknown reason.
        // Additionally, I cannot understand the logic TileObject.CanPlace uses to find
        // the tiles alternate TileObjectData, so I use my own (this is a hack, I am not
        // nearly talented enough to correctly do this.)
        IL_TileObject.CanPlace += CanPlace_UseCorrectTileObjectDataForRandomStyles;
    }

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
}
