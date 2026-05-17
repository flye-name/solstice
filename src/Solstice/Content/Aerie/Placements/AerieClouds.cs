using Solstice.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

// TODO: reimplement tile particle emission, dust pixelation, send dust to tile target, make shader not look like shit
public sealed class AerieCloudDust : ModDust
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieCloudDust.KEY;

    public override void OnSpawn(Dust dust)
    {
        dust.noGravity = true;
    }

    public override bool PreDraw(Dust dust)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, texture.Bounds, Color.White, dust.rotation, texture.Size() / 2f, 1f, SpriteEffects.None, 0f);
        return false;
    }
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

public class AerieCloudTile : ShaderMaskedTile
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

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;

        AddMapEntry(new Color(246, 234, 215));

        DustType = ModContent.DustType<AerieCloudDust>();
    }

    public override void PostSetDefaults()
    {
        Main.tileNoSunLight[Type] = false;
    }

    protected override void ApplyShader()
    {
        Main.graphics.graphicsDevice.Textures[1] = Assets.Images.CoherentNoise.Asset.Value;

        var shader = Assets.Effects.AerieCloudOverlay.CreateAerieCloudOverlayShader();
        shader.Parameters.Time = Main.GlobalTimeWrappedHourly / 60;
        shader.Parameters.Zoom = 256f;
        shader.Parameters.ScreenOffset = Main.screenPosition;
        shader.Parameters.ScreenSize = new(Main.screenWidth, Main.screenHeight);

        shader.Apply();
    }

    protected override void RenderIntoMask(Point p)
    {
        Main.instance.TilesRenderer.DrawSingleTile(new(), false, -1, Main.screenPosition, Vector2.Zero, p.X, p.Y);
    }

    /*public RenderTargetLease? target;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        target?.Dispose();
        target = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice);

        using (target?.Scope(clearColor: Color.Transparent))
        {
            Main.instance.TilesRenderer.DrawSingleTile(new(), false, -1, Main.screenPosition, new(Main.offScreenRange), i, j);
        }

        var shader = Assets.Effects.AerieCloudOverlay.CreateAerieCloudOverlayShader();

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader.Shader, Matrix.Identity);

        Main.spriteBatch.Draw(target?.Target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        Main.spriteBatch.Restart(in ss);

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
    }*/
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
