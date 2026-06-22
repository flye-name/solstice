using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace Solstice.Content.Aerie;

public class PreBossNight : SkyModifier
{
    #region sky modifier
    public override SkyModifierPriority Priority => SkyModifierPriority.StrongWeather;
    public override bool IsActive => Active;
    public override void UpdateSky()
    {
        SkyManagement.LerpSkyColors([
            new Color(0,0,0),
            new Color(116, 131, 250) * 0.07f,
            new Color(116, 131, 250) * 0.1f,
        ], TransitionTime = MathF.Min(TransitionTime + 0.001f, 1f));
    }
    #endregion

    public static bool Active;
    public static float Intensity;

    public static void DrawComet()
    {
        if (!Active)
            return;
        
        float EaseOutCirc(float x) => MathF.Sqrt(1 - MathF.Pow(x - 1, 2));

        var sb = Main.spriteBatch;

        var light = Assets.Images.Light.Asset.Value;
        var beam = Assets.Images.Beam.Asset.Value;
        var flare = Assets.Images.Flare.Asset.Value;
        var trail = Assets.Images.FuzzyTrail.Asset.Value;
        var bloom = Assets.Images.Bloom.Asset.Value;

        var baseColor = new Color(116, 131, 250);

        var position = Main.MouseScreen;
        var positions = new List<Vector3>();
        var altPositions = new List<Vector3>();
        var flarePosition = position;

        var sin = MathF.Sin(Main.GlobalTimeWrappedHourly * 14);
        var flicker = 1f + sin * (MathF.Sign(sin) <= 0 ? 0.025f : 0.01f);

        for (float i = 0; i < 1; i += 0.025f)
        {
            var midPoint = new Vector2(240, -130);
            var offset = Vector2.Lerp(Vector2.Lerp(Vector2.Zero, midPoint, i), Vector2.Lerp(midPoint, new Vector2(480, -200), i), i);
            
            if (i < 0.15f)
                flarePosition = position + offset - new Vector2(6f, -1);
            if (i > 0.05f)
                positions.Add(new Vector3(position + offset.RotatedBy(-0.05f) * 1.5f, 0));
            
            altPositions.Add(new Vector3(position + offset.RotatedBy(0.15f) * 1.4f - new Vector2(20, 10), 0));
        }

        
        var color = baseColor * Intensity;

        var vertices = TriangleStripBuilder.BuildPath(positions,
            _ => 20 * EaseOutCirc(MathF.Pow(_, 0.5f)) + 10 * MathF.Pow(1 - _, 3),
            c => Color.Lerp(color, Color.White, (1f - c) * 0.4f) * (1f - c) * MathHelper.Lerp(MathF.Pow(flicker, 2), 1, c), smoothingSubdivisions: 2);

        
        if (vertices.Length > 3) 
        {
            Main.graphics.GraphicsDevice.Textures[0] = light;   
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
        
        vertices = TriangleStripBuilder.BuildPath(altPositions, 
            _ => MathHelper.Clamp((1f - MathF.Pow(1f - _, 4)) * 350, 75, 350),
            c => color * MathF.Sin(c * MathF.PI) * MathHelper.Lerp(1, flicker, c) * 0.8f, smoothingSubdivisions: 2);

        if (vertices.Length > 3)
        {
            Main.graphics.GraphicsDevice.Textures[0] = light;   
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color *= 0.3f;
                vertices[i].TextureCoordinate.X += Main.GlobalTimeWrappedHourly * -.1f;
            }
            
            Main.graphics.GraphicsDevice.Textures[0] = trail;   
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }

        var rotation = (position - new Vector2(positions.Last().X, positions.Last().Y)).ToRotation();
        
        
        for (int i = 0; i < 2; i++)
            sb.Draw(flare, flarePosition, null, (i == 0 ? Color.White * 0.6f : color) * flicker, 0, flare.Size() / 2f, (0.3f + i * (flicker - 1) * 0.1f), SpriteEffects.None, 0);
    }
    
    [ModSystemHooks.PostUpdateEverything]
    public static void Update()
    {
        Active = true;
        
        Intensity = Active ? MathF.Min(1f, Intensity + 0.05f) : MathF.Max(0f, Intensity - 0.05f);
    }
}