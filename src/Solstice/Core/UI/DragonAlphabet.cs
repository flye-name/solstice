using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace Solstice.Core;

public static class DragonAlphabet
{
    public static Vector2[] GetPoints(Vector2 center, int length, out float[] rotations, float scale = 1f)
    {
        List<Vector2> result = new();
        List<float> resultRotations = new();

        var rotation = 0f;
        var point = center;
        for (int i = 0; i < length; i++)
        {
            var progress = Utils.GetLerpValue(0, length, i);
            
            point += new Vector2(30 * scale * MathHelper.Lerp(0.5f, 2f, progress) + 22 * scale, 0).RotatedBy(rotation);
            rotation = -MathHelper.TwoPi * progress * progress * .75f * scale;
            
            result.Add(point);
            resultRotations.Add(rotation);
        }

        rotations = resultRotations.ToArray();

        return result.ToArray();
    }
}