using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solstice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Solstice.Content.Aerie.Weather;

public class RedThunderstormScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Stormseeker");

    public override bool IsSceneEffectActive(Player player) => AerieSubworld.Active && RedThunderstorm.Active;

    public override SceneEffectPriority Priority => SceneEffectPriority.Event;
}

public class RedThunderstorm : SkyModifier
{
    #region sky modifier
    public override SkyModifierPriority Priority => SkyModifierPriority.StrongWeather;
    public override bool IsActive => Active;
    private float _skyFlash;
    private float _musicVolume;
    public override void UpdateSky()
    {
        _skyFlash = MathHelper.Lerp(_skyFlash, 0, 0.01f);
        
        var colors = Color.ArrayLerp(PresetSkyColors.RED_THUNDERSTORM, PresetSkyColors.RED_THUNDERSTORM_FLASH, _skyFlash);
        
        SkyManagement.LerpSkyColors(colors, TransitionTime = MathF.Min(TransitionTime + 0.001f, 1f));
        
        if (_skyFlash > 0.27f)
            Main.musicVolume = MathHelper.Lerp(Main.musicVolume, MathF.Min(_musicVolume, 0.25f), 0.1f);
        else if (_skyFlash > 0.1f)
            Main.musicVolume = MathHelper.Lerp(Main.musicVolume, _musicVolume, 0.01f);
        else if (_skyFlash > 0.01f)
            Main.musicVolume = _musicVolume;
        else
            _musicVolume = Main.musicVolume;

        if (TransitionTime > 0.3f && Main.mouseRight && _skyFlash <= 0.1f)
        {
            _skyFlash = Main.rand.NextFloat(0.6f, 0.8f);
            SoundEngine.PlaySound(new SoundStyle("Solstice/Assets/Sounds/Thunder") with { Pitch = -0.5f }, Main.LocalPlayer.Center + Main.rand.NextVector2CircularEdge(500, 500));
            
            for (int i = 0; i < 3; i++)
                SpawnDefaultRedSprite();
        }
    }

    public override void ResetSkyModifierInformation()
    {
        base.ResetSkyModifierInformation();
        _musicVolume = Main.musicVolume;
        _skyFlash = 0f;
    }

    #endregion

    #region red sprites

    public static float Intensity;
    public static bool Active;
    public const int MaxSprites = 10; 
    public static RedSprite[] RedSprites = new RedSprite[MaxSprites];

    [OnLoad]
    public static void Load()
    {
        for (int i = 0; i < MaxSprites; i++)
            RedSprites[i] = new RedSprite(Vector2.Zero, 0)
            {
                Active = false
            };
    }

    public static void SpawnDefaultRedSprite()
    {
        Point screenSize = new(Main.screenWidth, Main.screenHeight);

        Rectangle area = new Rectangle(0, screenSize.Y / 3, screenSize.X, screenSize.Y);
        
        SpawnRedSprite(new RedSprite(Main.rand.NextVector2FromRectangle(area), 120));
    }
    
    public static void SpawnRedSprite(RedSprite obj)
    {
        int index = -1;
        for (int i = 0; i < MaxSprites; i++)
        {
            if (RedSprites[i].Active)
                continue;

            index = i;
        }

        if (index == -1)
            return;

        RedSprites[index] = obj;
    }

    static void UpdateRedSprite(int index)
    {
        ref RedSprite rs = ref RedSprites[index];

        rs.Lifetime--;
        if (--rs.Lifetime < 0)
        {
            for (int i = 0; i < RedSprite.MaxBranches; i++)
                rs.Points[i].Clear();
            rs.Active = false;
        }

        var progress = Utils.GetLerpValue(rs.MaxLifetime, 0, rs.Lifetime);

        if (progress < 0.25f)
        {
            float innerProgress = progress * 4;
            
            var branchAmount = rs.Random.Next(RedSprite.MaxBranches - 3, RedSprite.MaxBranches);
            for (int i = 0; i < branchAmount; i++)
            {
                var rand = new UnifiedRandom(rs.Seed);
                var branchRand = new UnifiedRandom(rs.Seed + i + 1);

                var startPosition = rs.Points[i][0];
                
                var direction = new Vector2(0, -1).RotatedBy(rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + branchRand.NextFloat(-0.5f, 0.5f) * MathF.Sin(MathF.PI * innerProgress));
                
                var newPosition = startPosition + direction * branchRand.NextFloat(1000, 1500);
                var position = Vector2.Lerp(startPosition, newPosition, innerProgress);
                if (rs.Points[i].Count > 2)
                    position += new Vector2(0, Main.rand.NextFloat(-50, 50)).RotatedBy((rs.Points[i].Last() - newPosition).ToRotation());
                
                if (rs.Points[i].Count <= 2 || rs.Points[i].Last().Distance(position) > 100)
                    rs.Points[i].Add(position);
            }
        }
    }

    public static void DrawRedSprites()
    {
        for (int i = 0; i < MaxSprites; i++)
        {
            ref var rs = ref RedSprites[i];
            if (!rs.Active)
                continue;
            
            Main.spriteBatch.End(out var snapshot);
            Main.spriteBatch.Begin(snapshot with { SamplerState = SamplerState.PointWrap });
            {
                DrawRedSprite(i);
            }
            Main.spriteBatch.Restart(in snapshot);
        }
    }
    
    public static void DrawRedSprite(int index)
    {
        ref RedSprite rs = ref RedSprites[index];

        Main.graphics.GraphicsDevice.Textures[0] = Assets.Images.Beam.Asset.Value;
        var progress = Utils.GetLerpValue(rs.MaxLifetime, 0, rs.Lifetime);
        for (int j = 0; j < RedSprite.MaxBranches; j++)
        {
            var curPositions = rs.Points[j].Where(x => x != default).ToList();
            if (curPositions.Count < 2) continue;
            
            var positions = curPositions.Select(x => new Vector3(x, 0)).ToList();

            var color = PresetSkyColors.RED_THUNDERSTORM[2] with { A = 0 } * Intensity; 
            var opacity = (1f - MathHelper.Clamp(progress - 0.5f, 0, 1) * 2) * MathF.Pow(progress * 0.6f, 2) * new UnifiedRandom(rs.Seed).NextFloat(3, 6);

            opacity *= 2;
            float width = 40;

            var vertices = TriangleStripBuilder.BuildPath(positions, _ => MathF.Sin(MathF.PI * _) * width, c => color * (1f - c) * opacity, smoothingSubdivisions: 2);
            
            if (vertices.Length > 3)
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
    }
    #endregion

    [ModSystemHooks.PostUpdateEverything]
    public static void Update()
    {
        Intensity = Active ? MathF.Min(1f, Intensity + 0.05f) : MathF.Max(0f, Intensity - 0.05f);
        
        Active = true;
            
        Wind.SpawnChance = 3f;

        // Red sprites are updated even if the event is inactive so clearing ones can fade out properly.
        for (int i = 0; i < MaxSprites; i++)
        {
            if (RedSprites[i].Active)
                UpdateRedSprite(i);
        }

        if (!Active)
            return;
    }
}