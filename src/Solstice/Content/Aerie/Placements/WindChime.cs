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

namespace Solstice.Content.Aerie;

public sealed class WindChime : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.WindChime.KEY;

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<WindChimeTile>());
        Item.value = Item.buyPrice(copper: 10);
    }
}

public sealed class WindChimeTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.WindChimeTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.MultiTileSway[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);

        TileObjectData.newTile.Origin = new Point16(0, 0);

        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, 1, 0);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;

        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.PlatformNonHammered, 1, 0);
        TileObjectData.newAlternate.DrawYOffset = -12;
        TileObjectData.addAlternate(0);
        TileObjectData.newTile.LavaDeath = true;

        TileObjectData.addTile(Type);

        AddMapEntry(new(199, 162, 82));
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
        if (Main.gamePaused)
            return;

        var tilePos = new Vector2(i, j).ToWorldCoordinates();

        if (MathF.Abs(Main.windSpeedCurrent) > 0.4f && Main.rand.NextBool(120))
        {
            SoundEngine.PlaySound(
                Assets.Sounds.Decorative.Windchime.Asset with
                {
                    Type = SoundType.Ambient,
                    MaxInstances = 2,
                    pitchVariance = 0.2f,
                    Volume = 0.03f,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
                    PauseBehavior = PauseBehavior.PauseWithGame
                },
                tilePos
            );
        }
    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        overrideWindCycle = 1f;
        windPushPowerY = 0;
    }
}
