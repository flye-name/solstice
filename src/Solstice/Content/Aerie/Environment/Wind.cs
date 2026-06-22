using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BitOperations = System.Numerics.BitOperations;

namespace Solstice.Content.Aerie;

// TODO: HighFPSSupport Compatibility

[Autoload(Side = ModSide.Client)]
public static class Wind
{
    private const int max_old_positions = 50;

    private const float width = 5f;

    private const float loop_offset = 0.3f;

    private const float velocity_magnitude = 19f;

    private const float parallax_min = -0.5f;
    private const float parallax_max = 0.15f;

    private record struct WindParticle(Vector2 Position, Vector2[] OldPositions, Vector2 Velocity, float Wind, float Parallax, Vector2 ParallaxOffset, float LoopOffset, bool ShouldLoop, float Lifetime)
    {
        public bool Update()
        {
            const float lifetime_increment = 0.0063f;

            float increment = lifetime_increment * MathF.Abs(Wind);

            Lifetime += increment;

            if (Lifetime > 1f)
            {
                return false;
            }

            ParallaxOffset += (Main.screenPosition - Main.screenLastPosition) * -Parallax;

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

            return true;
        }

        public readonly void GenerateVertices(List<VertexPositionColorTexture> currentVertices, List<int> currentIndices)
        {
            const float parallax_scale_min = 0.3f;
            const float parallax_scale_max = 1.65f;

            const float alpha = 0.14f;

            var offset = ParallaxOffset;

            Vector3[] positions =
                OldPositions.Where(pos => pos != default)
                .Select(p => new Vector3(p + offset - Main.screenPosition, 0))
                .ToArray();

            if (positions.Length <= 2)
            {
                return;
            }

            float brightness =
                MathF.Sin(Lifetime * MathHelper.Pi)
              * Main.atmo
              * MathF.Abs(Wind)
              * Utils.Remap(Parallax, parallax_min, parallax_max, parallax_scale_min, parallax_scale_max);

            Color color = Main.ColorOfTheSkies * brightness * alpha;
            color.A = 0;

            VertexPositionColorTexture[] vertices = TriangleStripBuilder.BuildPath(
                positions,
                t => MathF.Sin(t * MathHelper.Pi) * brightness * width,
                _ => color,
                smoothingSubdivisions: 2
            );

            if (vertices.Length <= 4)
            {
                return;
            }

            int[] indices = new int[((vertices.Length / 2) * 6) - 6];

            int start = currentVertices.Count;

            for (var i = 0; i < (vertices.Length / 2) - 2; i++)
            {
                indices[(i * 6)] = start + (i * 2);
                indices[(i * 6) + 1] = start + (i * 2) + 3;
                indices[(i * 6) + 2] = start + (i * 2) + 1;

                indices[(i * 6) + 3] = start + (i * 2);
                indices[(i * 6) + 4] = start + (i * 2) + 2;
                indices[(i * 6) + 5] = start + (i * 2) + 3;
            }

            currentVertices.AddRange(vertices);
            currentIndices.AddRange(indices);
        }
    }

    private const int bits_per_chunk = sizeof(ulong) * 8;

    private const int max_wind = 512;

    private static readonly WindParticle[] background_wind = new WindParticle[max_wind];
    private static readonly ulong[] background_wind_mask = new ulong[(int)Math.Ceiling((double)max_wind / bits_per_chunk)];

    private static readonly WindParticle[] foreground_wind = new WindParticle[max_wind];
    private static readonly ulong[] foreground_wind_mask = new ulong[(int)Math.Ceiling((double)max_wind / bits_per_chunk)];

    private static int activeParticles;

    // TODO: Use IStatic data
    private static DynamicVertexBuffer? vertexBuffer;

    private static DynamicIndexBuffer? indexBuffer;

