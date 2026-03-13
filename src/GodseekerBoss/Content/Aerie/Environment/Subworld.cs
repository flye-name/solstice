using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace GodseekerBoss.Content.Aerie.Environment;

public class AerieSubworld : Subworld
{
    #region Edits
    [OnLoad]
    private static void Load()
    {
        On_NPC.UpdateNPC_UpdateGravity += UpdateNPC_UpdateGravity_RemoveSpaceGravity;

        IL_Player.Update += Update_RemoveSpaceGravity;
    }

    private static void UpdateNPC_UpdateGravity_RemoveSpaceGravity(On_NPC.orig_UpdateNPC_UpdateGravity orig, NPC self)
    {
        if (Active)
        {
            self.GravityIgnoresSpace = true;
        }

        orig(self);
    }

    private static void Update_RemoveSpaceGravity(ILContext il)
    {
        var c = new ILCursor(il);

        // gravity *= gravityMultiplier;
        // ---
        // ldarg.0
        // ldarg.0
        // ldfld float32 Terraria.Player::gravity
        // ldloc.3 - gravityMultiplier
        // ---
        // take in gravityMultiplier and out 1 if in our subworld, else ret
        // ---
        // mul
        // stfld float32 Terraria.Player::gravity

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg(out _),
            i => i.MatchLdarg(out _),
            i => i.MatchLdfld<Player>(nameof(Player.gravity))
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchMul(),
            i => i.MatchStfld<Player>(nameof(Player.gravity))
        );

        c.EmitDelegate(
            static (float multiplier) =>
            {
                if (Active)
                {
                    return 1f;
                }

                return multiplier;
            }
        );
    }
    #endregion

    public static bool Active => SubworldSystem.IsActive<AerieSubworld>();

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
        Main.windSpeedCurrent = -1f;

        // Lower cloud bounce [FIXME]
        if (Main.LocalPlayer.position.Y > (Main.maxTilesY * 16f) - 1200 && Main.LocalPlayer.velocity.Y >= 0)
        {
            Main.LocalPlayer.wingTime = Main.LocalPlayer.wingTimeMax;
            Main.LocalPlayer.velocity.Y -= 1.2f;
        }
        else if (Main.LocalPlayer.position.Y > (Main.maxTilesY * 16f) - 1310 && Main.LocalPlayer.velocity.Y <= 0)
        {
            Main.LocalPlayer.velocity.Y -= 1f;
        }
    }

    public override bool ShouldSave => true;

    [GlobalNPCHooks.EditSpawnPool]
    private static void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (Active)
        {
            foreach (int index in pool.Keys)
            {
                pool[index] = 0f;
            }
        }
    }

    private static readonly InfoDisplay[] hiddenInfo =
    [
        InfoDisplay.Watches,
        InfoDisplay.WeatherRadio,
        InfoDisplay.Compass,
        InfoDisplay.DepthMeter,
        InfoDisplay.Sextant
    ];

    [GlobalInfoDisplayHooks.ModifyDisplayParameters]
    private static void HideInfoAccs(
        InfoDisplay currentDisplay,
        ref string displayValue,
        ref string displayName,
        ref Color displayColor,
        ref Color displayShadowColor
    )
    {
        if (Active && hiddenInfo.Contains(currentDisplay))
        {
            displayValue = "???";

            displayColor = new Color(100, 100, 100, Main.mouseTextColor);
        }
    }
}
