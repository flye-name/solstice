using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Utilities;
using BackgroundTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;

namespace Godseeker.Content.Aerie.Environment;

public static class Stars
{
    private readonly record struct Star(Vector3 Position, float Scale, float Phase);

    private const int star_count = 1200;

    private static readonly Star[] stars = new Star[star_count];

    [OnLoad]
    private static void Load()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            var position = RandomPoint(Main.rand);

            float scale = Main.rand.NextFloat(0.1f, 1f);

            float phase = Main.rand.NextFloat() * MathF.Tau;

            stars[i] = new Star(position, scale, phase);
        }
    }

    private static Vector3 RandomPoint(UnifiedRandom rand)
    {
        float u = rand.NextFloat();
        float v = rand.NextFloat();

        float theta = 2 * MathF.PI * u;
        float phi = MathF.Acos(2 * v - 1);

        float x = MathF.Sin(phi) * MathF.Cos(theta);
        float y = MathF.Sin(phi) * MathF.Sin(theta);
        float z = MathF.Cos(phi);

        return new Vector3(x, y, z);
    }

    public static void DrawStars(SpriteBatch spriteBatch)
    {
        const float offscreen_margin = 50f;

        const float star_max_scale = 0.37f;
        const float star_min_scale = 0.11f;
        const float star_alpha = 0.14f;

        const float rotation_x = 0.4f;
        const float rotation_speed = 0.0014f;

        var center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

        float screenScale = center.Length();
        screenScale += offscreen_margin * screenScale / 1101f;

        var transform =
            Matrix.CreateRotationY(Main.GlobalTimeWrappedHourly * rotation_speed)
            * Matrix.CreateRotationX(rotation_x)
            * Matrix.CreateScale(screenScale)
            * Matrix.CreateTranslation(new(center, 0));

        var texture = BackgroundTextures.Star.Value;

        var origin = texture.Size() * 0.5f;

        foreach (var star in stars)
        {
            var position = Vector3.Transform(star.Position, transform);

            if (position.Z < 0)
            {
                continue;
            }

            float twinkle = (MathF.Sin(star.Phase + (Main.GlobalTimeWrappedHourly * 2.3f)) + 1) * 0.5f;

            float fade = 1f - MathF.Pow(position.Y / Main.screenHeight, 2f);

            float scale = MathF.Max(star.Scale * star_max_scale, star_min_scale) * fade * twinkle;

            var color = Color.White * star_alpha;
            color.A = 0;

            spriteBatch.Draw(
                new DrawParameters(texture)
                {
                    Position = new(position.X, position.Y),
                    Scale = new(scale),
                    Color = color,
                    Origin = origin,
                }
            );
        }
    }
}
