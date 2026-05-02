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
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie.Placements;

public class WindChimeTile : ModTile
{
    public override string Texture => PlacementTextures.WindChimeTile.Key;

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
        // TODO: reimplement ambient sounds

        /*if (Main.gamePaused)
            return;

        SoundStyle soundStyle = new("VoidseekerBoss/Assets/WindChime", 3)
        {
            Volume = 0.4f,
            PitchVariance = 0.1f,
            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
            PauseBehavior = PauseBehavior.PauseWithGame
        };

        if (MathF.Abs(Main.windSpeedCurrent) > 0.4f)
            if (Main.rand.NextBool(30))
                SoundEngine.PlaySound(soundStyle, new Vector2(i, j).ToWorldCoordinates());

        float length = Main.LocalPlayer.velocity.Length();
        float mag = MathHelper.Clamp(length / 10f, 0, 1);
        float chance = 1f - mag;

        soundStyle = new("VoidseekerBoss/Assets/WindChime", 3)
        {
            Volume = 0.4f,
            PitchVariance = 0.1f,
            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
            PauseBehavior = PauseBehavior.PauseWithGame
        };

        if (Main.rand.NextFloat(chance) < 0.1f)
        {
            if (Main.LocalPlayer.velocity.Length() < 2 || !new Rectangle(i * 16, j * 16, 16, 16).Intersects(Main.LocalPlayer.getRect()))
                return;

            SoundEngine.PlaySound(soundStyle, new Vector2(i, j).ToWorldCoordinates());
        }*/
    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        overrideWindCycle = 1f;
        windPushPowerY = 0;
    }
}

public class WindChime : ModItem
{
    public override string Texture => PlacementTextures.WindChime.Key;

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<WindChimeTile>());
        Item.value = Item.buyPrice(copper: 10);
    }
}