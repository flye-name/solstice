using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
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
            if (flames[i].Progress <= 0f)
                continue;
            
            var decrement = new UnifiedRandom(flames[i].Seed).NextFloat(0.02f, 0.05f);
            flames[i].Progress = MathF.Max(flames[i].Progress - decrement, 0);
        }


        if (Main.mouseRight)
        {
            New(Main.MouseWorld);
        }
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
        var texture = Assets.Images.Bloom.Asset.Value;
        var textureNoise = Assets.Images.NavierNoise.Asset.Value;
        var origin = texture.Size() / 2f;
        
        sb.End(out var ss);

        var shader = Data.Instance.FlameShader;
        shader.Apply();
        
        sb.Begin(ss with { CustomEffect = shader.Shader, SamplerState = SamplerState.PointWrap });

        Main.graphics.GraphicsDevice.Textures[1] = textureNoise;
        
        foreach (var flame in flames)
        {
            if (flame.Progress <= 0f)
                continue;
            
            var rand = new UnifiedRandom(flame.Seed);
            
            var position = flame.Position - Main.screenPosition;
            var scale = Vector2.One * flame.Progress * rand.NextFloat(0.1f, 0.25f);
            
            var color = Color.Orange * flame.Progress;
            color.A = 0;
            
            sb.Draw(new DrawParameters(texture)
            {
                Position = position,
                Origin = origin,
                Color = color,
                Scale = scale
            });
        }
        
        sb.Restart(in ss);
    }
    #endregion
}