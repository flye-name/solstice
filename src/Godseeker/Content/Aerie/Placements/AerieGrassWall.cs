using Daybreak.Common.Rendering;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Godseeker.Content.Aerie.Placements;

public class AerieGrassWallTile : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieGrassWallTile.KEY;

    public override bool Drop(int i, int j, ref int type) => false;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = false;

        DustType = ModContent.DustType<AerieGrassDust>();
        HitSound = SoundID.Grass;

        AddMapEntry(new(50, 140, 90));
    }

    public override void KillWall(int i, int j, ref bool fail)
    {
        fail = false;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Texture}_Leaves").Value;

        var offset = i * j;
        float sway = MathF.Sin(Main.GameUpdateCount / 20f + offset) * Main.windSpeedCurrent * 2f;

        int frameCount = 4;
        int frameHeight = texture.Height / frameCount;
        Rectangle rect = new(0, new UnifiedRandom(offset).Next(frameCount) * frameHeight, texture.Width, frameHeight);

        Main.spriteBatch.Draw(texture, new Vector2(i, j).ToWorldCoordinates() - Main.screenPosition + new Vector2(Main.offScreenRange + sway), rect, Lighting.GetColor(i, j), offset + sway * 0.05f, rect.Size() / 2f, 1, SpriteEffects.None, 0);
    }
}

public class AerieGrassWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieGrassWall.KEY;

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieGrassWallTile>());
        Item.useTime = Item.useAnimation;
    }
}