    [OnLoad]
    private static void Load()
    {
        Main.RunOnMainThread(
            static () =>
            {
                vertexBuffer = new DynamicVertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), max_wind * max_old_positions * 2, BufferUsage.None);
                indexBuffer = new DynamicIndexBuffer(Main.graphics.GraphicsDevice, typeof(int), max_wind * max_old_positions * 6, BufferUsage.None);
            }
        ).GetAwaiter().GetResult();

        On_Main.DrawBackgroundBlackFill += DrawBackgroundBlackFill_BackgroundWind;

        On_Main.DrawInfernoRings += DrawInfernoRings_ForegroundWind;

        On_Dust.UpdateDust += UpdateDust_Wind;
    }

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        IL_Main.UpdateAudio += UpdateAudio_WindAmbience;
    }

    private static void UpdateAudio_WindAmbience(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel? windAmbienceTarget = null;
        ILLabel? escapeSwitchTarget = null;

        int indexIndex = -1;

        c.GotoNext(
            i => i.MatchLdloc(out _),
            i => i.MatchLdcI4(45),
            i => i.MatchBeq(out windAmbienceTarget)
        );

        Debug.Assert(windAmbienceTarget is not null);

        c.GotoLabel(windAmbienceTarget);

        c.FindPrev(
            out _,
            i => i.MatchBr(out escapeSwitchTarget)
        );

        Debug.Assert(escapeSwitchTarget is not null);

        c.EmitDelegate(
            static () =>
            {
                RedThunderstorm.Active = true;

                if (ModLoader.isLoading)
                {
                    return false;
                }

                const float decay = 0.005f;

                Ambient(Assets.Music.Wind.HeavenWind.Slot, AerieSubworld.Active && !RedThunderstorm.Active);
                Ambient(Assets.Music.Wind.RedStormWind.Slot, AerieSubworld.Active && RedThunderstorm.Active);

                if (AerieSubworld.Active)
                {
                    const int slot = 45;

                    float volume = Main.musicFade[slot];

                    Main.audioSystem.UpdateAmbientCueTowardStopping(slot, decay, ref volume, Main.ambientVolume);

                    Main.musicFade[slot] = volume;
                }

                return AerieSubworld.Active;

                static void Ambient(int slot, bool active)
                {
                    const float decay = 0.005f;

                    float volume = Main.musicFade[slot];

                    if (active)
                    {
                        Main.audioSystem.UpdateAmbientCueState(slot, Main.instance.IsActive, ref volume, Main.ambientVolume);
                    }
                    else
                    {
                        Main.audioSystem.UpdateAmbientCueTowardStopping(slot, decay, ref volume, Main.ambientVolume);
                    }

                    Main.musicFade[slot] = volume;
                }
            }
        );

        c.EmitBrtrue(escapeSwitchTarget);

        // Make sure the game doesn't treat the wind as music.
        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<Main>(nameof(Main.curMusic))
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchBr(out _)
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdsfld<Main>(nameof(Main.musicFade)),
            i => i.MatchLdloc(out indexIndex)
        );

        c.MoveAfterLabels();

        c.EmitLdloc(indexIndex);
        c.EmitDelegate(
            static (int index) =>
            {
                return index == Assets.Music.Wind.HeavenWind.Slot
                    || index == Assets.Music.Wind.RedStormWind.Slot;
            }
        );

        c.EmitBrtrue(escapeSwitchTarget);
    }

    [OnUnload]
    private static void Unload()
    {
        Main.RunOnMainThread(
            static () =>
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
            }
        ).GetAwaiter().GetResult();
    }

    private static void DrawBackgroundBlackFill_BackgroundWind(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
    {
        orig(self);

        if (Main.gameMenu || !AerieSubworld.Active || activeParticles == 0)
        {
            return;
        }

        // I'm lazy, don't do this.
        var snapshot = new SpriteBatchSnapshot(Main.spriteBatch);
        Main.spriteBatch.Restart(in snapshot);

        DrawWind(false);

        Main.spriteBatch.PrepRenderState();

        Main.spriteBatch.Restart(in snapshot);
    }

    private static void DrawInfernoRings_ForegroundWind(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (Main.gameMenu || !AerieSubworld.Active || activeParticles == 0)
        {
            return;
        }

        // I'm lazy, don't do this.
        var snapshot = new SpriteBatchSnapshot(Main.spriteBatch);
        Main.spriteBatch.Restart(in snapshot);

        DrawWind(true);

        Main.spriteBatch.PrepRenderState();

        Main.spriteBatch.Restart(in snapshot);
    }

    private static void UpdateDust_Wind(On_Dust.orig_UpdateDust orig)
    {
        const float spawn_chance = 7f;
        const float red_thunder_spawn_chance = 18f;

        orig();

        if (Main.gameMenu || !AerieSubworld.Active)
        {
            return;
        }

        UpdateWind();

        float spawnChance = RedThunderstorm.Active ? red_thunder_spawn_chance : spawn_chance;
        spawnChance /= MathF.Abs(Main.WindForVisuals);

        if (Main.rand.NextBool((int)spawnChance))
        {
            SpawnWind(Main.rand.NextBool());
        }
    }

    private static void UpdateWind()
    {
        activeParticles = 0;

        Update(false);
        Update(true);

        return;

        static void Update(bool foreground)
        {
            var bitChunks = foreground ? foreground_wind_mask : background_wind_mask;
            var array = foreground ? foreground_wind : background_wind;

            for (var i = 0; i < bitChunks.Length; i++)
            {
                var bits = bitChunks[i];

                while (bits != 0)
                {
                    var bitIndex = BitOperations.TrailingZeroCount(bits);
                    var index = i * bits_per_chunk + bitIndex;

                    ref var wind = ref array[index];

                    if (!wind.Update())
                    {
                        bitChunks[i] ^= 1uL << bitIndex;
                    }
                    else
                    {
                        activeParticles++;
                    }

                    bits &= bits - 1;
                }
            }
        }
    }

    private static int GetFirstInactive(ulong[] bitChunks)
    {
        for (var i = 0; i < bitChunks.Length; i++)
        {
            var offset = BitOperations.TrailingZeroCount(~bitChunks[i]);

            var allBitsAreOccupied = offset == bits_per_chunk;

            if (allBitsAreOccupied)
            {
                continue;
            }

            return offset + i * bits_per_chunk;
        }

        return -1;
    }

    private static void SpawnWind(bool foreground)
    {
        var bitChunks = foreground ? foreground_wind_mask : background_wind_mask;
        var array = foreground ? foreground_wind : background_wind;

        var index = GetFirstInactive(bitChunks);

        if (index <= -1)
        {
            return;
        }

        const int loop_chance = 10;

        const int offscreen_margin = 100;

        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);

        float offset = -Main.WindForVisuals + Math.Clamp(Main.LocalPlayer.velocity.X / 50, -3f, 3f);

        offset = Utils.Remap(offset, -1, 1, 0, 1, false);

        Vector2 screenCenter = Main.screenPosition +
            new Vector2(
                screenSize.X * offset,
                screenSize.Y * 0.5f
            );

        Rectangle spawn = Utils.CenteredRectangle(screenCenter, new Vector2(screenSize.X * 0.8f, screenSize.Y + 600));

        spawn.Inflate(offscreen_margin, offscreen_margin);

        Vector2 position = Main.rand.NextVector2FromRectangle(spawn);

        array[index] = CreateWind();

        var chunkIndex = index / bits_per_chunk;
        var bitIndex = index % bits_per_chunk;
        bitChunks[chunkIndex] ^= 1uL << bitIndex;

        return;

        WindParticle CreateWind()
        {
            return new WindParticle(
                position,
                new Vector2[max_old_positions],
                Vector2.Zero,
                Main.WindForVisuals,
                Main.rand.NextFloat(foreground ? 0 : parallax_min, foreground ? parallax_max : 0),
                Vector2.Zero,
                Main.rand.NextFloat(-loop_offset, loop_offset),
                !RedThunderstorm.Active && Main.rand.NextBool(loop_chance),
                0f
            );
        }
    }

    private static void DrawWind(bool foreground)
    {
        GraphicsDevice device = Main.graphics.GraphicsDevice;

        var bitChunks = foreground ? foreground_wind_mask : background_wind_mask;
        var array = foreground ? foreground_wind : background_wind;

        var vertices = new List<VertexPositionColorTexture>();
        var indices = new List<int>();

        for (var i = 0; i < bitChunks.Length; i++)
        {
            var bits = bitChunks[i];

            while (bits != 0)
            {
                var bitIndex = BitOperations.TrailingZeroCount(bits);
                var index = i * bits_per_chunk + bitIndex;

                var wind = array[index];

                wind.GenerateVertices(vertices, indices);

                bits &= bits - 1;
            }
        }

        vertexBuffer?.SetData(vertices.ToArray(), 0, vertices.Count);
        indexBuffer?.SetData(indices.ToArray(), 0, indices.Count);

        device.Textures[0] = Assets.Images.Bloom.Asset.Value;

        device.RasterizerState = RasterizerState.CullNone;
        device.Indices = indexBuffer;
        device.SetVertexBuffer(vertexBuffer);
        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Count, 0, indices.Count / 3);
    }
}

