using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace GodseekerBoss.Content.Aerie.Placements;

public class AerieGrassSeeds : ModItem
{
    private static readonly Dictionary<int, int> grass_replacements = new()
    {
        {ModContent.TileType<AerieBrickTile>(), ModContent.TileType<AerieBrickGrassTile>()},
    };

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;

        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.DirtBlock, 1);
        ItemID.Sets.GrassSeeds[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickGrassTile>());
    }

    public override bool? UseItem(Player player)
    {
        int i = Player.tileTargetX;
        int j = Player.tileTargetY;

        Tile tile = Framing.GetTileSafely(i, j);

        if (tile.HasTile &&
            grass_replacements.ContainsKey(tile.TileType) &&
            player.IsInTileInteractionRange(i, j, TileReachCheckSettings.Simple))
        {
            Main.tile[i, j].TileType = (ushort)grass_replacements[tile.TileType];

            WorldGen.SquareTileFrame(i, j, true);

            SoundEngine.PlaySound(SoundID.Dig, player.position);

            return true;
        }

        return false;
    }
}

public class AerieTallGrassSeeds : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieTallGrassTile>());
        Item.value = Item.sellPrice(silver: 5);
        // Vanilla flower seeds don't have auto-reuse.
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation;
    }
}

public sealed class AerieTallGrassFlowerSeeds : AerieTallGrassSeeds
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.placeStyle = 1;
    }
}

public sealed class AerieTallGrassTile : ModTile
{
    [OnLoad]
    private static new void Load()
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
                    if (type != ModContent.TileType<AerieTallGrassTile>())
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

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(0, 0, 1);

        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 7;
        TileObjectData.newTile.StyleMultiplier = 7;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.RandomStyleRange = 3;
        TileObjectData.newAlternate.StyleMultiplier = 3;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.DrawFlipMode[Type] = 1;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        AddMapEntry(new Color(185, 168, 72));
        // DustType = ModContent.DustType<AerieGrassDust>();
        HitSound = SoundID.Grass;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        WorldGen.PlantCheck(i, j);
        return false;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        // Vanilla flower seeds change random style after each placement.
        TileObjectPreviewData.randomCache.Reset();

        TileObject.CanPlace(i, j, Type, item.placeStyle, Main.LocalPlayer.direction, out _, onlyCheck: true);
    }
}
