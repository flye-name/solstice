using Solstice.Common;
using Solstice.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieBrickDust : ModDust
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickDust.KEY;

    public override void OnSpawn(Dust dust)
    {
        dust.rotation = Main.rand.NextFloatDirection();
        dust.frame = new Rectangle(0, Main.rand.Next(6) * 16, 16, 16);
    }

    public override bool Update(Dust dust)
    {
        dust.rotation += 0.07f * dust.velocity.X;

        dust.scale *= 0.98f;
        dust.velocity.Y += 0.06f;

        dust.velocity.X *= 0.98f;

        dust.position += dust.velocity;

        if (dust.scale < 0.15f)
        {
            dust.active = false;
        }

        return false;
    }
}

public sealed class AerieBrick : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrick.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe()
           .AddIngredient<AerieBrickWall>(4)
           .AddTile(TileID.WorkBenches)
           .Register();

        CreateRecipe()
           .AddIngredient<AerieStone>(2)
           .AddTile(TileID.Furnaces)
           .Register();
    }
}

public class AerieBrickTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        Main.tileBrick[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        /*TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieBrickTileMerge.Asset,
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>(),
            ModContent.TileType<AerieBrickErodedTile>()
        );*/

        AddMapEntry(new Color(147, 144, 131));

        DustType = ModContent.DustType<AerieBrickDust>();
        HitSound = SoundID.Tink;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 4;
    }
}

public sealed class AerieBrickWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickWall.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieBrickWallTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
           .AddIngredient<AerieBrick>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public sealed class AerieBrickWallTile : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickWallTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        WallID.Sets.AllowsWind[Type] = true;

        AddMapEntry(new Color(100, 98, 90));

        DustType = ModContent.DustType<AerieBrickDust>();
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 4;
    }
}