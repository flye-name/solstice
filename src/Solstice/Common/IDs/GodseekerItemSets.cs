using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Solstice.Common;

public static class SolsticeItemSets
{
    public readonly record struct TileReplacementInfo(bool UseSmartCursor, IDictionary<int, int> Replacements);

    /// <summary>
    /// If and how this item should "replace" tiles, similarly to grass seeds or moss.
    /// </summary>
    public static TileReplacementInfo?[] TileReplacements { get; private set; } = [];

    public readonly record struct ItemSwapInfo(int SwapTo, bool ShiftToSwap, SoundStyle? SoundType);

    /// <summary>
    /// Makes this item swap to a different item when right-clicked or alt-fired, similarly to the Rubblemaker.
    /// </summary>
    public static ItemSwapInfo?[] SwapsTo { get; private set; } = [];

    /// <summary>
    /// Makes this item count as the supplied items when stacking/sorting.
    /// </summary>
    public static int[]?[] CountsAs { get; private set; } = [];

    // TODO: Extend into versatile animation set.
    /// <summary>
    /// Makes this item use a custom single frame, without need for an animation.
    /// </summary>
    public static Rectangle?[] StaticFrame { get; private set; } = [];

    private static Mod Mod => ModContent.GetInstance<Solstice>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        TileReplacements = CreateSet<TileReplacementInfo?>(nameof(TileReplacements), null);
        SwapsTo = CreateSet<ItemSwapInfo?>(nameof(SwapsTo), null);
        CountsAs = CreateSet<int[]?>(nameof(CountsAs), null);
        StaticFrame = CreateSet<Rectangle?>(nameof(StaticFrame), null);

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
        On_Player.GetItemDrawFrame += GetItemDrawFrame_UseStaticFrame;
        On_Main.DrawItem_GetBasics += DrawItem_GetBasics_UseStaticFrame;
        // Praying this is too big to be inlined.
        On_Main.GetItemDrawFrame += GetItemDrawFrame_UseStaticFrame;
        IL_ItemSlot.DrawItemIcon += DrawItemIcon_UseStaticFrame;
        On_Item.GetDrawHitbox += GetDrawHitbox_UseStaticFrame;

        // Item stacking for similar items.
        IL_ItemSorting.Sort += Sort_CountsAs;
        On_Item.IsTheSameAs += IsTheSameAs_CountsAs;
        // Re-JIT various vanilla methods that use 'Item.IsTheSameAs.'
        {
            IL_Chest.PutItemInNearbyChest += _ => { };
            IL_Main.TryAllowingToCraftRecipe += _ => { };
            IL_Player.ItemSpace += _ => { };
            IL_Player.CanItemSlotAccept += _ => { };
            IL_Player.DoCoins += _ => { };
            IL_Player.FillAmmo += _ => { };
            IL_Player.GetItem_FillIntoOccupiedSlot_VoidBag += _ => { };
            IL_Player.GetItem_FillIntoOccupiedSlot += _ => { };
            IL_Recipe.ConsumeForCraft += _ => { };
            IL_Recipe.CollectGuideRecipes += _ => { };
            IL_ChestUI.DepositAll += _ => { };
            IL_ChestUI.TryPlacingInChest += _ => { };
            IL_ItemSlot.LeftClick_ItemArray_int_int += _ => { };
            IL_ItemSlot.RightClick_ItemArray_int_int += _ => { };
            IL_ItemSlot.HandleShopSlot += _ => { };
            IL_ItemSlot.AccCheck += _ => { };
            IL_ItemSlot.AccessorySwap += _ => { };
            IL_ItemSlot.AccCheck_ForPlayer += _ => { };
        }

        // Replicate vanilla item swapping behaviour for custom values.
        IL_Player.ItemCheck_ManageRightClickFeatures += ItemCheck_ManageRightClickFeatures_SwapItem;
        On_ItemSlot.TryItemSwap += TryItemSwap_SwapItem;

        // Replicate vanilla tile replacement behavior (similar to moss and grass seeds) with
        // any tiles.
        // TODO:
        //      - HitSound options
        //      - Better impl that integrates with vanilla moss replacement correctly
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

#region Static Frame
    private static Rectangle GetItemDrawFrame_UseStaticFrame(On_Player.orig_GetItemDrawFrame orig, Player self, int type)
    {
        var rectangle = StaticFrame[type];

        if (rectangle is null)
        {
            return orig(self, type);
        }

        return rectangle.Value;
    }

    private static void DrawItem_GetBasics_UseStaticFrame(On_Main.orig_DrawItem_GetBasics orig, Main self, Item item, int slot, out Texture2D texture, out Rectangle frame, out Rectangle glowmaskFrame)
    {
        orig(self, item, slot, out texture, out frame, out glowmaskFrame);

        var rectangle = StaticFrame[item.type];

        if (rectangle is null)
        {
            return;
        }

        frame = glowmaskFrame = rectangle.Value;
    }

    private static void GetItemDrawFrame_UseStaticFrame(On_Main.orig_GetItemDrawFrame orig, int item, out Texture2D itemTexture, out Rectangle itemFrame)
    {
        orig(item, out itemTexture, out itemFrame);

        var rectangle = StaticFrame[item];

        if (rectangle is null)
        {
            return;
        }

        itemFrame = rectangle.Value;
    }

