using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Godseeker.Core;
using Godseeker.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using MiscTextures = Godseeker.GeneratedAssets.Assets.Images.Textures;

namespace Godseeker.Content.Aerie.Environment;

// TODO: HighFPSSupport Compatibility
public record struct WindParticle : IParticle
{
    private const int max_old_positions = 50;

    private const float width = 5f;

    private const float loop_offset = 0.3f;

    private const float velocity_magnitude = 19f;

    private const float parallax_min = -0.5f;
    private const float parallax_max = 0.15f;

    public Vector2 Position { get; set; }

    public Vector2[] OldPositions { get; init; }

    public Vector2 Velocity { get; set; }

    public float Wind { get; init; }

    public float Parallax { get; init; }

    public float LoopOffset { get; init; }

    public bool ShouldLoop { get; init; }

    public float Lifetime { get; set; }

    public bool IsActive { get; set; }

    private Vector2 parallaxOffset;

    public WindParticle(Vector2 position, float wind, bool shouldLoop, bool foreground)
    {
        Position = position;
        OldPositions = new Vector2[max_old_positions];
        Velocity = Vector2.Zero;
        Wind = wind;
        Parallax = Main.rand.NextFloat(foreground ? 0 : parallax_min, foreground ? parallax_max : 0);
        LoopOffset = Main.rand.NextFloat(-loop_offset, loop_offset);
        ShouldLoop = shouldLoop;
        Lifetime = 0f;
        IsActive = true;
    }

    void IParticle.Update()
    {
        const float lifetime_increment = 0.0063f;

        float increment = lifetime_increment * MathF.Abs(Wind);

        Lifetime += increment;

        if (Lifetime > 1f)
        {
            IsActive = false;
        }

        parallaxOffset += (Main.screenPosition - Main.screenLastPosition) * -Parallax;

        const float wave_frequency = 0.6f;
        const float wave_amplitude = 0.1f;

        float wave = MathF.Sin((Lifetime + ((float)Main.timeForVisualEffects / 60f)) * wave_frequency) * wave_amplitude;

        var newVelocity = new Vector2(Wind, wave) * Utils.Remap(Parallax, parallax_min, parallax_max, 0.6f, 1.2f);

        // Loop behavior, similar to vanilla paper airplanes
        if (ShouldLoop)
        {
            const float loop_range = 0.06f;

            float range = loop_range / MathHelper.Clamp(MathF.Abs(Wind), 0.01f, 1);
            range *= 0.5f;

            float offset = 0.5f + LoopOffset;

            float interpolator = Utils.Remap(Lifetime, offset - range, offset + range, 0f, 1f);

            newVelocity = newVelocity.RotatedBy(MathHelper.TwoPi * interpolator * -MathF.Sign(Wind));
        }

        Velocity = newVelocity.SafeNormalize(Vector2.UnitY) * velocity_magnitude * MathF.Abs(Wind);

        Position += Velocity;

        for (int i = OldPositions.Length - 2; i >= 0; i--)
        {
            OldPositions[i + 1] = OldPositions[i];
        }

        OldPositions[0] = Position;
    }

    readonly void IParticle.Draw(SpriteBatch spriteBatch, GraphicsDevice device)
    {
        var offset = parallaxOffset;

        Vector3[] positions =
            OldPositions.Where(pos => pos != default)
            .Select(p => new Vector3(p + offset - Main.screenPosition, 0))
            .ToArray();

        if (positions.Length <= 2)
        {
            return;
        }

        const float parallax_scale_min = 0.3f;
        const float parallax_scale_max = 1.65f;

        const float alpha = 0.14f;

        float brightness =
            MathF.Sin(Lifetime * MathHelper.Pi)
            * Main.atmo
            * MathF.Abs(Wind)
            * Utils.Remap(Parallax, parallax_min, parallax_max, parallax_scale_min, parallax_scale_max);

        Color color = Main.ColorOfTheSkies * brightness * alpha;
        color.A = 0;

        VertexPositionColorTexture[] vertices =
            TriangleStripBuilder.BuildPath(positions,
            t => MathF.Sin(t * MathHelper.Pi) * brightness * width,
            _ => color,
            smoothingSubdivisions: 2);

        if (vertices.Length > 3)
        {
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
    }
}

[Autoload(Side = ModSide.Client)]
public static class Wind
{
    private const int loop_chance = 10;

    private const int offscreen_margin = 100;

    public const int WIND_COUNT = 60;
    public static readonly ParticleHandler<WindParticle> BackgroundWind = new(WIND_COUNT);
    public static readonly ParticleHandler<WindParticle> ForegroundWind = new(WIND_COUNT);

    [OnLoad]
    private static void Load()
    {
        On_Main.DrawBackgroundBlackFill += DrawBackgroundBlackFill_BackgroundWind;

        On_Main.DrawInfernoRings += DrawInfernoRings_ForegroundWind;

        On_Dust.UpdateDust += UpdateDust_Wind;
    }

    private static void DrawBackgroundBlackFill_BackgroundWind(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
    {
        orig(self);

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        SpriteBatch spriteBatch = Main.spriteBatch;

        GraphicsDevice device = Main.graphics.GraphicsDevice;

        device.Textures[0] = MiscTextures.Bloom;

        BackgroundWind.Draw(spriteBatch, device);

        var snapshot = new SpriteBatchSnapshot(Main.spriteBatch);
        Main.spriteBatch.Restart(in snapshot);
    }

    private static void DrawInfernoRings_ForegroundWind(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        SpriteBatch spriteBatch = Main.spriteBatch;

        GraphicsDevice device = Main.graphics.GraphicsDevice;

        device.Textures[0] = MiscTextures.Bloom;

        ForegroundWind.Draw(spriteBatch, device);

        var snapshot = new SpriteBatchSnapshot(Main.spriteBatch);
        Main.spriteBatch.Restart(in snapshot);
    }

    private static void UpdateDust_Wind(On_Dust.orig_UpdateDust orig)
    {
        orig();

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        BackgroundWind.Update();
        ForegroundWind.Update();

        SpawnWind();
    }

    private static void SpawnWind()
    {
        const float spawn_chance = 15f;

        float spawnChance = spawn_chance / MathF.Abs(Main.WindForVisuals);

        if (!Main.rand.NextBool((int)spawnChance))
        {
            return;
        }

        Vector2 screensize = new(Main.screenWidth, Main.screenHeight);

        float offset = -Main.WindForVisuals + Math.Clamp(Main.LocalPlayer.velocity.X / 50, -3f, 3f);

        offset = Utils.Remap(offset, -1, 1, 0, 1, false);

        Vector2 screenCenter = Main.screenPosition +
            new Vector2(
                screensize.X * offset,
                screensize.Y * 0.5f
            );

        Rectangle spawn = Utils.CenteredRectangle(screenCenter, new(screensize.X * 0.8f, screensize.Y + 600));

        spawn.Inflate(offscreen_margin, offscreen_margin);

        Vector2 position = Main.rand.NextVector2FromRectangle(spawn);

        if (Main.rand.NextBool())
        {
            ForegroundWind.Spawn(new(position, Main.WindForVisuals, Main.rand.NextBool(loop_chance), true));
        }
        else
        {
            BackgroundWind.Spawn(new(position, Main.WindForVisuals, Main.rand.NextBool(loop_chance), false));
        }
    }
}

