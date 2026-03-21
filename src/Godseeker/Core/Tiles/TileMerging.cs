using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Godseeker.Core.Tiles;

// Adapted from 'https://github.com/GabeHasWon/SpiritReforged/blob/scarabdate/Common/TileCommon/TileMerging/TileMerger.cs' with consent from math2.
public static class TileMerging
{
    private const int full_frame_width = 108;

    private static readonly Dictionary<int, Asset<Texture2D>> textureOverlayByType = [];

    private static HashSet<int>?[] mergesWith { get; set; } = [];

    private static readonly Point[] offsets =
    [
        new(-1, -1), new(18, 0), new(18, 36), new(54, 0),
        new(0, 18), new(0, 0), new(0, 36), new(90, 0),
        new(36, 18), new(36, 0), new(36, 36), new(90, 18),
        new(54, 18), new(72, 0), new(72, 18), new(18, 18)
    ];

    private static Mod Mod => ModContent.GetInstance<Godseeker>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        mergesWith = CreateSet<HashSet<int>?>(nameof(mergesWith), null);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return TileID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    [OnUnload]
    private static void Unload()
    {
        textureOverlayByType.Clear();
        paintCache.Clear();
    }

    public static void AddCustomMerge(int type, Asset<Texture2D> texture, params int[] targetTypes)
    {
        textureOverlayByType[type] = texture;

        TileID.Sets.ChecksForMerge[type] = true;

        foreach (int targetType in targetTypes)
        {
            mergesWith[targetType] ??= [];
            mergesWith[targetType]?.Add(type);

            Main.tileMerge[type][targetType] = true;
            Main.tileMerge[targetType][type] = true;

            TileID.Sets.ChecksForMerge[targetType] = true;
        }
    }

#region Drawing
    [GlobalTileHooks.PostDraw]
    private static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        var merges = mergesWith[type];
        if (merges is not null && merges.Count > 0)
        {
            DrawMerge(spriteBatch, i, j, merges);
        }
    }

    private static void DrawMerge(SpriteBatch spriteBatch, int i, int j, params IEnumerable<int> types)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        Color color = Lighting.GetColor(i, j);

        int frameNumber = tile.Get<TileWallWireStateData>().TileFrameNumber;

        Vector2 zero =
            Main.drawToScreen
          ? Vector2.Zero
          : new Vector2(Main.offScreenRange);

        Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;

        foreach (int type in types)
        {
            (int mask, int paint) = GetMergeData(i, j, type);

            if (mask <= 0 ||
                !textureOverlayByType.TryGetValue(type, out var asset))
            {
                continue;
            }

            Color finalColor = color;

            Texture2D? texture = null;

            if (paint > PaintID.None)
            {
                if (!TryGetPaintTexture(type, paint, asset, out texture))
                {
                    finalColor = finalColor.MultiplyRGBA(WorldGen.paintColor(paint));
                }
            }

            texture ??= asset.Value;

            Point p = offsets[mask];

            var source = new Rectangle(p.X + (frameNumber * full_frame_width), p.Y, 16, 16);

            spriteBatch.Draw(texture, position, source, finalColor);
        }
    }

    private static (int mask, int shaderIndex) GetMergeData(int i, int j, int type)
    {
        Tile center = Framing.GetTileSafely(i, j);

        int mask = 0;
        int shaderIndex = 0;

        Check(i, j - 1, 1, true);
        Check(i, j + 1, 2, false);
        Check(i - 1, j, 4, false);
        Check(i + 1, j, 8, false);

        return (mask, shaderIndex);

        void Check(int x, int y, int bit, bool isUp)
        {
            Tile neighbor = Framing.GetTileSafely(x, y);

            if (neighbor.TileType != type)
            {
                return;
            }

            bool canMerge = isUp
                ? !center.IsHalfBlock && (center.BottomSlope || center.Slope == 0) &&
                  (neighbor.TopSlope || neighbor.Slope == 0)
                : !center.IsHalfBlock;

            if (!canMerge)
            {
                return;
            }

            mask |= bit;

            if (shaderIndex == 0)
            {
                shaderIndex = neighbor.TileColor;
            }
        }
    }
#endregion

#region Paint
    private static readonly Dictionary<TileMergingVariantKey, TileMergingRenderTargetHolder> paintCache = [];

    internal readonly record struct TileMergingVariantKey(int Type, int PaintColor);

    internal class TileMergingRenderTargetHolder(TileMergingVariantKey key, Asset<Texture2D> asset, int copySettingsFrom = -1) : TilePaintSystemV2.ARenderTargetHolder
    {
        public TileMergingVariantKey Key = key;

        public TreePaintingSettings PaintSettings = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);

        public Asset<Texture2D> Texture = asset;

        public override void Prepare()
        {
            Texture.Wait();

            PrepareTextureIfNecessary(Texture.Value);
        }

        public override void PrepareShader()
        {
            PrepareShader(Key.PaintColor, PaintSettings);
        }
    }

    private static bool TryGetPaintTexture(
        int type,
        int paintColor,
        Asset<Texture2D> asset,
        [NotNullWhen(true)] out Texture2D? texture
    )
    {
        texture = null;

        TileMergingVariantKey key = new(type, paintColor);

        if (paintCache.TryGetValue(key, out TileMergingRenderTargetHolder? holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new TileMergingRenderTargetHolder(key, asset);

        paintCache[key] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

        return false;
    }
#endregion
}
