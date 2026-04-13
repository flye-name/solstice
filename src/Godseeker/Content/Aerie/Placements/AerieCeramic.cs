using Godseeker.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie;

public sealed class AerieCeramic : ModItem
{
    public override string Texture => PlacementTextures.AerieCeramic.Key;

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
    public override string Texture => PlacementTextures.AerieCeramicTile.Key;

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
            PlacementTextures.AerieCeramicTileMerge,
            ModContent.TileType<AerieBrickTile>(),
            ModContent.TileType<AerieBrickGrassTile>(),
            ModContent.TileType<AerieStoneTile>()
        );

        AddMapEntry(new Color(108, 93, 78));

        DustType = -1;
        HitSound = SoundID.Tink;
    }
}