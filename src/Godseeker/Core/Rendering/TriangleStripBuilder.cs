using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Godseeker.Core;

// Watered down version of Vermin/Syrup's triangle strip implementation in Cataphract.
public enum StripCurveType : byte
{
    Linear,
    CatmullRom,
    CubicBezier,
    Hermite
}

public enum StripJoinStyle : byte
{
    Perpendicular,
    Miter
}

public enum StripWidthAttenuation : byte
{
    None,
    ContinuitySquared
}

public static class TriangleStripBuilder
{
    private const float epsilon = 1e-6f;

    public static VertexPositionColorTexture[] BuildPath(
        IReadOnlyList<Vector3> path,
        Func<float, float> widthFunction,
        Func<float, Color> colorFunction,
        Vector3? upHint = null,
        StripJoinStyle joinStyle = StripJoinStyle.Miter,
        int smoothingSubdivisions = 0,
        StripCurveType curveType = StripCurveType.CatmullRom,
        StripWidthAttenuation widthAttenuation = StripWidthAttenuation.ContinuitySquared
    )
    {
        if (path.Count < 2)
        {
            throw new ArgumentException("At least two points are required.", nameof(path));
        }

        IReadOnlyList<Vector3> workingPath = SmoothPath(path, smoothingSubdivisions, curveType);

        float[] pathProgress = ComputeProgress(workingPath);

        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[workingPath.Count * 2];

        if (vertices.Length > short.MaxValue)
        {
            throw new InvalidOperationException("Strip produced more vertices than supported by the index buffer.");
        }

        Vector3 up = upHint ?? Vector3.UnitZ;
        Vector3 upNormalized = up.LengthSquared() < epsilon ? Vector3.UnitZ : Vector3.Normalize(up);

        int segmentCount = workingPath.Count - 1;

        Vector3[] segmentDirs = new Vector3[segmentCount];
        Vector3[] segmentRights = new Vector3[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 dir = workingPath[i + 1] - workingPath[i];

            if (dir.LengthSquared() < epsilon)
            {
                dir = i > 0 ? segmentDirs[i - 1] : Vector3.UnitY;
            }

            dir.Normalize();

            Vector3 right = Vector3.Cross(upNormalized, dir);

            if (right.LengthSquared() < epsilon)
            {
                right = FindPerpendicular(dir);
            }
            else
            {
                right.Normalize();
            }

            segmentDirs[i] = dir;
            segmentRights[i] = right;
        }

        Vector3 lastTangent = segmentDirs[0];

        for (int i = 0; i < workingPath.Count; i++)
        {
            Vector3 position = workingPath[i];
            float t = pathProgress[i];

            float baseWidth = Math.Max(0f, widthFunction(t));

            Vector3 prevDir = segmentDirs[Math.Max(i - 1, 0)];
            Vector3 nextDir = segmentDirs[Math.Min(i, segmentCount - 1)];

            Vector3 tangent;

            if (i == 0)
            {
                tangent = nextDir;
            }
            else if (i == segmentCount)
            {
                tangent = prevDir;
            }
            else
            {
                tangent = prevDir + nextDir;
                if (tangent.LengthSquared() < epsilon)
                {
                    tangent = nextDir;
                }
                else
                {
                    tangent.Normalize();
                }
            }

            float width = baseWidth;

            if (i > 0 && widthAttenuation == StripWidthAttenuation.ContinuitySquared)
            {
                float continuity = MathHelper.Clamp((Vector3.Dot(lastTangent, tangent) + 1f) * 0.5f, 0f, 1f);
                width *= continuity * continuity;
            }

            float halfWidth = width * 0.5f;

            Vector3 prevRight = segmentRights[Math.Max(i - 1, 0)];
            Vector3 nextRight = segmentRights[Math.Min(i, segmentCount - 1)];

            Vector3 rightOffset;
            Vector3 leftOffset;

            if (joinStyle == StripJoinStyle.Miter)
            {
                rightOffset = ComputeMiterOffset(prevRight, nextRight, i == 0, i == segmentCount, halfWidth);
                leftOffset = ComputeMiterOffset(-prevRight, -nextRight, i == 0, i == segmentCount, halfWidth);
            }
            else
            {
                Vector3 joinNormal;

                if (i == 0)
                {
                    joinNormal = nextRight;
                }
                else if (i == segmentCount)
                {
                    joinNormal = prevRight;
                }
                else
                {
                    joinNormal = prevRight + nextRight;

                    if (joinNormal.LengthSquared() < epsilon)
                    {
                        joinNormal = nextRight;
                    }
                }

                if (joinNormal.LengthSquared() < epsilon)
                {
                    joinNormal = Vector3.UnitY;
                }

                joinNormal.Normalize();

                rightOffset = joinNormal * halfWidth;
                leftOffset = -joinNormal * halfWidth;
            }

            Vector3 leftPos = position + leftOffset;
            Vector3 rightPos = position + rightOffset;

            float uCoord = pathProgress[i];
            Color color = colorFunction(t);

            vertices[i * 2 + 1] = new(leftPos, color, new(uCoord, 1));
            vertices[i * 2] = new(rightPos, color, new(uCoord, 0));

            lastTangent = tangent;
        }

        return vertices;
    }

#region Path Evaluation
    private static IReadOnlyList<Vector3> RemoveDegenerates(IReadOnlyList<Vector3> path)
    {
        if (path.Count < 2)
        {
            return path;
        }

        List<Vector3> result = new(path.Count);

        Vector3 last = path[0];

        result.Add(last);

        for (int i = 1; i < path.Count; i++)
        {
            if (Vector3.DistanceSquared(last, path[i]) <= epsilon)
            {
                continue;
            }

            last = path[i];
            result.Add(last);
        }

        if (result.Count == 1)
        {
            result.Add(path[^1]);
        }

        return result;
    }

