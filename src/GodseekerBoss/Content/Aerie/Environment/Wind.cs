using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using GodseekerBoss.Core;
using GodseekerBoss.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using MiscTextures = GodseekerBoss.GeneratedAssets.Assets.Images.Textures;

namespace GodseekerBoss.Content.Aerie.Environment;

// TODO: HighFPSSupport Compatibility
public record struct WindParticle : IParticle
{
    private const int max_old_positions = 60;

    private const float width = 5f;

    private const float lifetime_increment = 0.0063f;

    private const float wave_frequency = 0.6f;
    private const float wave_amplitude = 0.1f;

    private const float loop_range = 0.06f;

    private const float loop_max_offset = 0.3f;

    private const float velocity_magnitude = 19f;

    public Vector2 Position { get; set; }

    public Vector2[] OldPositions { get; init; }

    public Vector2 Velocity { get; set; }

    public float Wind { get; init; }

    public float LoopOffset { get; init; }

    public bool ShouldLoop { get; init; }

    public float Lifetime { get; set; }

    public bool IsActive { get; set; }

    public WindParticle(Vector2 position, float wind, bool shouldLoop)
    {
        Position = position;
        OldPositions = new Vector2[max_old_positions];
        Velocity = Vector2.Zero;
        Wind = wind;
        LoopOffset = Main.rand.NextFloat(-loop_max_offset, loop_max_offset);
        ShouldLoop = shouldLoop;
        Lifetime = 0f;
        IsActive = true;
    }

    void IParticle.Update()
    {
        float increment = lifetime_increment * MathF.Abs(Wind);

        Lifetime += increment;

        if (Lifetime > 1f)
        {
            IsActive = false;
        }

        float wave = MathF.Sin((Lifetime + ((float)Main.timeForVisualEffects / 60f)) * wave_frequency) * wave_amplitude;

        Vector2 newVelocity = new(Wind, wave);

        // Loop behavior, similar to vanilla paper airplanes
        if (ShouldLoop)
        {
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
        Vector3[] positions =
            OldPositions.Where(pos => pos != default)
            .Select(p => new Vector3(p - Main.screenPosition, 0))
            .ToArray();

        if (positions.Length <= 2)
        {
            return;
        }

        float brightness = MathF.Sin(Lifetime * MathHelper.Pi) * Main.atmo * MathF.Abs(Wind);

        float alpha = 0.2f; // TODO: Config

        // Color based on the tile at the center of the trail
        Vector3 center = positions[positions.Length / 2];

        Point tilePosition = (new Vector2(center.X, center.Y) - Main.screenPosition).ToTileCoordinates();

        Color color = Main.ColorOfTheSkies * brightness * alpha;
        color.A = 0;

        // TODO: Use upcoming DAYBREAK rendering
        VertexPositionColorTexture[] vertices =
            TriangleStripBuilder.BuildPath(positions,
            t => MathF.Sin(t * MathHelper.Pi) * brightness * width,
            t => color,
            smoothingSubdivisions: 1);

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

    public const int WIND_COUNT = 120;
    public static readonly ParticleHandler<WindParticle> Winds = new(WIND_COUNT);

    [OnLoad]
    private static void Load()
    {
        On_Main.DrawInfernoRings += InGameDraw;

        On_Dust.UpdateDust += UpdateDust_Wind;
    }

    private static void InGameDraw(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        Draw();
    }

    private static void Draw()
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        GraphicsDevice device = Main.graphics.GraphicsDevice;

        var snapshot = new SpriteBatchSnapshot(spriteBatch);
        using (spriteBatch.Scope())
        {
            spriteBatch.Begin(snapshot);
            {
                device.Textures[0] = MiscTextures.Bloom;

                Winds.Draw(spriteBatch, device);
            }
            spriteBatch.End();
        }
    }

    private static void UpdateDust_Wind(On_Dust.orig_UpdateDust orig)
    {
        orig();

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        Winds.Update();

        SpawnWind();
    }

    private static void SpawnWind()
    {
        const float spawn_chance = 22f;

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

        Winds.Spawn(new(position, Main.WindForVisuals, Main.rand.NextBool(loop_chance)));
    }
}

