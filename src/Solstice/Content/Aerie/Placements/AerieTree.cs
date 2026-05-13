using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Solstice.Content.Aerie.Placements;

public class AerieSapling : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieSapling.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;

        TileObjectData.newTile.Origin = new(0, 1);

        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<AerieStoneTile>(), ModContent.TileType<AerieStoneGrassTile>()];

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaDeath = true;

        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.addTile(Type);

        AddMapEntry(new(200, 200, 200), Language.GetText("MapObject.Sapling"));

        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = ModContent.DustType<AerieGrassDust>();
        AdjTiles = [TileID.Saplings];
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void RandomUpdate(int i, int j)
    {
        if (Main.rand.NextBool(20))
            if (WorldGen.GrowTree(i, j) && WorldGen.PlayerLOS(i, j))
                WorldGen.TreeGrowFXCheck(i, j);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
    {
        WorldGen.GrowTree(i, j);

        if (i % 2 == 0)
            effects = SpriteEffects.FlipHorizontally;
    }
}

public class AerieTree : ModTree
{
    public override Asset<Texture2D> GetTexture() => Assets.Images.Aerie.Placements.AerieTree.Asset;

    public override Asset<Texture2D> GetBranchTextures() => Assets.Images.Aerie.Placements.AerieTree_Branch.Asset;

    public override Asset<Texture2D> GetTopTextures() => Assets.Images.Aerie.Placements.AerieTree_Crown.Asset; // ozzatree

    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override int DropWood() => ItemID.FogboundDye;

    public override int TreeLeaf() => GoreID.TreeLeaf_Jungle;

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<AerieStoneTile>(), ModContent.TileType<AerieStoneGrassTile>()];
    }

    public override int SaplingGrowthType(ref int style)
    {
        style = 0;
        return ModContent.TileType<AerieSapling>();
    }
}