using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Solstice.Core;
using SubworldLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Generation;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Solstice.Content.Aerie;

public partial class AerieSubworld : Subworld
{
    #region Edits
    private static readonly Type[] disabledPowerTypes =
    [
        typeof(CreativePowers.StartDayImmediately),
        typeof(CreativePowers.StartNightImmediately),
        typeof(CreativePowers.StartNoonImmediately),
        typeof(CreativePowers.StartMidnightImmediately),

        typeof(CreativePowers.FreezeTime),
        typeof(CreativePowers.ModifyTimeRate),

        typeof(CreativePowers.FreezeWindDirectionAndStrength),
        typeof(CreativePowers.ModifyWindDirectionAndStrength),

        typeof(CreativePowers.FreezeRainPower),
        typeof(CreativePowers.ModifyRainPower),

        typeof(CreativePowers.StopBiomeSpreadPower),
    ];

    [OnLoad]
    private static new void Load()
    {
        On_Player.DropTombstone += DropTombstone_DisableTombstones;

        foreach (var type in disabledPowerTypes)
        {
            MonoModHooks.Modify(
                type.GetMethod(
                    nameof(ICreativePower.GetIsUnlocked),
                    BindingFlags.Instance | BindingFlags.Public
                ),
                GetIsUnlocked_DisablePowers
            );
        }

        // Re-JIT various methods that reference any disabled power's GetIsUnlocked impl.
        IL_WorldGen.UpdateWorld_Inner += _ => { };
        IL_UICreativePowersMenu.WeatherCategoryButtonClick += _ => { };
        IL_UICreativePowersMenu.TimeCategoryButtonClick += _ => { };

        On_CreativePowersHelper.AddUnlockTextIfNeeded += AddUnlockTextIfNeeded_HideUnused;

        On_NPC.UpdateNPC_UpdateGravity += UpdateNPC_UpdateGravity_RemoveSpaceGravity;

        IL_Player.Update += Update_RemoveSpaceGravity;
    }

    private static ushort aerieMapSkyPosition;

    private const string aerie_map_sky_key = "AerieSkyGradient";

    private static readonly Color aerie_map_sky_gradient_top = new(176, 197, 247);
    private static readonly Color aerie_map_sky_gradient_mid = new(252, 150, 165);
    private static readonly Color aerie_map_sky_gradient_bot = new(255, 220, 178);

    [OnLoad(Side = ModSide.Client)]
    private static void LoadClient()
    {
        // TML lacks any convenient hooks after loading modded map entries;
        // luckily we don't have to unload our additional entries (see MapLoader.UnloadModMap.)
        MonoModHooks.Add(
            typeof(MapLoader).GetMethod(
                nameof(MapLoader.FinishSetup),
                BindingFlags.Static | BindingFlags.NonPublic
            ),
            FinishSetup_AddAerieSkyGradient
        );

        IL_MapHelper.CreateMapTile += CreateMapTile_UseAerieSkyGradient;

        MonoModHooks.Modify(
            typeof(MapIO).GetMethod(
                nameof(MapIO.WriteModMap),
                BindingFlags.Static | BindingFlags.NonPublic
            ),
            WriteModMap_UseAerieSkyGradient
        );

        MonoModHooks.Modify(
            typeof(MapIO).GetMethod(
                nameof(MapIO.ReadModMap),
                BindingFlags.Static | BindingFlags.NonPublic
            ),
            ReadModMap_UseAerieSkyGradient
        );
    }

    private static void WriteModMap_UseAerieSkyGradient(ILContext il)
    {
        var c = new ILCursor(il);

        int writerIndex = -1; // arg
        int typeIndex = -1; // loc

        ILLabel? loopTarget = null;

        c.GotoNext(
            i => i.MatchRet()
        );

        c.GotoNext(
            i => i.MatchLdarg(out writerIndex),
            i => i.MatchLdloc(out _),
            i => i.MatchCallvirt<ICollection<ushort>>($"get_{nameof(ICollection.Count)}")
        );

        c.GotoNext(
            i => i.MatchLdarg(writerIndex),
            i => i.MatchLdloc(out typeIndex)
        );

        c.GotoNext(
            i => i.MatchLdstr(string.Empty)
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchBr(out loopTarget)
        );

        Debug.Assert(loopTarget is not null);

        c.MoveAfterLabels();

        c.EmitLdarg(writerIndex);
        c.EmitLdloc(typeIndex);

        c.EmitDelegate(
            static (BinaryWriter writer, ushort type) =>
            {
                if (type < aerieMapSkyPosition || type >= aerieMapSkyPosition + byte.MaxValue)
                {
                    return false;
                }

                writer.Write(value: true);
                writer.Write(nameof(Solstice));
                writer.Write(aerie_map_sky_key);

                writer.Write((ushort)(type - aerieMapSkyPosition));

                return true;
            }
        );

        c.EmitBrtrue(loopTarget);
    }

    private static void ReadModMap_UseAerieSkyGradient(ILContext il)
    {
        var c = new ILCursor(il);

        int modNameIndex = -1; // loc, string
        int nameIndex = -1; // loc, string
        int optionIndex = -1; // loc, ushort

        c.GotoNext(
            MoveType.After,
            i => i.MatchCallvirt<BinaryReader>(nameof(BinaryReader.ReadString)),
            i => i.MatchStloc(out modNameIndex)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchCallvirt<BinaryReader>(nameof(BinaryReader.ReadString)),
            i => i.MatchStloc(out nameIndex)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchCallvirt<BinaryReader>(nameof(BinaryReader.ReadUInt16)),
            i => i.MatchStloc(out optionIndex)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdloc(out _),
            i => i.MatchLdloc(out _),
            i => i.MatchLdloc(out _),
            i => i.MatchCallvirt<IDictionary<ushort, ushort>>("set_Item")
        );

        c.Index--;

        c.EmitLdloc(modNameIndex);
        c.EmitLdloc(nameIndex);
        c.EmitLdloc(optionIndex);

        c.EmitDelegate(
            static (
                ushort newType,
                string modName,
                string name,
                ushort option
            ) =>
            {
                if (modName != nameof(Solstice) || name != aerie_map_sky_key)
                {
                    return newType;
                }

                return (ushort)(aerieMapSkyPosition + option);
            }
        );
    }

