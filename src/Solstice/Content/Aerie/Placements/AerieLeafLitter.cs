using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Solstice.Common;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Light;
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
        On_Player.TryPainting += TryPainting_LeafPaint;
        On_Player.PlaceThing_PaintScrapper_TryScrapping += PlaceThing_PaintScrapper_TryScrapping_LeafPaint;

        IL_Main.DoDraw += DoDraw_RenderOrder;

        On_Main.DoDraw_WallsTilesNPCs += DoDraw_WallsTilesNPCs_Clear;
        On_TileDrawing.Draw += Draw_Clear;

        On_Player.DoesPickTargetTransformOnKill += DoesPickTargetTransformOnKill_RemoveLitter;

        On_WorldGen.KillTile += KillTile_RemoveLitter;

        On_Player.PickWall += PickWall_RemoveLitter;

        On_TileDrawing.PostDrawTiles += PostDrawTiles_LeafLitter;
        On_Main.DoDraw_WallsAndBlacks += DoDraw_WallsAndBlacks_LeafLitter;

        IL_WallDrawing.DrawWalls += DrawWalls_AddPoint;
    }

    private static void TryPainting_LeafPaint(On_Player.orig_TryPainting orig, Player self, int x, int y, bool paintingAWall, bool applyItemAnimation)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (!applyItemAnimation)
        {
            orig(self, x, y, paintingAWall, applyItemAnimation);
            return;
        }

        if (paintingAWall)
        {
            tileData.LeafLitterNoPaintWall = false;
        }
        else
        {
            tileData.LeafLitterNoPaintTile = false;
        }

        orig(self, x, y, paintingAWall, applyItemAnimation);
    }

    private static void PlaceThing_PaintScrapper_TryScrapping_LeafPaint(On_Player.orig_PlaceThing_PaintScrapper_TryScrapping orig, Player self, int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (!self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem ||
            (!tile.TileCoatedOrPainted && !tile.WallCoatedOrPainted))
        {
            orig(self, x, y);
            return;
        }

        if (tile.HasTile && tileData.HasLeafLitterTile && !tileData.LeafLitterNoPaintTile)
        {
            tileData.LeafLitterNoPaintTile = true;
            self.ApplyItemTime(self.inventory[self.selectedItem], self.tileSpeed);
            PaintEffect(false);
            return;
        }

        if (tile.HasWall && tileData.HasLeafLitterWall && !tile.TileCoatedOrPainted && !tileData.LeafLitterNoPaintWall)
        {
            tileData.LeafLitterNoPaintWall = true;
            self.ApplyItemTime(self.inventory[self.selectedItem], self.tileSpeed);
            PaintEffect(true);
            return;
        }

        orig(self, x, y);

        return;

        void PaintEffect(bool wall)
        {
            var oldCoating = WorldGen.coatingColors(tile, !wall);

            if (wall ? tile.WallPainted : tile.TilePainted)
            {
                WorldGen.paintEffect(x, y, 0, wall ? tile.WallColor : tile.TileColor);
            }

            if (wall ? tile.WallCoated : tile.TileCoated)
            {
                WorldGen.paintCoatEffect(x, y, 0, oldCoating);
            }
        }
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

        var position = new Point(i, j).ToWorldCoordinates();

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

                if (!tile.HasTile || !(IsVisible(tile) || tileData.LeafLitterNoPaintTile) || !tileData.HasLeafLitterTile)
                {
                    continue;
                }

                DrawLeafOverlay(i, j, false);
            }
        }
        sb.End();

        return;

        static bool IsVisible(Tile tile)
        {
            return !tile.IsTileInvisible || TileDrawing.Instance._shouldShowInvisibleBlocks;
        }
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

                if (!tile.HasWall || (!IsVisible(tile) && !tileData.LeafLitterNoPaintWall) || !tileData.HasLeafLitterWall)
                {
                    continue;
                }

                DrawLeafOverlay(i, j, true);
            }
        }
        sb.Restart(in ss);

        return;

        static bool IsVisible(Tile tile)
        {
            return !tile.IsWallInvisible || TileDrawing.Instance._shouldShowInvisibleBlocks;
        }
    }

    [GlobalTileHooks.PostDraw]
    private static void Tile_PostDraw_AddPoint(int i, int j, int type, SpriteBatch spriteBatch)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        var tileData = tile.Get<SolsticeTileData>();

        if (tileData.HasLeafLitterTile)
        {
            leafPositions.Add(new Point(i, j));
        }
    }

    private static void DrawWalls_AddPoint(ILContext il)
    {
        var c = new ILCursor(il);

        int iIndex = -1;
        int jIndex = -1;

        ILLabel? loopEndTarget = null;

        c.FindNext(
            out _,
            i => i.MatchCall<WallDrawing>(nameof(WallDrawing.FullTile)),
            i => i.MatchBrtrue(out loopEndTarget)
        );

        Debug.Assert(loopEndTarget is not null);

        c.GotoLabel(loopEndTarget);

        c.MoveAfterLabels();

        c.FindPrev(
            out _,
            i => i.MatchLdloc(out iIndex),
            i => i.MatchLdloc(out jIndex),
            i => i.MatchLdloc(out _),
            i => i.MatchLdloc(out _),
            i => i.MatchCall(typeof(WallLoader), nameof(WallLoader.PostDraw)));

        c.EmitLdloc(iIndex);
        c.EmitLdloc(jIndex);
        c.EmitDelegate(
            static (int i, int j) =>
            {
                Tile tile = Framing.GetTileSafely(i, j);

                var tileData = tile.Get<SolsticeTileData>();

                if (tileData.HasLeafLitterWall)
                {
                    leafPositions.Add(new Point(i, j));
                }
            }
        );
    }

