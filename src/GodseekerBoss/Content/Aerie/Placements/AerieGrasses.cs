using GodseekerBoss.Common.IDs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
// ReSharper disable InconsistentNaming

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

public class TallAerieGrassSeeds : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tall1x1AerieGrassTile>());
        Item.value = Item.sellPrice(silver: 5);
        // Vanilla flower seeds don't have auto-reuse.
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation;
    }
}

public sealed class TallAerieGrassFlowerSeeds : TallAerieGrassSeeds
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.placeStyle = 1;
    }
}

public sealed class Tall1x1AerieGrassTile : ModTile
{
    public override string Texture => GeneratedAssets.Content.Aerie.Placements.Textures.TallAerieGrassTile.Key;

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

        GodseekerTileSets.UseAlternateTileObjectDataRandomStyles[Type] = true;

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

public sealed class Tall1x2AerieGrassTile : ModTile
{
    public override string Texture => GeneratedAssets.Content.Aerie.Placements.Textures.TallerAerieGrassTile.Key;

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(0, 0, 1);

        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 4;
        TileObjectData.newTile.StyleMultiplier = 4;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.RandomStyleRange = 4;
        TileObjectData.newAlternate.StyleMultiplier = 4;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.DrawFlipMode[Type] = 1;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        GodseekerTileSets.UseAlternateTileObjectDataRandomStyles[Type] = true;

        AddMapEntry(new Color(185, 168, 72));
        // DustType = ModContent.DustType<AerieGrassDust>();
        HitSound = SoundID.Grass;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        WorldGen.PlantCheck(i, j);
        return false;
    }
}

