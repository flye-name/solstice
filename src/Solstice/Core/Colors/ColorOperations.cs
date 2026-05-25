using Microsoft.Xna.Framework;
using System;

namespace Solstice.Core;

public static class ColorOperations
{
    extension(Color)
    {
        public static Color Pow(Color color, float amount)
        {
            color.R = PowComponent(color.R);
            color.G = PowComponent(color.G);
            color.B = PowComponent(color.B);
            color.A = PowComponent(color.A);

            return color;

            byte PowComponent(byte component)
            {
                return (byte)(Math.Pow((float)component / byte.MaxValue, amount) * byte.MaxValue);
            }
        }
    }
}
