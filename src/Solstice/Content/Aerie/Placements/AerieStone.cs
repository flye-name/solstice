using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieStoneDust : ModDust
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStoneDust.KEY;
}

public sealed class AerieStone : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStone.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieStoneTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(2)
           .AddIngredient<AerieBrick>()
           .AddTile(TileID.WorkBenches)
           .Register();

        CreateRecipe(2)
           .AddIngredient<AerieBrickEroded>()
           .AddTile(TileID.WorkBenches)
           .Register();

        CreateRecipe()
           .AddIngredient<AerieStoneWall>(4)
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieStoneTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStoneTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieStoneTileMerge.Asset,
            ModContent.TileType<AerieBrickErodedTile>()
        );

        AddMapEntry(new Color(103, 94, 78));

        DustType = ModContent.DustType<AerieStoneDust>();
        HitSound = SoundID.Tink;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        //offset every other X pos
        Tile tile = Main.tile[i, j];
        int frameX = tile.TileFrameX / 18;
        int frameY = tile.TileFrameY / 18;
        int numX = 0;
        int numY = 0;
        bool isLeft = i % 2 == 0;
        if ((frameX == 0 || frameX == 4 || frameX == 5 || frameX == 9 || frameX == 10 || frameX == 11 || frameX == 12) && frameY <= 2)
            numY = isLeft ? 0 : 1;
        if (frameX <= 3 && frameX >= 1 && frameY <= 3)
            numX = isLeft ? 1 : 2;
        if (frameX <= 8 && frameX >= 6 && frameY <= 4)
            numX = isLeft ? 6 : 7;
        if (frameX <= 11 && frameX >= 9 && frameY == 4)
            numX = isLeft ? 9 : 10;

        if ((frameX == 0 || frameX == 2 || frameX == 4) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 0 : 2;
        if ((frameX == 1 || frameX == 3 || frameX == 5) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 1 : 3;

        if (numX != 0)
            tileFrameX = (short)(numX * 18);
        if (numY != 0)
            tileFrameY = (short)(numY * 18);

        //offset every other Y pos
        if (j % 2 == 1)
            tileFrameY += 90;
    }
}

public sealed class AerieStoneGrassTile : AerieStoneTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStoneGrassTile.KEY;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        RegisterItemDrop(ModContent.ItemType<AerieStone>(), 0);

        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<AerieStoneTile>()] = true;
        Main.tileMerge[ModContent.TileType<AerieStoneTile>()][Type] = true;

        SolsticeTileSets.TransformTo[Type] = ModContent.TileType<AerieStoneTile>();

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieStoneGrassTileMerge.Asset,
            ModContent.TileType<AerieBrickErodedTile>()
        );

        HitSound = SoundID.Dig;
        DustType = ModContent.DustType<AerieGrassDust>();
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        //offset every other X pos
        Tile tile = Main.tile[i, j];
        int frameX = tile.TileFrameX / 18;
        int frameY = tile.TileFrameY / 18;
        int numX = 0;
        int numY = 0;
        bool isLeft = i % 2 == 0;
        if ((frameX == 0 || frameX == 4 || frameX == 5 || frameX == 9 || frameX == 10 || frameX == 11 || frameX == 12) && frameY <= 2)
            numY = isLeft ? 0 : 1;
        if (frameX <= 3 && frameX >= 1 && frameY <= 3)
            numX = isLeft ? 1 : 2;
        if (frameX <= 8 && frameX >= 6 && frameY <= 4)
            numX = isLeft ? 6 : 7;
        if (frameX <= 11 && frameX >= 9 && frameY == 4)
            numX = isLeft ? 9 : 10;

        if ((frameX == 0 || frameX == 2 || frameX == 4) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 0 : 2;
        if ((frameX == 1 || frameX == 3 || frameX == 5) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 1 : 3;

        if (numX != 0)
            tileFrameX = (short)(numX * 18);
        if (numY != 0)
            tileFrameY = (short)(numY * 18);

        //offset every other Y pos
        if (j % 2 == 1)
            tileFrameY += 90;
    }
}

public sealed class AerieStoneWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStoneWall.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieStoneWallTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
           .AddIngredient<AerieStone>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieStoneWallTile : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieStoneWallTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        WallID.Sets.AllowsWind[Type] = true;

        AddMapEntry(new Color(54, 48, 39));

        DustType = ModContent.DustType<AerieStoneDust>();
    }
}