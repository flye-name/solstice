using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent.Skies;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using BackgroundTextures = GodseekerBoss.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;
using MiscShaders = GodseekerBoss.GeneratedAssets.Assets.Effects;
using MiscTextures = GodseekerBoss.GeneratedAssets.Assets.Images.Textures;

namespace GodseekerBoss.Content.Aerie.Environment;

public class AerieBackground : ModSurfaceBackgroundStyle
{
    #region Edits
    /*
     * 1: 247, 177, 155 | 255, 218, 176
     * 2: 202, 134, 199 | 255, 187, 182
     * 3: 216, 105, 122 | 252, 156, 127
     * 4: 231, 80, 14 | 177, 92, 99
     * 5: 233, 151, 122 | 250, 201, 159
     * 6: 236, 57, 93 | 191, 40, 106
     * 7: 249, 203, 188 | 252, 162, 206
    */


    // private static readonly Color far_fog_color = new(247, 177, 155);
    // private static readonly Color mid_fog_color = new(255, 218, 176);

    [OnLoad]
    private static void Load()
    {
        On_Main.DrawSurfaceBG_BackMountainsStep1 += DrawSurfaceBG_BackMountainsStep1_Fog;
        On_Main.DrawSurfaceBG_BackMountainsStep2 += DrawSurfaceBG_BackMountainsStep2_Fog;

        IL_Main.DoDraw += DoDraw_CorrectSamplerState;

        On_AmbientSky.FadingSkyEntity.Update += Update_DisableSkyEntities;
        On_AmbientSky.FadingSkyEntity.UpdateOpacity += UpdateOpacity_HideSkyEntities;

        On_Main.DrawSunAndMoon += DrawSunAndMoon_HideSun;

        On_Main.DrawStarsInBackground += DrawStarsInBackground_Sky;
    }

    private static void DrawSurfaceBG_BackMountainsStep1_Fog(On_Main.orig_DrawSurfaceBG_BackMountainsStep1 orig, Main self, double backgroundTopMagicNumber, float bgGlobalScaleMultiplier, int pushBGTopHack)
    {
        orig(self, backgroundTopMagicNumber, bgGlobalScaleMultiplier, pushBGTopHack);

        if (AerieSubworld.Active)
        {
            Color far_fog_color = new(247, 177, 155);

            DrawFog(Main.spriteBatch, far_fog_color);
        }
    }

    private static void DrawSurfaceBG_BackMountainsStep2_Fog(On_Main.orig_DrawSurfaceBG_BackMountainsStep2 orig, Main self, int pushBGTopHack)
    {
        orig(self, pushBGTopHack);

        if (AerieSubworld.Active)
        {
            Color mid_fog_color = new(255, 218, 176);

            DrawFog(Main.spriteBatch, mid_fog_color);
        }
    }

    private static void DrawFog(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.End(out var snapshot);
        spriteBatch.Begin(snapshot with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.LinearWrap });
        {
            Vector2 size = new(Main.screenWidth, Main.screenHeight);

            var source = new Rectangle(0, Main.instance.bgTopY, (int)size.X, (int)size.Y);

            MiscShaders.AerieFog.Parallax = (float)(Main.screenPosition.X * Main.instance.bgParallax) / Main.screenWidth;

            MiscShaders.AerieFog.Time = Main.GlobalTimeWrappedHourly * 0.05f * ((float)Main.instance.bgParallax + 1f);

            MiscShaders.AerieFog.Source = new Vector4(source.X, source.Y, source.Width, source.Height);

            MiscShaders.AerieFog.Apply();

            spriteBatch.Draw(MiscTextures.CoherentNoise, source, color);
        }
        spriteBatch.Restart(in snapshot);
    }

    private static void DoDraw_CorrectSamplerState(ILContext il)
    {
        var c = new ILCursor(il);

        // Change the sampler state used for the background
        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch)),
            i => i.MatchLdcI4(0),
            i => i.MatchLdcI4(0),
            i => i.MatchCallvirt<OverlayManager>(nameof(OverlayManager.Draw))
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<SamplerState>(nameof(SamplerState.LinearClamp))
        );

        c.EmitPop();

        c.EmitDelegate(() => SamplerState.PointClamp);
    }

    private static void Update_DisableSkyEntities(On_AmbientSky.FadingSkyEntity.orig_Update orig, object self, int frameCount)
    {
        if (AerieSubworld.Active)
        {
            if (((AmbientSky.FadingSkyEntity)self).Opacity <= 0)
            {
                ((AmbientSky.FadingSkyEntity)self).IsActive = false;
            }
        }
        else
        {
            orig(self, frameCount);
        }
    }

    // Probs useless since objects should spawn with 0 opacity anyways, but still here to ensure that if objects spawn anyways, they are faded out and not instantly disabled
    private static void UpdateOpacity_HideSkyEntities(On_AmbientSky.FadingSkyEntity.orig_UpdateOpacity orig, object self, int frameCount)
    {
        if (AerieSubworld.Active)
        {
            if (((AmbientSky.FadingSkyEntity)self).Opacity > 0)
            {
                ((AmbientSky.FadingSkyEntity)self).Opacity--;
            }
            else
            {
                orig(self, frameCount);
            }
        }
        else
        {
            orig(self, frameCount);
        }
    }

    private static void DrawSunAndMoon_HideSun(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Microsoft.Xna.Framework.Color moonColor, Microsoft.Xna.Framework.Color sunColor, float tempMushroomInfluence)
    {
        if (!AerieSubworld.Active)
        {
            orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
        }
    }

    private static void DrawStarsInBackground_Sky(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        if (AerieSubworld.Active)
        {
            var skyDest = new Rectangle(0, 0, Main.screenWidth, Math.Max(Main.screenHeight, BackgroundTextures.Sky.Value.Height));

            Main.spriteBatch.Draw(BackgroundTextures.Sky, skyDest, Color.White);

            Main.spriteBatch.End(out var snapshot);
            Main.spriteBatch.Begin(snapshot with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.LinearClamp, BlendState = BlendState.Additive });
            {
                Vector2 size = new(Main.screenWidth, Main.screenHeight);

                var ringsDest = Utils.CenteredRectangle(size * 0.5f, new(MathF.Max(Main.screenWidth, Main.screenHeight)));

                MiscShaders.AerieRing.Time = Main.GlobalTimeWrappedHourly;

                MiscShaders.AerieRing.Apply();

                Main.spriteBatch.Draw(BackgroundTextures.Ring, ringsDest, Color.White);
            }
            Main.spriteBatch.Restart(in snapshot);
        }
        else
        {
            orig(self, sceneArea, artificial);
        }
    }

    #endregion

    public override void ModifyFarFades(float[] fades, float transitionSpeed)
    {
        for (int i = 0; i < fades.Length; i++)
        {
            if (i == Slot)
            {
                fades[i] += transitionSpeed;
                if (fades[i] > 1f)
                {
                    fades[i] = 1f;
                }
            }
            else
            {
                fades[i] -= transitionSpeed;
                if (fades[i] < 0f)
                {
                    fades[i] = 0f;
                }
            }
        }
    }

    public override int ChooseFarTexture()
    {
        return BackgroundTextureLoader.GetBackgroundSlot(BackgroundTextures.Far.Key);
    }

    public override int ChooseMiddleTexture()
    {
        return BackgroundTextureLoader.GetBackgroundSlot(BackgroundTextures.Mid.Key);
    }

    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
    {
        return -1;
    }
}
