using Daybreak.Common.Features.Hooks;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.GameContent.Skies;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace GodseekerBoss.Core.Subworlds;

public class DragonAerieSubworld : Subworld
{
    #region Edits

    [OnLoad]
    private static void Load()
    {
        On_AmbientSky.FadingSkyEntity.Update += Update_DisableSkyEntities;
        On_AmbientSky.FadingSkyEntity.UpdateOpacity += UpdateOpacity_HideSkyEntities;

        On_Main.DrawSunAndMoon += DrawSunAndMoon_HideSun;
    }

    private static void Update_DisableSkyEntities(On_AmbientSky.FadingSkyEntity.orig_Update orig, object self, int frameCount)
    {
        if (Active)
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

    //Probs useless since objects should spawn with 0 opacity anyways, but still here to ensure that if objects spawn anyways, they are faded out and not instantly disabled
    private static void UpdateOpacity_HideSkyEntities(On_AmbientSky.FadingSkyEntity.orig_UpdateOpacity orig, object self, int frameCount)
    {
        if (Active)
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
        if (!Active)
        {
            orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
        }
    }

    private sealed class DisableSpawns : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (Active)
            {
                foreach (int index in pool.Keys)
                {
                    pool[index] = 0f;
                }
            }
        }
    }

    #endregion

    public static bool Active => SubworldSystem.IsActive<DragonAerieSubworld>();

    public override int Width => 1000;

    public override int Height => 400;

    public override List<GenPass> Tasks => new List<GenPass>() 
    {
        new PassLegacy("Aerie Settings", new WorldGenLegacyMethod(AerieSubworldSettings))
    };

    private static void AerieSubworldSettings(GenerationProgress progres, GameConfiguration configurations)
    {
        Main.worldSurface = Main.maxTilesY;
        Main.rockLayer = Main.maxTilesY + 42;
    }

    public override void SetStaticDefaults()
    {
            
    }

    public override void OnLoad()
    {
        SubworldSystem.hideUnderworld = true;
    }

    public override void Update()
    {
        Main.cloudAlpha = 0f;
        Main.raining = false;
        Main.cloudBGActive = 0f;
        for (int i = 0; i < Main.cloud.Length; i++)
        {
            Main.cloud[i].active = false;
        }
        Main.time = 27000;
        Main.dayTime = true;
        Main.eclipse = false;
        Main.windSpeedTarget = -1f;
    }

    // public override bool ShouldSave => true;
}
