using Daybreak.Hooks;
using Daybreak.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Utilities;
using static Solstice.Content.Aerie.RedThunderstormSky;

namespace Solstice.Content.Aerie;

public static class RedSpriteRendering
{
    private const int buffer_capacity = RedSprite.MaxBranches * RedSprite.MaxPoints * MaxSprites;
    
    private static DynamicVertexBuffer? vertexBuffer;
    private static DynamicIndexBuffer? indexBuffer;
    
    [OnLoad]
    private static void Load() => Main.RunOnMainThread(LoadBuffers).GetAwaiter().GetResult();
    
    [OnUnload]
    private static void Unload() => Main.RunOnMainThread(UnloadBuffers).GetAwaiter().GetResult();
    
    private static void LoadBuffers()
    {
        vertexBuffer = new DynamicVertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), buffer_capacity * 2, BufferUsage.None);
        indexBuffer = new DynamicIndexBuffer(Main.graphics.GraphicsDevice, typeof(int), buffer_capacity * 6, BufferUsage.None);
    }

    private static void UnloadBuffers()
    {
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
    }
    
    public static void DrawRedSprites()
    {
        List<int> indices = new();
        List<VertexPositionColorTexture> vertices = new();
        
        for (int i = 0; i < MaxSprites; i++)
            CreateData(ref RedSprites[i], indices, vertices);

        if (vertices.Count < 3)
            return;
        
        Main.spriteBatch.End(out var snapshot);
        Main.spriteBatch.Begin(snapshot with { SamplerState = SamplerState.PointWrap });
        {
            var device = Main.graphics.GraphicsDevice;
            device.RasterizerState = RasterizerState.CullNone;
            
            vertexBuffer?.SetData(vertices.ToArray(), 0, vertices.Count);
            indexBuffer?.SetData(indices.ToArray(), 0, indices.Count);
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;
            
            device.Textures[0] = Assets.Images.Beam.Asset.Value;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Count, 0, indices.Count / 3);
        }
        Main.spriteBatch.Restart(in snapshot);
    }

    private static VertexPositionColorTexture[]? GenerateIndividualBranch(ref RedSprite rs, int branchIndex, float progress)
    {
        var curPositions = rs.Points[branchIndex].Where(x => x != default).ToList();
        
        if (curPositions.Count < 2) return null;
            
        var positions = curPositions.Select(x => new Vector3(x, 0)).ToList();

        var color = PresetSkyColors.RED_THUNDERSTORM[2] with { A = 0 } * Intensity; 
        var opacity = (1f - MathHelper.Clamp(progress - 0.5f, 0, 1) * 2) * new UnifiedRandom(rs.Seed).NextFloat(0.25f, 1);

        opacity *= 2;
        float width = 40;
        
        return TriangleStripBuilder.BuildPath(positions, _ => MathF.Sin(MathF.PI * _) * width, c => color * (1f - c) * opacity, smoothingSubdivisions: 2);
    }
    
    private static void CreateData(ref RedSprite rs, List<int> indices, List<VertexPositionColorTexture> vertices)
    {
        if (!rs.Active)
            return;
        
        var progress = Utils.GetLerpValue(rs.MaxLifetime, 0, rs.Lifetime);
        for (int j = 0; j < RedSprite.MaxBranches; j++)
        {
            var newVertices = GenerateIndividualBranch(ref rs, j, progress); 

            if (newVertices is null || newVertices.Length < 3)
                continue;

            var newIndices = new int[(newVertices.Length * 3) - 6];

            int start = vertices.Count;
            
            for (var i = 0; i < newVertices.Length / 2 - 2; i++)
            {
                int index = i * 6;
                
                newIndices[index]     = start + (i * 2);
                newIndices[index + 1] = start + (i * 2) + 3;
                newIndices[index + 2] = start + (i * 2) + 1;

                newIndices[index + 3] = start + (i * 2);
                newIndices[index + 4] = start + (i * 2) + 2;
                newIndices[index + 5] = start + (i * 2) + 3;
            }
            
            vertices.AddRange(newVertices);
            indices.AddRange(newIndices);
        }
    }
}