    private static void DrawItemIcon_UseStaticFrame(ILContext il)
    {
        var c = new ILCursor(il);

        int itemTypeIndex = -1; // loc
        int frameIndex = -1; // loc

        ILLabel? ternaryJumpTarget = null;

        c.GotoNext(
            i => i.MatchLdarg(out _),
            i => i.MatchLdfld<Item>(nameof(Item.type)),
            i => i.MatchStloc(out itemTypeIndex)
        );

        c.GotoNext(
            i => i.MatchCallvirt<DrawAnimation>(nameof(DrawAnimation.GetFrame)),
            i => i.MatchBr(out ternaryJumpTarget)
        );

        Debug.Assert(ternaryJumpTarget is not null);

        c.GotoLabel(ternaryJumpTarget, MoveType.Before);

        c.GotoNext(
            MoveType.After,
            i => i.MatchStloc(out frameIndex)
        );

        c.EmitLdloc(itemTypeIndex);
        c.EmitLdloca(frameIndex);

        c.EmitDelegate(
            static (int type, ref Rectangle frame) =>
            {
                var rectangle = StaticFrame[type];

                if (rectangle is not null)
                {
                    frame = rectangle.Value;
                }
            }
        );
    }

    private static Rectangle GetDrawHitbox_UseStaticFrame(On_Item.orig_GetDrawHitbox orig, int type, Player user)
    {
        var rectangle = StaticFrame[type];

        if (rectangle is null)
        {
            return orig(type, user);
        }

        return rectangle.Value;
    }
#endregion

#region Counts As
    private static void Sort_CountsAs(ILContext il)
    {
        var c = new ILCursor(il);

        int itemAIndex = -1; // loc
        int itemBIndex = -1; // loc

        ILLabel jumpInequalityCheckTarget = c.DefineLabel();

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdloc(out itemAIndex),
            i => i.MatchLdfld<Item>(nameof(Item.type)),
            i => i.MatchLdloc(out itemBIndex),
            i => i.MatchLdfld<Item>(nameof(Item.type)),
            i => i.MatchBneUn(out _)
        );

        c.MarkLabel(jumpInequalityCheckTarget);

        c.GotoPrev(
            MoveType.Before,
            i => i.MatchLdloc(out itemAIndex),
            i => i.MatchLdfld<Item>(nameof(Item.type)),
            i => i.MatchLdloc(out itemBIndex),
            i => i.MatchLdfld<Item>(nameof(Item.type))
        );

        c.EmitLdloc(itemAIndex);
        c.EmitLdloc(itemBIndex);

        c.EmitDelegate(ItemCountsAs);

        c.EmitBrtrue(jumpInequalityCheckTarget);
    }

    private static bool IsTheSameAs_CountsAs(On_Item.orig_IsTheSameAs orig, Item self, Item compareItem)
    {
        if (ItemCountsAs(self, compareItem))
        {
            return true;
        }

        return orig(self, compareItem);
    }

    private static bool ItemCountsAs(Item itemA, Item itemB)
    {
        return CountsAs[itemA.type]?.Contains(itemB.type) is true ||
               CountsAs[itemB.type]?.Contains(itemA.type) is true;
    }
#endregion

#region Swaps To
    private static void ItemCheck_ManageRightClickFeatures_SwapItem(ILContext il)
    {
        var c = new ILCursor(il);

        int playerIndex = -1; // arg

        ILLabel? skipSwapsTarget = null;

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg(out playerIndex),
            i => i.MatchLdfld<Player>(nameof(Player.itemAnimation)),
            i => i.MatchBrtrue(out skipSwapsTarget)
        );

        Debug.Assert(skipSwapsTarget is not null);

        c.EmitLdarg(playerIndex);

        c.EmitDelegate(
            static (Player player) =>
            {
                var item = player.inventory[player.selectedItem];

                var info = SwapsTo[item.type];

                if (info is null)
                {
                    return false;
                }

                var swapInfo = info.Value;

                int stack = item.stack;
                item.ChangeItemType(swapInfo.SwapTo);
                item.stack = Math.Min(stack, item.maxStack);

                SoundEngine.PlaySound(swapInfo.SoundType);

                player.releaseUseTile = false;
                Main.mouseRightRelease = false;

                Recipe.FindRecipes();

                return true;
            }
        );

        c.EmitBrtrue(skipSwapsTarget);
    }

    private static void TryItemSwap_SwapItem(On_ItemSlot.orig_TryItemSwap orig, Item item)
    {
        var info = SwapsTo[item.type];

        if (info is null)
        {
            orig(item);

            return;
        }

        var swapInfo = info.Value;

        // TODO: Not this?
        if (swapInfo.ShiftToSwap && !Main.keyState.IsKeyDown(Keys.LeftShift))
        {
            orig(item);

            return;
        }

        int stack = item.stack;
        item.ChangeItemType(swapInfo.SwapTo);
        item.stack = Math.Min(stack, item.maxStack);

        SoundEngine.PlaySound(swapInfo.SoundType);

        Main.stackSplit = 30;
        Main.mouseRightRelease = false;

        Recipe.FindRecipes();
    }
#endregion

#region Tile Replacements
    private delegate void orig_Step_GrassSeeds(
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
            !self.IsInTileInteractionRange(i, j, new TileReachCheckSettings { TileRangeMultiplier = 1 }) ||
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
#endregion
}
