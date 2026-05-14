using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Solstice.Core;

public static class TileUtils
{
    public static Color GetDrawColor(int i, int j, bool wall = false, bool paint = false, bool allowFullBright = true)
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
            Color tileColor = Lighting.GetColor(i, j);

            if (tile.IsTileFullbright && (paint || allowFullBright))
            {
                tileColor = Color.White;
            }

            if (tile.IsActuated)
            {
                tileColor = tile.actColor(tileColor);
            }
            else if (TileDrawing.ShouldTileShine(tile.TileType, tile.frameX))
            {
                tileColor = Main.shine(tileColor, tile.TileType);
            }

            return tileColor;
        }

        Color WallColor()
        {
            Color wallColor = Lighting.GetColor(i, j);

            if ((tile.IsWallFullbright && (paint || allowFullBright)) || tile.WallType == WallID.EchoWall)
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

        public bool TileCoatedOrPainted => tile.TilePainted || tile.TileCoated;

        public bool WallCoatedOrPainted => tile.WallPainted || tile.WallCoated;

        public bool TilePainted => tile.TileColor > PaintID.None;

        public bool WallPainted => tile.WallColor > PaintID.None;

        public bool TileCoated => tile.IsTileInvisible || tile.IsTileFullbright;

        public bool WallCoated => tile.IsWallInvisible || tile.IsWallFullbright;
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
