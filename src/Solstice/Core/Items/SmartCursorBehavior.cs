using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;

namespace Solstice.Core;

public interface ISmartCursorBehavior
{
    [OnLoad]
    private static void Load()
    {
        IL_SmartCursorHelper.SmartCursorLookup += SmartCursorLookup_CustomBehavior;
    }

    private static void SmartCursorLookup_CustomBehavior(ILContext il)
    {
        var c = new ILCursor(il);

        int infoIndex = -1;    // loc
        int xTargetIndex = -1; // loc
        int yTargetIndex = -1; // loc

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdloc(out infoIndex),
            i => i.MatchLdloca(out xTargetIndex),
            i => i.MatchLdloca(out yTargetIndex),
            i => i.MatchCall<SmartCursorHelper>(nameof(SmartCursorHelper.Step_StaffOfRegrowth))
        );

        c.EmitLdloc(infoIndex);
        c.EmitLdloca(xTargetIndex);
        c.EmitLdloca(yTargetIndex);

        c.EmitDelegate(
            static (SmartCursorHelper.SmartCursorUsageInfo info, ref int xTarget, ref int yTarget) =>
            {
                if (info.item.ModItem is not ISmartCursorBehavior behavior)
                {
                    return;
                }

                (xTarget, yTarget) = behavior.FindSmartCursorTarget(info);
            }
        );
    }

    Point FindSmartCursorTarget(SmartCursorHelper.SmartCursorUsageInfo info);
}
