using Daybreak.Common.Rendering;
using Godseeker.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace Godseeker.Content.Aerie;

public sealed class AerieCloudDust : ModDust
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudDust.KEY;

    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;
}

public sealed class AerieCloud : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloud.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieCloudTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe()
           .AddIngredient<AerieCloudWall>(4)
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieCloudTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;

        TileID.Sets.MergesWithClouds[Type] = true;
        TileID.Sets.Clouds[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        TileID.Sets.NegatesFallDamage[Type] = true;

        TileMerging.AddCustomMerge(
            Type,
            Assets.Images.Aerie.Placements.AerieCloudTileMerge.Asset,
            ModContent.TileType<AerieBrickTile>(),
            ModContent.TileType<AerieBrickGrassTile>(),
            ModContent.TileType<AerieBrickErodedTile>(),
            ModContent.TileType<AerieCeramicTile>(),
            ModContent.TileType<AerieStoneTile>(),
            ModContent.TileType<AerieStoneGrassTile>(),
            TileID.Cloud,
            TileID.RainCloud,
            TileID.SnowCloud
        );

        AddMapEntry(new Color(246, 234, 215));

        DustType = ModContent.DustType<AerieCloudDust>();
    }

    public override void PostSetDefaults()
    {
        Main.tileNoSunLight[Type] = false;
    }

    public RenderTargetLease? target;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        target?.Dispose();
        target = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice);

        using (target?.Scope(clearColor: Color.Transparent))
        {
            Main.instance.TilesRenderer.DrawSingleTile(new(), false, -1, Main.screenPosition, new(Main.offScreenRange), i, j);
        }

        var shader = Assets.Effects.AerieCloudOverlay.CreateAerieCloudOverlay();

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Shader, Matrix.Identity);

        Main.spriteBatch.Draw(target?.Target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(ss);

        return false;
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustType;
        makeDust = true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public sealed class AerieCloudWall : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudWall.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AerieCloudWallTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
           .AddIngredient<AerieCloud>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieCloudWallTile : ModWall
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudWallTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        Main.wallLight[Type] = true;

        WallID.Sets.AllowsWind[Type] = true;

        AddMapEntry(new Color(190, 168, 156));

        DustType = ModContent.DustType<AerieCloudDust>();
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
