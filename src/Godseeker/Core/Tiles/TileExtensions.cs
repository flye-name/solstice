using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace Godseeker.Core;

public static class TileExtensions
{
    extension(SlopeType slope)
    {
        public bool Block => slope == SlopeType.Solid;

        public bool Up => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight;

        public bool Down => slope.Block || slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight;

        public bool Left => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeDownLeft;

        public bool Right => slope.Block || slope is SlopeType.SlopeUpRight or SlopeType.SlopeDownRight;
    }
}