#region Drawing
    private static void DrawLeafOverlay(int i, int j, bool wall)
    {
        const int frame_count = 4;
        const int frame_width = 24;

        const float sway_freq = 1f / 20f;

        var asset = Assets.Images.Aerie.Placements.AerieLeafLitterOverlay.Asset;

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

        bool disallowPaint =
            wall
          ? tileData.LeafLitterNoPaintWall
           : tileData.LeafLitterNoPaintTile;

        if (disallowPaint)
        {
            paint = 0;
        }

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        Color color = Tile.GetDrawColor(i, j, wall, useColor, !disallowPaint);

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

public class AerieLeafLitter : ModItem, ISmartCursorBehavior
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieLeafLitter.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;

        SolsticeItemSets.UseCursorPlacementIcon[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 14;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 8;
        Item.useTime = 8;
        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.consumable = true;

        Item.ApplyItemAnimationCompensationsToVanillaItems();
    }

    public override bool? UseItem(Player player)
    {
        Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];

        if (!tile.HasTile && !tile.HasWall)
        {
            return false;
        }
        
        ref var tileData = ref tile.Get<SolsticeTileData>();

        if (tile.HasTile && Main.tileSolid[tile.TileType] && !tileData.HasLeafLitterTile)
        {
            tileData.HasLeafLitterTile = true;
            tileData.LeafLitterNoPaintTile = false;

            SoundEngine.PlaySound(in SoundID.Dig);

            return true;
        }

        if (tile.HasWall && !tileData.HasLeafLitterWall)
        {
            tileData.HasLeafLitterWall = true;
            tileData.LeafLitterNoPaintWall = false;

            SoundEngine.PlaySound(in SoundID.Dig);

            return true;
        }

        return false;
    }

    Point ISmartCursorBehavior.FindSmartCursorTarget(SmartCursorHelper.SmartCursorUsageInfo info)
    {
        var targets = new List<Point>();

        for (int i = info.reachableStartX; i <= info.reachableEndX; i++)
        {
            for (int j = info.reachableStartY; j <= info.reachableEndY; j++)
            {
                Tile tile = Main.tile[i, j];

                var tileData = tile.Get<SolsticeTileData>();

                if ((tile.HasTile && Main.tileSolid[tile.TileType] && !tileData.HasLeafLitterTile)
                 || (tile.HasWall && !tileData.HasLeafLitterWall))
                {
                    targets.Add(new Point(i, j));
                }
            }
        }

        if (targets.Count <= 0)
        {
            return new Point(-1, -1);
        }

        float distance = -1f;
        Point first = targets[0];

        foreach (Point target in targets)
        {
            var position = target.ToWorldCoordinates();

            float newDistance = Vector2.Distance(position, info.mouse);

            if ((int)distance != -1 && !(newDistance < distance))
            {
                continue;
            }

            distance = newDistance;
            first = target;
        }

        if (!Collision.InTileBounds(
                first.X, first.Y,
                info.reachableStartX, info.reachableStartY,
                info.reachableEndX, info.reachableEndY))
        {
            return new Point(-1, -1);
        }

        return first;
    }
}