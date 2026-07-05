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
        public required VertexBuffer VertexBuffer { get; init; }
        public required IndexBuffer IndexBuffer { get; init; }

        public static Data LoadData(Mod mod)
        {
            return Main.RunOnMainThread(
                () => new Data
                {
                    FlameShader = Assets.Effects.Flame.CreateFlameShader(),
                    
                    VertexBuffer = new DynamicVertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), flame_count * 2, BufferUsage.None),
                    IndexBuffer = new DynamicIndexBuffer(Main.graphics.GraphicsDevice, typeof(int), flame_count * 6, BufferUsage.None)
                }
            ).GetAwaiter().GetResult();
        }

        public static void UnloadData(Data data)
        {
            Main.RunOnMainThread(() =>
            {
                Data.Instance.VertexBuffer?.Dispose();
                Data.Instance.IndexBuffer?.Dispose();
            }).GetAwaiter().GetResult();
        }
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
    private static readonly Vector2[] quadTexCoords = [
        Vector2.Zero,
        new(0, 1),
        new(1, 0),
        Vector2.One
    ];
            
    private static readonly Vector2[] quadOrigins = [
        -Vector2.One,
        new(-1, 1),
        new(1, -1),
        Vector2.One
    ];
    
    private static void GenerateFlameVertexData(List<int> indices, List<VertexPositionColorTexture> vertices, Vector2 baseOrigin)
    {
        foreach (var flame in flames)
        {
            if (flame.Progress <= 0f)
                continue;
            
            var rand = new UnifiedRandom(flame.Seed);
            
            var position = flame.Position - Main.screenPosition;
            var scale = flame.Progress * rand.NextFloat(0.1f, 0.25f);
            
            var color = Color.Orange * flame.Progress;
            color.A = 0;
            
            var newVertices = new VertexPositionColorTexture[4];
            
            for (int i = 0; i < 4; i++)
                newVertices[i] = new(new Vector3(baseOrigin * quadOrigins[i] * scale + position, 0), color, quadTexCoords[i]);
            
            var start = vertices.Count;
            int[] newIndices = [ start, start + 1, start + 2, start + 3, start + 2, start + 1 ];
            
            vertices.AddRange(newVertices);
            indices.AddRange(newIndices);
        }
    }
    
    public static void DrawFlames(SpriteBatch sb)
    {
        var texture = Assets.Images.Bloom.Asset.Value;
        var textureNoise = Assets.Images.NavierNoise.Asset.Value;

        var indices = new List<int>();
        var vertices = new List<VertexPositionColorTexture>();
        
        GenerateFlameVertexData(indices, vertices, texture.Size() / 2f);

        if (vertices.Count < 3)
            return;
        
        sb.End(out var ss);
        sb.Begin(ss with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.PointWrap });
        
        var device = Main.graphics.GraphicsDevice;
        device.RasterizerState = RasterizerState.CullNone;
        
        var shader = Data.Instance.FlameShader;

        var vertexBuffer = Data.Instance.VertexBuffer;
        var indexBuffer = Data.Instance.IndexBuffer;
        vertexBuffer.SetData(vertices.ToArray(), 0, vertices.Count);
        indexBuffer.SetData(indices.ToArray(), 0, indices.Count);
        device.SetVertexBuffer(vertexBuffer);
        device.Indices = indexBuffer;
            
        device.Textures[0] = texture;
        device.Textures[1] = textureNoise;
        shader.Apply();
        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Count, 0, indices.Count / 3);
        
        sb.Restart(in ss);
    }
    #endregion
}