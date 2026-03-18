using GodseekerBoss.Common.IDs;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace GodseekerBoss.Content.Aerie.Placements;

public sealed class AerieBrick : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<AerieBrickTile>();
        Item.width = 12;
        Item.height = 12;
    }
}

public class AerieBrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        Main.tileBrick[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(138, 158, 168));

        DustType = -1;
        HitSound = SoundID.Tink;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        if (j % 2 == 0)
        {
            tileFrameY += 270;
        }
    }
}

public sealed class AerieBrickGrassTile : AerieBrickTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        RegisterItemDrop(ModContent.ItemType<AerieBrick>(), 0);

        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<AerieBrickTile>()] = true;
        Main.tileMerge[ModContent.TileType<AerieBrickTile>()][Type] = true;

        GodseekerTileSets.SwapToOnFailedHit[Type] = ModContent.TileType<AerieBrickTile>();

        HitSound = SoundID.Dig;
    }
}