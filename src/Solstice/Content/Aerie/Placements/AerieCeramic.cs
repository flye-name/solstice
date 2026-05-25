using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieCeramic : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCeramic.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieCeramicTile>());
    }

    public override void AddRecipes()
    {
        // TODO
    }
}

public class AerieCeramicTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCeramicTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            useCorners: true,
            Assets.Images.Aerie.Placements.AerieCeramicTileMerge.Asset,
            ModContent.TileType<AerieBrickTile>(),
            ModContent.TileType<AerieBrickGrassTile>(),
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>()
        );

        AddMapEntry(new Color(108, 93, 78));

        DustType = ModContent.DustType<AerieStoneDust>();

        HitSound = SoundID.Tink;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}