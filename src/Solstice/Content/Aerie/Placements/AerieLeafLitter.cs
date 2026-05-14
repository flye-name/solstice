using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using ReLogic.Content;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Solstice.Core.TileMerging;

namespace Solstice.Content.Aerie;

public static class AerieLeafLitterRendering
{
    private static readonly HashSet<Point> leafPositions = [];

    [OnLoad]
    private static void Load()
    {
        IL_Main.DoDraw += DoDraw_RenderOrder;

        On_Main.DoDraw_WallsTilesNPCs += DoDraw_WallsTilesNPCs_Clear;
        On_TileDrawing.Draw += Draw_Clear;

        On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill_RemoveLitter;

        On_WorldGen.KillTile += KillTile_RemoveLitter;

        On_Player.PickWall += PickWall_RemoveLitter;

        On_TileDrawing.PostDrawTiles += PostDrawTiles_LeafLitter;
        On_Main.DoDraw_WallsAndBlacks += DoDraw_WallsAndBlacks_LeafLitter;
    }

    // Don't do this.
    private static void DoDraw_RenderOrder(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcI4(3),
            i => i.MatchBneUn(out _),
            i => i.MatchLdarg(out _),
            i => i.MatchCall<Main>(nameof(Main.RenderTiles))
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<Main>(nameof(Main.renderCount)),
            i => i.MatchLdsfld<Lighting>(nameof(Lighting.LegacyEngine)),
            i => i.MatchCallvirt<LegacyLighting>($"get_{nameof(LegacyLighting.Mode)}")
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcI4(2)
        );

        c.EmitPop();
        c.EmitLdcI4(3);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcI4(0)
        );

        c.EmitPop();
        c.EmitLdcI4(3);
    }

    private static void DoDraw_WallsTilesNPCs_Clear(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
    {
        if (Main.drawToScreen)
        {
            leafPositions.Clear();
        }

        orig(self);
    }

    private static void Draw_Clear(On_TileDrawing.orig_Draw orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
    {
        if (solidLayer && intoRenderTargets)
        {
            leafPositions.Clear();
        }

        orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
    }
    private static bool DoesPickTargetTransformOnKill_RemoveLitter(On_Player.orig_DoesPickTargetTransformOnKill orig, Player self, HitTile hitCounter, int damage, int x, int y, int pickPower, int bufferIndex, Tile tileTarget)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        var tileData = tile.Get<SolsticeTileData>();

        if (tileData.HasLeafLitterTile && hitCounter.AddDamage(bufferIndex, damage, updateAmount: false) >= 100)
        {
            return true;
        }

        return orig(self, hitCounter, damage, x, y, pickPower, bufferIndex, tileTarget);
    }

    private static void KillTile_RemoveLitter(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (!tile.HasTile || !tileData.HasLeafLitterTile)
        {
            orig(i, j, fail, effectOnly, noItem);
            return;
        }

        if (!fail || effectOnly)
        {
            return;
        }

        RemoveLeafEffect(i, j, false);
        tileData.HasLeafLitterTile = false;
    }

    private static void PickWall_RemoveLitter(On_Player.orig_PickWall orig, Player self, int x, int y, int damage)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (tileData.HasLeafLitterWall)
        {
            RemoveLeafEffect(x, y, true);
            tileData.HasLeafLitterWall = false;

            return;
        }

        orig(self, x, y, damage);
    }

    private static void RemoveLeafEffect(int i, int j, bool wall)
    {
        int count = Main.rand.Next(3, 8);

        var position = new Vector2(i * 16f, j * 16f);
        position += new Vector2(8);

        Color color = wall ? new Color(180, 180, 180) : Color.White;

        SoundEngine.PlaySound(SoundID.Grass, position);

        for (int k = 0; k < count; k++)
        {
            var offset = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

            var velocity = Vector2.Normalize(offset) * Main.rand.NextFloat(0.3f, 2.1f);
            velocity -= Vector2.UnitY * 0.6f;

            Dust.NewDustPerfect(position + offset, ModContent.DustType<AerieGrassDust>(), velocity, newColor: color);
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

        offset += wall ? 40 : 0;

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
    private static readonly Dictionary<int, AerieLeafRenderTargetHolder> paintCache = [];

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

        if (paintCache.TryGetValue(paintColor, out AerieLeafRenderTargetHolder? holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new AerieLeafRenderTargetHolder(paintColor, asset);

        paintCache[paintColor] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

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
        Tile tile = Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y];

        if (!tile.HasTile && !tile.HasWall)
        {
            return base.UseItem(player);
        }
        
        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (tile.HasTile && Main.tileSolid[tile.TileType] && !tileData.HasLeafLitterTile)
        {
            tileData.HasLeafLitterTile = true;
            return true;
        }

        if (tile.HasWall && !tileData.HasLeafLitterWall)
        {
            tileData.HasLeafLitterWall = true;
        }

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