using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class PaperbarkShingles : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.PaperbarkShingles.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PaperbarkShinglesTile>());
    }
}

public class PaperbarkShinglesTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.PaperbarkShinglesTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(73, 64, 45));
        HitSound = SoundID.Dig;
    }
}