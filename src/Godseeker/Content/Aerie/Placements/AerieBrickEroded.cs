using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie;

public sealed class AerieBrickEroded : ModItem
{
    public override string Texture => PlacementTextures.AerieBrickEroded.Key;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickErodedTile>());
    }
}

public class AerieBrickErodedTile : ModTile
{
    public override string Texture => PlacementTextures.AerieBrickErodedTile.Key;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(138, 158, 168));

        DustType = -1;
        HitSound = SoundID.Tink;
    }
}