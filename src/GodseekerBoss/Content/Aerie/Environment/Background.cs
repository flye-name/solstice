using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent.Skies;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using BackgroundTextures = GodseekerBoss.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;

namespace GodseekerBoss.Content.Aerie.Environment;

public class AerieBackground : ModSurfaceBackgroundStyle
{
    #region Edits

    [OnLoad]
    private static void Load()
    {
        IL_Main.DoDraw += DoDraw_CaptureSky;

        On_AmbientSky.FadingSkyEntity.Update += Update_DisableSkyEntities;
        On_AmbientSky.FadingSkyEntity.UpdateOpacity += UpdateOpacity_HideSkyEntities;

        On_Main.DrawSunAndMoon += DrawSunAndMoon_HideSun;

        On_Main.DrawStarsInBackground += DrawStarsInBackground_Gradient;
    }

    private static void DoDraw_CaptureSky(ILContext il)
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

    private static void DrawStarsInBackground_Gradient(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        if (AerieSubworld.Active)
        {
            var dest = new Rectangle(0, 0, Main.screenWidth, Math.Max(Main.screenHeight, BackgroundTextures.Sky.Value.Height));

            Main.spriteBatch.Draw(BackgroundTextures.Sky, dest, Color.White);
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
