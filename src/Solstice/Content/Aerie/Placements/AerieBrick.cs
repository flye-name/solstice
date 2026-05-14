using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieBrick : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrick.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe()
           .AddIngredient<AerieBrickWall>(4)
           .AddTile(TileID.WorkBenches)
           .Register();

        // TODO: Recipe groups
        CreateRecipe()
           .AddIngredient<AerieBrickEroded>()
           .AddTile(TileID.Furnaces)
           .Register();

        CreateRecipe()
           .AddIngredient<AerieStone>(2)
           .AddTile(TileID.Furnaces)
           .Register();
    }
}

public class AerieBrickTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        Main.tileBrick[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieBrickTileMerge.Asset,
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>(),
            ModContent.TileType<AerieBrickErodedTile>()
        );

        AddMapEntry(new Color(138, 158, 168));

        DustType = DustID.Tin;
        HitSound = SoundID.Tink;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        if (j % 2 == 0)
        {
            tileFrameY += 270;
        }
    }
}

public sealed class AerieBrickGrassTile : AerieBrickTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickGrassTile.KEY;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        RegisterItemDrop(ModContent.ItemType<AerieBrick>(), 0);

        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<AerieBrickTile>()] = true;
        Main.tileMerge[ModContent.TileType<AerieBrickTile>()][Type] = true;

        SolsticeTileSets.TransformTo[Type] = ModContent.TileType<AerieBrickTile>();

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieBrickGrassTileMerge.Asset,
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>(),
            ModContent.TileType<AerieBrickErodedTile>()
        );

        HitSound = SoundID.Dig;

        DustType = ModContent.DustType<AerieGrassDust>();
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 3 : 6;
    }
}

public sealed class AerieBrickWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickWall.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieBrickWallTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
           .AddIngredient<AerieBrick>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public sealed class AerieBrickWallTile : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickWallTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        WallID.Sets.AllowsWind[Type] = true;

        AddMapEntry(new Color(100, 98, 90));

        DustType = DustID.Tin;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}