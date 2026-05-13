using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

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
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieStoneTileMerge.Asset,
            ModContent.TileType<AerieBrickErodedTile>()
        );

        AddMapEntry(new Color(103, 94, 78));

        DustType = -1;
        HitSound = SoundID.Tink;
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
            Assets.Images.Aerie.Placements.AerieStoneTileMerge.Asset,
            ModContent.TileType<AerieBrickErodedTile>()
        );

        HitSound = SoundID.Dig;
        DustType = ModContent.DustType<AerieGrassDust>();
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

        DustType = -1;
    }
}