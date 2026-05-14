using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Solstice.Core;

public static class TileUtils
{
    public static Color GetDrawColor(int i, int j, bool wall = false, bool paint = false)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        Color color = wall ? WallColor() : TileColor();

        if (paint)
        {
            color.MultiplyRGBA(WorldGen.paintColor(wall ? tile.WallColor : tile.TileColor));
        }

        return color;

        Color TileColor()
        {
            return TileDrawing.GetFinalLight(tile, tile.TileType, Lighting.GetColor(i, j), Color.White);
        }

        Color WallColor()
        {
            Color wallColor = Lighting.GetColor(j, i);

            if (tile.IsWallFullbright || tile.WallType == WallID.EchoWall)
            {
                wallColor = Color.White;
            }

            return wallColor;
        }
    }
}

public static class TileExtensions
{
    extension(Tile tile)
    {
        public bool HasWall => tile.WallType != WallID.None;
    }

    extension(SlopeType slope)
    {
        public bool Block => slope == SlopeType.Solid;

        public bool Up => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight;

        public bool Down => slope.Block || slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight;

        public bool Left => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeDownLeft;

        public bool Right => slope.Block || slope is SlopeType.SlopeUpRight or SlopeType.SlopeDownRight;

        public bool UpLeft => slope.Up || slope.Left;

        public bool DownLeft => slope.Down || slope.Left;

        public bool UpRight => slope.Up || slope.Right;

        public bool DownRight => slope.Down || slope.Right;
    }
}
