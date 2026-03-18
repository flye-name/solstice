using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace GodseekerBoss.Common.IDs;

public static class GodseekerItemSets
{
    public readonly record struct TileReplacementInfo(bool UseSmartCursor, IDictionary<int, int> Replacements);

    public static TileReplacementInfo?[] TileReplacements { get; private set; } = [];

    private static Mod Mod => ModContent.GetInstance<GodseekerBoss>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        TileReplacements = CreateSet<TileReplacementInfo?>(nameof(TileReplacements), null);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return ItemID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

#region Edits
    [OnLoad]
    private static void Load()
    {
        // tMod hooks do not support non-publicized types.
        MonoModHooks.Add(
            typeof(SmartCursorHelper).GetMethod(
                nameof(SmartCursorHelper.Step_GrassSeeds),
                BindingFlags.NonPublic | BindingFlags.Static
            ),
            Step_GrassSeeds_Replacements
        );

        On_Player.PlaceThing_TryReplacingTiles += PlaceThing_TryReplacingTiles_Replacements;
    }

    public delegate void orig_Step_GrassSeeds(
        SmartCursorHelper.SmartCursorUsageInfo providedInfo,
        ref int focusedX,
        ref int focusedY
    );

    private static void Step_GrassSeeds_Replacements(orig_Step_GrassSeeds orig, SmartCursorHelper.SmartCursorUsageInfo providedInfo, ref int focusedX, ref int focusedY)
    {
        int type = providedInfo.item.type;

        var info = TileReplacements[type];

        if (info is null)
        {
            orig(providedInfo, ref focusedX, ref focusedY);

            return;
        }

        var replacementInfo = info.Value;

        SmartCursorHelper._targets.Clear();
        for (int i = providedInfo.reachableStartX; i <= providedInfo.reachableEndX; i++)
        {
            for (int j = providedInfo.reachableStartY; j <= providedInfo.reachableEndY; j++)
            {
                Tile tile = Main.tile[i, j];
                if (tile.HasTile && replacementInfo.Replacements.ContainsKey(tile.TileType))
                {
                    SmartCursorHelper._targets.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        if (SmartCursorHelper._targets.Count <= 0)
        {
            return;
        }

        float distance = -1f;
        Tuple<int, int> first = SmartCursorHelper._targets[0];

        foreach (Tuple<int, int> target in SmartCursorHelper._targets)
        {
            Vector2 position = new Vector2(target.Item1, target.Item2) * 16f;
            position += new Vector2(8f);

            float newDistance = Vector2.Distance(position, providedInfo.mouse);

            if ((int)distance != -1 && !(newDistance < distance))
            {
                continue;
            }

            distance = newDistance;
            first = target;
        }

        if (!Collision.InTileBounds(
                first.Item1, first.Item2,
                providedInfo.reachableStartX, providedInfo.reachableStartY,
                providedInfo.reachableEndX, providedInfo.reachableEndY))
        {
            return;
        }

        focusedX = first.Item1;
        focusedY = first.Item2;
    }

    private static bool PlaceThing_TryReplacingTiles_Replacements(On_Player.orig_PlaceThing_TryReplacingTiles orig, Player self, bool canUse)
    {
        int i = Player.tileTargetX;
        int j = Player.tileTargetY;

        Tile tile = Framing.GetTileSafely(i, j);

        Item item = self.inventory[self.selectedItem];

        var info = TileReplacements[item.type];

        if (info is null || !canUse)
        {
            return orig(self, canUse);
        }

        var replacementInfo = info.Value;

        if (!tile.HasTile ||
            !replacementInfo.Replacements.ContainsKey(tile.TileType) ||
            !self.IsInTileInteractionRange(i, j, TileReachCheckSettings.Simple) ||
            !(self.controlUseItem && self.itemAnimation > 0 && self.ItemTimeIsZero))
        {
            return false;
        }

        Main.tile[i, j].TileType = (ushort)replacementInfo.Replacements[tile.TileType];

        WorldGen.SquareTileFrame(i, j, true);

        SoundEngine.PlaySound(SoundID.Dig, i * 16, j * 16);

        self.ApplyItemTime(item, self.tileSpeed);

        return false;
    }
#endregion
}
