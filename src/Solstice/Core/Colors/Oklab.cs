using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solstice.Core;

public static class Oklab
{
    public static Matrix ConeToLms { get; } = new(
        0.4121656120f, 0.2118591070f, 0.0883097947f, 0f,
        0.5362752080f, 0.6807189584f, 0.2818474174f, 0f,
        0.0514575653f, 0.1074065790f, 0.6302613616f, 0f,
        0f, 0f, 0f, 0f
    );

    public static Matrix LmsToCone { get; } = new(
        4.0767245293f, -1.2681437731f, -0.0041119885f, 0f,
        -3.3072168827f, 2.6093323231f, -0.7034763098f, 0f,
        0.2307590544f, -0.3411344290f, 1.7068625689f, 0f,
        0f, 0f, 0f, 0f
    );

    extension(Color)
    {
        public static Vector3 ToOklab(Color color)
        {
            var oklab = Vector3.Transform(color.ToVector3(), ConeToLms);
            {
                oklab.X = MathF.Pow(oklab.X, 0.333f);
                oklab.Y = MathF.Pow(oklab.Y, 0.333f);
                oklab.Z = MathF.Pow(oklab.Z, 0.333f);
            }
            return oklab;
        }

        public static Color FromOklab(Vector3 oklab)
        {
            oklab.X = MathF.Pow(oklab.X, 3f);
            oklab.Y = MathF.Pow(oklab.Y, 3f);
            oklab.Z = MathF.Pow(oklab.Z, 3f);

            return new Color(Vector3.Transform(oklab, LmsToCone));
        }

        public static Color OklabLerp(Color colorA, Color colorB, float amount)
        {
            return Lerp(colorA, colorB, amount);
        }
    }

    public static Color Lerp(Color colorA, Color colorB, float amount)
    {
        var oklabA = Color.ToOklab(colorA);
        var oklabB = Color.ToOklab(colorB);

        var mix = Vector3.Lerp(oklabA, oklabB, amount);

        var color = Color.FromOklab(mix);
        {
            color.A = (byte)(colorA.A + (colorB.A - colorA.A) * amount * byte.MaxValue);
        }
        return color;
    }
}
