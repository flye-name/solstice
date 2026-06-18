using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public static Color[] ArrayLerp(IEnumerable<Color> colors1, IEnumerable<Color> colors2, float t)
        {
            var innerColors1 = colors1.ToArray();
            var innerColors2 = colors2.ToArray();
            
            var length = Math.Min(innerColors1.Length, innerColors2.Length);

            var result = new Color[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = Color.Lerp(innerColors1[i], innerColors2[i], t);
            }

            return result;
        }
    }
}