    private static void CreateMapTile_UseAerieSkyGradient(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld(typeof(MapHelper), nameof(MapHelper.skyPosition))
        );

        c.EmitDelegate(
            static (ushort skyPosition) =>
            {
                if (Active)
                {
                    return aerieMapSkyPosition;
                }

                return skyPosition;
            }
        );
    }

    private delegate void orig_FinishSetup();

    private static void FinishSetup_AddAerieSkyGradient(orig_FinishSetup orig)
    {
        const float upper_gradient_size = 0.6f;

        orig();

        aerieMapSkyPosition = (ushort)MapHelper.colorLookup.Length;

        Color[] colors = new Color[byte.MaxValue];

        int upper = (int)(colors.Length * upper_gradient_size);

        for (int i = 0; i < upper; i++)
        {
            colors[i] = Color.OklabLerp(aerie_map_sky_gradient_top, aerie_map_sky_gradient_mid, (float)i / upper);
        }

        for (int i = 0; i < colors.Length - upper; i++)
        {
            colors[upper + i] = Color.OklabLerp(aerie_map_sky_gradient_mid, aerie_map_sky_gradient_bot, (float)i / (colors.Length - upper));
        }

        Array.Resize(ref MapHelper.colorLookup, aerieMapSkyPosition + colors.Length);
        Lang._mapLegendCache.Resize(aerieMapSkyPosition + colors.Length);

        for (int k = 0; k < colors.Length; k++)
        {
            MapHelper.colorLookup[aerieMapSkyPosition + k] = colors[k];
            Lang._mapLegendCache[aerieMapSkyPosition + k] = LocalizedText.Empty;
        }
    }

    private static void DropTombstone_DisableTombstones(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, NetworkText deathText, int hitDirection)
    {
        if (Active)
        {
            return;
        }

        orig(self, coinsOwned, deathText, hitDirection);
    }

    private static void AddUnlockTextIfNeeded_HideUnused(On_CreativePowersHelper.orig_AddUnlockTextIfNeeded orig, ref string originalText, bool needed, string descriptionKey)
    {
        const string disabled_key = $"Mods.{nameof(Solstice)}.CreativePowers.DisabledInAerie";

        // Not full-proof but should work well enough as it's rather unlikely other mods will use
        // powers' unlock feature due to its unused/half implemented behaviour.
        if (Active && !needed)
        {
            descriptionKey = disabled_key;
        }

        orig(ref originalText, needed, descriptionKey);
    }

    private static void GetIsUnlocked_DisablePowers(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel jumpRetTarget = c.DefineLabel();

        c.EmitDelegate(static () => Active);

        c.EmitBrfalse(jumpRetTarget);

        c.EmitLdcI4(0);
        c.EmitRet();

        c.MarkLabel(jumpRetTarget);
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
        // Take in gravityMultiplier and out 1 if in our subworld, else ret.
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

    public override List<GenPass> Tasks => // TODO: Add actual generation so these run
    [
        new PassLegacy("Aerie Fake Loading", (progress, configuration) =>
        {
            // TODO: Check if it's the first time visiting
            Thread.Sleep(30000);
        }),
        new PassLegacy("Aerie Settings", AerieSubworldSettings),
    ];

    private static void AerieSubworldSettings(GenerationProgress progress, GameConfiguration configurations)
    {
        Main.worldSurface = Main.maxTilesY;
        Main.rockLayer = Main.maxTilesY + 42;
    }

    public override void OnLoad()
    {
        SubworldSystem.hideUnderworld = true;
    }

    [ModSystemHooks.PreUpdatePlayers]
    private static void UpdateSubworld()
    {
        if (!Active)
        {
            return;
        }

        Main.cloudAlpha = 0f;
        Main.cloudBGActive = 0f;

        foreach (Cloud cloud in Main.cloud)
        {
            cloud.active = false;
        }

        Main.raining = false;

        Main.time = 27000;
        Main.dayTime = true;

        Main.eclipse = false;

        Main.windSpeedTarget = -1f;
        Main.windSpeedCurrent = -1f;

        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        if (Main.LocalPlayer.position.Y > (Main.maxTilesY * 16f) - 1200 && Main.LocalPlayer.velocity.Y >= 0)
        {
            Main.LocalPlayer.wingTime = Main.LocalPlayer.wingTimeMax;
            Main.LocalPlayer.velocity.Y -= 1.2f;

            // Net-sync velocity changes.
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
        }
        else if (Main.LocalPlayer.position.Y > (Main.maxTilesY * 16f) - 1310 && Main.LocalPlayer.velocity.Y <= 0)
        {
            Main.LocalPlayer.velocity.Y -= 1f;

            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
        }
    }

    public override bool ShouldSave => true;

    [GlobalNPCHooks.EditSpawnPool]
    private static void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!Active)
        {
            return;
        }

        foreach (int index in pool.Keys)
        {
            pool[index] = 0f;
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
        if (!Active || !hiddenInfo.Contains(currentDisplay))
        {
            return;
        }

        displayValue = "???";

        displayColor = new Color(100, 100, 100, Main.mouseTextColor);
    }
}
