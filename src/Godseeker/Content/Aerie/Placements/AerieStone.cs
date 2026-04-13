using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie;

public sealed class AerieStone : ModItem
{
    public override string Texture => PlacementTextures.AerieStone.Key;

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
        // TODO
    }
}

public class AerieStoneTile : ModTile
{
    public override string Texture => PlacementTextures.AerieStoneTile.Key;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(147, 144, 131));

        DustType = -1;
        HitSound = SoundID.Tink;
    }
}