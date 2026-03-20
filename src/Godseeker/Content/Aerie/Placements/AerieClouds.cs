using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie.Placements;

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
}

public class AerieCloudTile : ModTile
{
    public override string Texture => PlacementTextures.AerieCloudTile.Key;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        Main.tileBlockLight[Type] = false;
        
        TileID.Sets.ChecksForMerge[Type] = true;

        TileID.Sets.NegatesFallDamage[Type] = true;

        AddMapEntry(new Color(246, 234, 215));

        DustType = ModContent.DustType<AerieCloudDust>();
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
}
