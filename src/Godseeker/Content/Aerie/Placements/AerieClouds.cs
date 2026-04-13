using Godseeker.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie;

public class AerieCloudDust : ModDust
{
    public override string Texture => PlacementTextures.AerieCloudDust.Key;

    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;
}

public sealed class AerieCloud : ModItem
{
    public override string Texture => PlacementTextures.AerieCloud.Key;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieCloudTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe()
           .AddIngredient<AerieCloudWall>(4)
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieCloudTile : ModTile
{
    public override string Texture => PlacementTextures.AerieCloudTile.Key;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;

        TileID.Sets.MergesWithClouds[Type] = true;
        TileID.Sets.Clouds[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        TileID.Sets.NegatesFallDamage[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            PlacementTextures.AerieCloudTileMerge,
            ModContent.TileType<AerieBrickTile>(),
            ModContent.TileType<AerieBrickGrassTile>(),
            ModContent.TileType<AerieCeramicTile>(),
            ModContent.TileType<AerieStoneTile>(),
            TileID.Cloud,
            TileID.RainCloud,
            TileID.SnowCloud
        );

        AddMapEntry(new Color(246, 234, 215));

        DustType = ModContent.DustType<AerieCloudDust>();
    }

    public override void PostSetDefaults()
    {
        Main.tileNoSunLight[Type] = false;
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustType;
        makeDust = true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public sealed class AerieCloudWall : ModItem
{
    public override string Texture => PlacementTextures.AerieCloudWall.Key;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieCloudWallTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
           .AddIngredient<AerieCloud>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieCloudWallTile : ModWall
{
    public override string Texture => PlacementTextures.AerieCloudWallTile.Key;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        Main.wallLight[Type] = true;

        WallID.Sets.AllowsWind[Type] = true;

        AddMapEntry(new Color(190, 168, 156));

        DustType = ModContent.DustType<AerieCloudDust>();
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
