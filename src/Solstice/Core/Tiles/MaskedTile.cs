using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.ObjectData;

// Modified ver of https://github.com/gold-meridian/daybreak-mod/pull/18 - Lucille Karma
// TODO: Use Daybreak's impl post restructuring?
//       More general system that supports merge frames and could integrate with AerieLeafLiter's system
//       Rewrite docs
namespace Solstice.Core;

/// <summary>
///     Represents an instance of a tile whose
///     contents are rendered as a shader post-processing
///     step at the end of standard tile renders.
/// </summary>
public abstract class MaskedTile : ModTile
{
    /// <summary>
    ///     The set of points on the tile grid
    ///     that should be rendered.
    /// </summary>
    protected readonly HashSet<Point> MaskPositions = [];

    /// <summary>
    ///     Whether any of this kind of tile are being
    ///     rendered this frame or not.
    /// </summary>
    public bool Active => MaskPositions.Count >= 1;

    /// <summary>
    ///     The mask render target that contains
    ///     the contents of all tiles of this kind
    ///     on-screen.
    /// </summary>
    public RenderTargetLease? Mask
    {
        get;
        private set;
    }

    private void RenderMaskTargetContents()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        foreach ((int i, int j) in MaskPositions)
        {
            RenderIntoMask(i, j);
        }

        Main.spriteBatch.End();
    }

    private void RenderMaskTargetResults()
    {
        if (Mask is null)
        {
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        float yOffset = TileObjectData.GetTileData(Type, 0)?.DrawYOffset ?? 0f;

        var texture = Mask.Target;
        Vector2 position = Vector2.UnitY * yOffset;

        ApplyShader();

        Main.spriteBatch.Draw(texture, position, Color.White);

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Applies optional tile-independent effects before
    ///     tiles get rendered into the mask target.
    /// </summary>
    protected virtual void PreMaskTargetRender()
    { }

    /// <summary>
    ///     Renders a masked tile instance
    ///     into the mask target.
    /// </summary>
    /// <remarks>
    ///     This is distinct from the typical Draw
    ///     hooks, and exists solely to dictate what
    ///     is presented in <see cref=""/>
    ///     
    ///     <br></br>
    /// 
    ///     You may still use PreDraw and PostDraw
    ///     to perform standard tile rendering tasks.
    /// </remarks>
    protected abstract void RenderIntoMask(int i, int j);

    /// <summary>
    ///     Applies the post-processing shader to
    ///     the mask target for use at the time of rendering.
    /// </summary>
    protected virtual void ApplyShader()
    { }

    /// <inheritdoc />
    public override void DrawEffects(int x, int y, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        TileDrawing.Instance.AddSpecialPoint(x, y, TileDrawing.TileCounterType.CustomSolid);
    }

    /// <inheritdoc />
    public override void SpecialDraw(int x, int y, SpriteBatch spriteBatch)
    {
        MaskPositions.Add(new Point(x, y));
    }

    public void RenderMaskedTiles()
    {
        if (!Active)
        {
            return;
        }

        using (Main.spriteBatch.Scope())
        {
            Mask ??= ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);

            PreMaskTargetRender();
            using (Mask.Scope(clearColor: Color.Transparent))
            {
                RenderMaskTargetContents();
            }

            RenderMaskTargetResults();
            MaskPositions.Clear();
        }
    }
}