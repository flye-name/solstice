using Daybreak.Common.Rendering;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Solstice.Content.Aerie.Placements;

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

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => AerieLeafCoatingDrawing.DrawLeafOverlay(i, j, true);
}

// used in AerieLeafCoatingDrawing
public class AerieGrassWallLeavesFake : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieGrassWallTile_Leaves.KEY;
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