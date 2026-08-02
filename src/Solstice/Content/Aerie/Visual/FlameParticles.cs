using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Mathematics;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Solstice.Content.Aerie;

public static class FlameParticles
{
    #region data
    private const int flame_count = 600;
    
    private sealed class Data : IStatic<Data>
    {
        public required WrapperShaderData<Assets.Effects.Flame.Parameters> FlameShader { get; init; }

        public static Data LoadData(Mod mod)
        {
            return Main.RunOnMainThread(
                () => new Data
                {
                    FlameShader = Assets.Effects.Flame.CreateFlameShader(),
                }
            ).GetAwaiter().GetResult();
        }

        public static void UnloadData(Data data) { }
    }
    
    private record struct Flame(int Seed)
    {
        public Vector2 Position = Vector2.Zero;
        public float Progress = 0f;
        public Angle Rotation;
    }
    
    private static readonly Flame[] flames = new Flame[flame_count];
    

    [OnLoad(Side = ModSide.Client)]
    private static void Load()
    {
        for (int i = 0; i < flames.Length; i++)
        {
            var seed = Main.rand.Next(int.MaxValue);
            flames[i] = new Flame(seed);
        }

        On_Main.DrawInfernoRings += (orig, self) =>
        {
            orig(self);
            
            DrawFlames(Main.spriteBatch);
        };
    }
    #endregion

    #region functional
    [ModSystemHooks.PostUpdateDusts]
    private static void UpdateFlames()
    {
        for (int i = 0; i < flame_count; i++)
        {
            ref float progress = ref flames[i].Progress;
            
            if (progress <= 0f)
                continue;

            var rand = new UnifiedRandom(flames[i].Seed);
            
            var decrement = rand.NextFloat(0.02f, 0.05f);
            progress = MathF.Max(progress - decrement, 0);

            var fallingVelocity = rand.NextVector2Unit() * rand.NextFloat(5, 15);
            var risingVelocity = new Vector2(rand.NextFloat(-4, 4), rand.NextFloat(-30, -15));
            var velocity = Vector2.Lerp(fallingVelocity, risingVelocity, progress * 0.8f) * progress;
            
            flames[i].Rotation = Angle.FromVector(velocity.RotatedBy(MathHelper.PiOver2));
            flames[i].Position += velocity;
        }


        /*if (Main.mouseRight)
        {
            var max = 12f;
            for (int i = 0; i < max; i++)
                New(Vector2.Lerp(new Vector2(Main.lastMouseX, Main.lastMouseY) + Main.screenPosition, Main.MouseWorld, i / max));
        }*/
    }

    public static void New(Vector2 position)
    {
        var availableFlames = flames.Where(x => x.Progress <= 0f).ToList();

        if (availableFlames.Count <= 0)
            return;
        
        var index = flames.IndexOf(availableFlames.Last());

        flames[index].Position = position;
        flames[index].Progress = 1f;
    }
    #endregion

    #region rendering
    public static void DrawFlames(SpriteBatch sb)
    {
        var texture = Assets.Images.Fire.Asset.Value;
        var textureNoise = Assets.Images.CoherentNoise.Asset.Value;
        var baseOrigin = texture.Size() / 2f;
        
        sb.End(out var ss);

        var shader = Data.Instance.FlameShader;

        shader.Parameters.uDirection = new Vector2(0.2f, 1f);
        shader.Parameters.uNoiseStrength = 0.2f;
        shader.Parameters.uColorQuantity = 8;
        shader.Parameters.uNoiseColorQuantity = 4;
        
        shader.Apply();
        
        sb.Begin(ss with { CustomEffect = shader.Shader, SamplerState = SamplerState.PointWrap });

        Main.graphics.GraphicsDevice.Textures[1] = textureNoise;
        
        for (int i = flame_count - 1; i > 0; i--)
        {
            var flame = flames[i];
            if (flame.Progress <= 0f)
                continue;
            
            var rand = new UnifiedRandom(flame.Seed);

            var charred = rand.NextBool(3);
            
            var position = flame.Position - Main.screenPosition;
            var charredScale = new Vector2(MathF.Pow(flame.Progress, 2), 1);
            var scale = (charred ? charredScale : Vector2.One) * flame.Progress * rand.NextFloat(.5f, 1f);

            var origin = new Vector2(baseOrigin.X, baseOrigin.Y * flame.Progress);
            
            var rotation = flame.Rotation.LerpTo(Angle.FromRadians(rand.NextFloat(MathF.Tau)), flame.Progress);

            position += Main.rand.NextVector2Circular(5, 5) * scale;
            
            var color = Color.Lerp(Color.Lerp(Color.Gold with { A = 30 }, Color.OrangeRed with { A = 120 }, (1.1f - flame.Progress) * 5), Color.Black, MathHelper.Clamp((1f - flame.Progress) * 1.5f, 0, 1));
            if (!charred)
                color = Color.Lerp(Color.Gold with { A = 30 }, Color.OrangeRed with { A = 120 }, (1.1f - flame.Progress) * 5) * MathF.Pow(flame.Progress, 2);
            color *= flame.Progress;
            
            if (!charred)
                sb.Draw(new DrawParameters(texture)
                {
                    Position = position,
                    Origin = origin,
                    Color = color * 0.25f * scale.X,
                    Rotation = rotation,
                    Scale = scale * 1.5f
                });
            
            sb.Draw(new DrawParameters(texture)
            {
                Position = position,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            });
        }
        
        sb.Restart(in ss);
    }
    #endregion
}