    private static float[] ComputeProgress(IReadOnlyList<Vector3> path)
    {
        float[] progress = new float[path.Count];
        float cumulative = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            cumulative += Vector3.Distance(path[i - 1], path[i]);
            progress[i] = cumulative;
        }

        if (cumulative > epsilon)
        {
            float inv = 1f / cumulative;

            for (int i = 1; i < progress.Length; i++)
                progress[i] *= inv;
        }

        return progress;
    }
#endregion

#region Curve Evaluation
    private static IReadOnlyList<Vector3> SmoothPath(IReadOnlyList<Vector3> path, int subdivisions, StripCurveType curveType)
    {
        if (subdivisions <= 0 || path.Count < 2)
        {
            return path;
        }

        List<Vector3> result = new((path.Count - 1) * (subdivisions + 1) + 1)
        {
            path[0]
        };

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 p0 = path[Math.Max(i - 1, 0)];
            Vector3 p1 = path[i];
            Vector3 p2 = path[i + 1];
            Vector3 p3 = path[Math.Min(i + 2, path.Count - 1)];

            for (int s = 1; s <= subdivisions; s++)
            {
                float t = s / (float)(subdivisions + 1);

                Vector3 point = EvaluateCurve(curveType, p0, p1, p2, p3, t, path.Count);

                result.Add(point);
            }

            result.Add(p2);
        }

        return RemoveDegenerates(result);
    }

    private static Vector3 EvaluateCurve(StripCurveType curveType, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, int count)
    {
        return curveType switch
        {
            StripCurveType.CatmullRom when count >= 4 => Vector3.CatmullRom(p0, p1, p2, p3, t),

            StripCurveType.CubicBezier when count >= 4 => CubicBezier(p0, p1, p2, p3, t),

            StripCurveType.Hermite when count >= 4 => Hermite(p0, p1, p2, p3, t),

            _ => Vector3.Lerp(p1, p2, t)
        };
    }

    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 c1 = p1 + (p2 - p0) / 6f;
        Vector3 c2 = p2 - (p3 - p1) / 6f;

        float inv = 1f - t;

        return (inv * inv * inv * p1)
             + (3f * inv * inv * t * c1)
             + (3f * inv * t * t * c2)
             + (t * t * t * p2);
    }

    private static Vector3 Hermite(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 tan1 = (p2 - p0) * 0.5f;
        Vector3 tan2 = (p3 - p1) * 0.5f;

        return Vector3.Hermite(p1, tan1, p2, tan2, t);
    }
#endregion

    private static Vector3 FindPerpendicular(Vector3 vector)
    {
        Vector3 axis = Math.Abs(vector.Y) < Math.Abs(vector.X)
            ? Vector3.UnitY
            : Vector3.UnitX;

        Vector3 perpendicular = Vector3.Cross(vector, axis);

        if (perpendicular.LengthSquared() < epsilon)
        {
            perpendicular = Vector3.Cross(vector, Vector3.UnitZ);
        }

        perpendicular.Normalize();

        return perpendicular;
    }

    private static Vector3 ComputeMiterOffset(Vector3 prevNormal, Vector3 nextNormal, bool isStart, bool isEnd, float halfWidth)
    {
        if (halfWidth <= epsilon)
        {
            return Vector3.Zero;
        }

        if (isStart)
        {
            return nextNormal * halfWidth;
        }
        if (isEnd)
        {
            return prevNormal * halfWidth;
        }

        float prevLenSq = prevNormal.LengthSquared();
        float nextLenSq = nextNormal.LengthSquared();

        if (prevLenSq < epsilon || nextLenSq < epsilon)
        {
            return (nextLenSq >= prevLenSq ? nextNormal : prevNormal) * halfWidth;
        }

        Vector3 sum = prevNormal + nextNormal;

        float sumLenSq = sum.LengthSquared();

        if (sumLenSq < 1e-4f)
        {
            return nextNormal * halfWidth;
        }

        Vector3 miter = sum / MathF.Sqrt(sumLenSq);

        float denom = Vector3.Dot(miter, nextNormal);
        float absDenom = MathF.Abs(denom);

        if (absDenom <= 1e-3f)
        {
            return nextNormal * halfWidth;
        }

        float scale = halfWidth / denom;

        const float miter_limit = 4f;

        float maxScale = halfWidth * miter_limit;

        if (MathF.Abs(scale) > maxScale)
        {
            scale = MathF.Sign(scale) * maxScale;
        }

        return miter * scale;
    }
}
