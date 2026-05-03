using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Godseeker.Content.Aerie;

public class CenserTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.CenserTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.MultiTileSway[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.Origin = new(1, 0);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = new(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new(AnchorType.PlatformNonHammered, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.DrawYOffset = -12;
        TileObjectData.addAlternate(0);

        AddMapEntry(new(199, 162, 82));

        TileObjectData.addTile(Type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];

        if (TileObjectData.IsTopLeft(tile))
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);

        return false;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {

    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        overrideWindCycle = 1f;
        windPushPowerY = 0;
    }
}

public class Censer : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.Censer.KEY;

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CenserTile>());
        Item.value = Item.buyPrice(copper: 10);
    }
}