using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Solstice.Content.Aerie;

public sealed class Censer : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.Censer.KEY;

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CenserTile>());
        Item.value = Item.buyPrice(copper: 10);
    }
}

public sealed class CenserTile : ModTile
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

    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        if (TileObjectData.IsTopLeft(tile) && Main.rand.NextBool(2))
        {
            var dust = Dust.NewDustPerfect(new Vector2(i, j).ToWorldCoordinates(16) + new Vector2(Main.rand.NextFloat(-8, 8), 10), DustID.Smoke, -Vector2.UnitY * Main.rand.NextFloat(0f, 0.6f) + new Vector2(Main.windSpeedCurrent, 0), 100, default, 0.1f);
            dust.fadeIn = 1;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];

        if (TileObjectData.IsTopLeft(tile))
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);

        return false;
    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        overrideWindCycle = 1f;
        windPushPowerY = 0;
    }
}