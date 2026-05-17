using Daybreak.Common.Features.Hooks;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieCloudDust : ModDust
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudDust.KEY;

    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;
}

public sealed class AerieCloud : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloud.KEY;

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

// MaskedTile is used to place clouds in the foreground
public sealed class AerieCloudTile : MaskedTile
{
    public override void Load()
    {
        On_Main.DrawInfernoRings += DrawInfernoRings_DrawTarget;
    }

    private void DrawInfernoRings_DrawTarget(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        RenderMaskedTiles();

        orig(self);
    }

    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudTile.KEY;

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
            Assets.Images.Aerie.Placements.AerieCloudTileMerge.Asset,
            ModContent.TileType<AerieBrickTile>(),
            ModContent.TileType<AerieBrickGrassTile>(),
            ModContent.TileType<AerieBrickErodedTile>(),
            ModContent.TileType<AerieCeramicTile>(),
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>(),
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

    protected override void RenderIntoMask(int i, int j)
    {
        TileDrawing.Instance.DrawSingleTile(new TileDrawInfo(), false, -1, Main.screenPosition, Vector2.Zero, i, j);

        Tile tile = Framing.GetTileSafely(i, j);
        Tile above = Framing.GetTileSafely(i, j - 1);

        if (above.TileType == Type)
        {
            return;
        }

        bool merges = Main.tileMerge[Type][above.type] || Main.tileMerge[above.type][Type];

        if ((merges && !tile.IsHalfBlock)
         || (above.IsTileInvisible && !TileDrawing.Instance._shouldShowInvisibleBlocks)
         || tile.Slope > 0)
        {
            return;
        }

        var offset = new Vector2(0, -6);

        TileDrawing.Instance.DrawSingleTile(new TileDrawInfo(), false, -1, Main.screenPosition, offset, i, j);
    }
}

public sealed class AerieCloudWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudWall.KEY;

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
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudWallTile.KEY;

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
