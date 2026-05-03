using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Godseeker.Content.Aerie;

public sealed class GodspeedPainting : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.GodspeedPainting.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
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
        Item.createTile = ModContent.TileType<GodspeedPaintingTile>();
        Item.width = 40;
        Item.height = 40;
    }
}

public sealed class GodspeedPaintingTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.GodspeedPaintingTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.Width = 6;
        TileObjectData.newTile.Height = 6;
        TileObjectData.newTile.Origin = new Point16(3, 3);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        DustType = DustID.WoodFurniture;

        AddMapEntry(new Color(99, 50, 30), Language.GetText("MapObject.Painting"));
    }
}