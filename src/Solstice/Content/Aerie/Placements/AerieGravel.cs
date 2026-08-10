using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieGravel : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieGravel.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieGravelTile>());
    }
}

public class AerieGravelTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieGravelTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        
        Main.tileMerge[Type][ModContent.TileType<AerieBrickTile>()] = true;
        Main.tileMerge[Type][ModContent.TileType<AerieStoneTile>()] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(95, 90, 70));
        HitSound = SoundID.Dig;
    }
}