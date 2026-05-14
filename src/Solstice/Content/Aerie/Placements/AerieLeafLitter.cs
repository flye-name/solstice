using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Solstice.Content.Aerie;

public static class AerieLeafLitterRendering
{
    private static readonly HashSet<Point> leafPositions = [];

    [OnLoad]
    private static void Load()
    {
        On_TileDrawing.Draw += Draw_Clear;

        On_TileDrawing.PostDrawTiles += PostDrawTiles_LeafLitter;
        On_Main.DoDraw_WallsAndBlacks += DoDraw_WallsAndBlacks_LeafLitter;
    }

    private static void Draw_Clear(On_TileDrawing.orig_Draw orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
    {
        orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);

        if (solidLayer)
        {
            leafPositions.Clear();
        }
    }

    private static void PostDrawTiles_LeafLitter(On_TileDrawing.orig_PostDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets)
    {
        orig(self, solidLayer, forRenderTargets, intoRenderTargets);

        if (!solidLayer || intoRenderTargets)
        {
            return;
        }

        var sb = Main.spriteBatch;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        {
            foreach ((int i, int j) in leafPositions)
            {
                Tile tile = Framing.GetTileSafely(i, j);

                var tileData = tile.Get<SolsticeTileData>();

                if (!tile.HasTile || !TileDrawing.IsVisible(tile) || !tileData.HasLeafLitterTile)
                {
                    continue;
                }

                DrawLeafOverlay(i, j, false);
            }
        }
        sb.End();
    }

    private static void DoDraw_WallsAndBlacks_LeafLitter(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        orig(self);

        var sb = Main.spriteBatch;

        sb.End(out var ss);
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        {
            foreach ((int i, int j) in leafPositions)
            {
                Tile tile = Framing.GetTileSafely(i, j);

                var tileData = tile.Get<SolsticeTileData>();

                if (!tile.HasWall || tile.IsWallInvisible || !tileData.HasLeafLitterWall)
                {
                    continue;
                }

                DrawLeafOverlay(i, j, true);
            }
        }
        sb.Restart(in ss);
    }

    [GlobalTileHooks.PostDraw]
    private static void Tile_PostDraw_AddPoint(int i, int j, int type, SpriteBatch spriteBatch)
    {
        leafPositions.Add(new Point(i, j));
    }

    [GlobalWallHooks.PostDraw]
    private static void Wall_PostDraw_AddPoint(int i, int j, int type, SpriteBatch spriteBatch)
    {
        leafPositions.Add(new Point(i, j));
    }

#region Drawing
    private static void DrawLeafOverlay(int i, int j, bool wall)
    {
        const int frame_count = 4;
        const int frame_width = 24;

        const float sway_freq = 1f / 20f;

        var asset = Assets.Images.Aerie.Placements.AerieGrassWallTile_Leaves.Asset;

        var sb = Main.spriteBatch;

        Tile tile = Framing.GetTileSafely(i, j);

        var tileData = tile.Get<SolsticeTileData>();

        int offset = i * j;

        float swayAmp = Main.WindForVisuals * 2f;

        float sway = MathF.Sin((Main.GameUpdateCount * sway_freq) + offset) * swayAmp;

        int frameHeight = asset.Value.Height / frame_count;
        var frame = new Rectangle(wall ? 0 : frame_width, new FastRandom(offset).Next(frame_count) * frameHeight, frame_width, frameHeight);

        var position = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
        position += new Vector2(8);
        position.X += sway;

        float rotation = offset + (sway * 0.05f);

        int paint = wall ? tile.WallColor : tile.TileColor;

        if (wall
          ? tileData.LeafLitterNoPaintWall
          : tileData.LeafLitterNoPaintTile)
        {
            paint = 0;
        }

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        Color color = TileUtils.GetDrawColor(i, j, false, useColor);

        sb.Draw(texture, position, frame, color, rotation, frame.Size() * 0.5f, 1f, SpriteEffects.None, 0);
    }
#endregion

#region Paint
    private static AerieLeafRenderTargetHolder? paintInstance;

    internal class AerieLeafRenderTargetHolder(int paintColor, Asset<Texture2D> asset, int copySettingsFrom = -1) : TilePaintSystemV2.ARenderTargetHolder
    {
        public int PaintColor = paintColor;

        public TreePaintingSettings PaintSettings = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);

        public Asset<Texture2D> Texture = asset;

        public override void Prepare()
        {
            Texture.Wait();

            PrepareTextureIfNecessary(Texture.Value);
        }

        public override void PrepareShader()
        {
            PrepareShader(PaintColor, PaintSettings);
        }
    }

    private static bool TryGetPaintTexture(
        int paintColor,
        Asset<Texture2D> asset,
        [NotNullWhen(true)] out Texture2D? texture
    )
    {
        texture = null;

        if (paintInstance?.IsReady is true)
        {
            texture = paintInstance.Target;

            return true;
        }

        paintInstance = new AerieLeafRenderTargetHolder(paintColor, asset);

        Main.instance.TilePaintSystem._requests.Add(paintInstance);

        return false;
    }
#endregion
}

public class AerieLeafLitter : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieLeaf.KEY;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Paintbrush);
    }

    public override bool? UseItem(Player player)
    {
        if (!Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].HasTile && Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].WallType == 0)
            return base.UseItem(player);
        
        ref var tileData = ref Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].Get<SolsticeTileData>();
        tileData.HasLeafLitterTile = !tileData.HasLeafLitterTile;
        tileData.HasLeafLitterWall = !tileData.HasLeafLitterWall;
        return true;
    }
}

public class AerieLeafCoatingPaintSeparator : ModItem
{
    public override string Texture => Assets.Images.AerieFlute.KEY;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Paintbrush);
    }

    public override bool? UseItem(Player player)
    {
        if (!Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].HasTile && Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].WallType == 0)
            return base.UseItem(player);
        
        ref var tileData = ref Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].Get<SolsticeTileData>();
        tileData.LeafLitterNoPaintTile = !tileData.LeafLitterNoPaintTile;
        tileData.LeafLitterNoPaintWall = !tileData.LeafLitterNoPaintWall;
        return true;
    }
}