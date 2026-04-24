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
using BackgroundTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;
using MiscShaders = Godseeker.GeneratedAssets.Assets.Effects;
using MiscTextures = Godseeker.GeneratedAssets.Assets.Images.Textures;

namespace Godseeker.Content.Aerie;

public sealed class AerieBackground : ModSurfaceBackgroundStyle
{
#region Edits
    private static readonly Color far_fog_color = new(247, 177, 155);
    private static readonly Color mid_fog_color = new(255, 218, 176);
    private static readonly Color near_fog_color = new(255, 248, 227);
    private static readonly Color behind_tiles_fog_color = Color.Lerp(mid_fog_color, near_fog_color, 0.65f);

    [OnLoad(Side = ModSide.Client)]
    private static new void Load()
    {
        IL_Main.DrawBG += DrawBG_RemoveSpaceOffset;

        On_Main.UpdateAtmosphereTransparencyToSkyColor += UpdateAtmosphereTransparencyToSkyColor_HideSpace;

        On_Main.DrawSurfaceBG_BackMountainsStep1 += DrawSurfaceBG_BackMountainsStep1_Fog;
        On_Main.DrawSurfaceBG_BackMountainsStep2 += DrawSurfaceBG_BackMountainsStep2_Fog;

        IL_Main.DoDraw += DoDraw_CorrectSamplerState;

        On_AmbientSky.FadingSkyEntity.Update += Update_DisableSkyEntities;
        On_AmbientSky.FadingSkyEntity.UpdateOpacity += UpdateOpacity_HideSkyEntities;

        On_Main.DrawSunAndMoon += DrawSunAndMoon_HideSun;

        On_Main.DrawStarsInBackground += DrawStarsInBackground_Sky;
    }

    [ModSystemHooks.PostSetupContent]
    private static void PostLoad()
    {
        if (Main.dedServ)
        {
            return;
        }

        // Should be loaded after main loading is complete to render above wind.
        On_Main.DrawBackgroundBlackFill += DrawBackgroundBlackFill_Fog;

        On_Main.DrawInfernoRings += DrawInfernoRings_Fog;
    }

    private static void DrawBackgroundBlackFill_Fog(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
    {
        orig(self);

        if (!AerieSubworld.Active)
        {
            return;
        }

        const float parallax = 0.88f;

        float top = (Main.maxTilesY * 16f) + 16f - (Main.screenPosition.Y + Main.screenHeight + 900);

        top *= parallax;

        DrawFog(Main.spriteBatch, behind_tiles_fog_color, (int)top, parallax, true);
    }

    private static void DrawInfernoRings_Fog(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (!AerieSubworld.Active)
        {
            return;
        }

        const float parallax = 1.12f;

        float top = (Main.maxTilesY * 16f) + 16f - (Main.screenHeight + 760);

        top -= Main.screenPosition.Y;

        top *= parallax;

        DrawFog(Main.spriteBatch, near_fog_color, (int)top, parallax, true);
    }

    private static void DrawBG_RemoveSpaceOffset(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.Before,
            i => i.MatchStfld<Main>(nameof(Main.scAdj))
        );

        c.EmitDelegate(
            static (float num) =>
            {
                if (AerieSubworld.Active)
                {
                    // inverse of the original calculation, with a few nitpicky changes
                    // ---
                    // float num = Math.Min(PlayerInput.RealScreenHeight, LogicCheckScreenHeight);
                    // float num2 = screenPosition.Y + (float)(screenHeight / 2) - num / 2f;
                    // scAdj = (float)(worldSurface * 16.0) / (num2 + num);
                    // ---

                    return 2.75f - (Main.screenPosition.Y + (Main.screenHeight * 0.5f)) / ((Main.worldSurface * 16f) + 16f) * 2.3f;
                }
                return num;
            }
        );
    }

    private static void UpdateAtmosphereTransparencyToSkyColor_HideSpace(On_Main.orig_UpdateAtmosphereTransparencyToSkyColor orig)
    {
        if (!AerieSubworld.Active)
        {
            orig();
        }
    }

    private static void DrawSurfaceBG_BackMountainsStep1_Fog(On_Main.orig_DrawSurfaceBG_BackMountainsStep1 orig, Main self, double backgroundTopMagicNumber, float bgGlobalScaleMultiplier, int pushBGTopHack)
    {
        orig(self, backgroundTopMagicNumber, bgGlobalScaleMultiplier, pushBGTopHack);

        if (AerieSubworld.Active)
        {
            DrawFog(Main.spriteBatch, far_fog_color);
        }
    }

    private static void DrawSurfaceBG_BackMountainsStep2_Fog(On_Main.orig_DrawSurfaceBG_BackMountainsStep2 orig, Main self, int pushBGTopHack)
    {
        orig(self, pushBGTopHack);

        if (AerieSubworld.Active)
        {
            DrawFog(Main.spriteBatch, mid_fog_color);
        }
    }

    private static void DrawFog(SpriteBatch spriteBatch, Color color, int top = -1, float parallax = -1, bool useZoom = false)
    {
        if (top == -1)
        {
            top = Main.instance.bgTopY;
        }
        if (parallax < 0f)
        {
            parallax = (float)Main.instance.bgParallax;
        }

        if (top > Main.screenHeight)
        {
            return;
        }

        var zoom = useZoom ? Main.GameViewMatrix.Zoom : new Vector2(1f);

        spriteBatch.End(out var snapshot);
        spriteBatch.Begin(snapshot with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.LinearWrap });
        { 
            float size = MathF.Max(Main.screenWidth, Main.screenHeight);

            var source = new Rectangle(0, (int)MathF.Max(top, Main.screenHeight - size), (int)size, (int)size);

            MiscShaders.AerieFog.Parallax = Main.screenPosition.X * parallax / Main.screenWidth;

            MiscShaders.AerieFog.Time = Main.GlobalTimeWrappedHourly * 0.05f * (parallax + 1f);

            var offset = new Vector2(Main.screenPosition.X % 2, Main.screenPosition.Y % 2);

            MiscShaders.AerieFog.Source = new Vector4(offset.X, offset.Y, source.Width, source.Height);

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

    private static void UpdateOpacity_HideSkyEntities(On_AmbientSky.FadingSkyEntity.orig_UpdateOpacity orig, object self, int frameCount)
    {
        if (AerieSubworld.Active && ((AmbientSky.FadingSkyEntity)self).Opacity > 0)
        {
            ((AmbientSky.FadingSkyEntity)self).Opacity--;

            return;
        }

        orig(self, frameCount);

    }

    private static void DrawSunAndMoon_HideSun(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
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

            Stars.DrawStars(Main.spriteBatch);

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
