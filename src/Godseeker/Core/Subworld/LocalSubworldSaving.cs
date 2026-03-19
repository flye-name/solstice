using Daybreak.Common.Features.Hooks;
using Godseeker.Content.Aerie.Environment;
using MonoMod.Cil;
using SubworldLibrary;
using System.IO;
using System.Reflection;
using Terraria.IO;
using Terraria.ModLoader;

namespace Godseeker.Core.Subworld;

#if DEBUG
internal static class LocalSubworldSaving
{
    [OnLoad]
    private static void Load()
    {
        MonoModHooks.Modify(
            typeof(SubworldSystem).GetMethod(
                nameof(SubworldSystem.LoadWorld),
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public
            ),
            LoadWorld_LoadLocalFile
        );
    }

    private static void LoadWorld_LoadLocalFile(ILContext il)
    {
        ILCursor c = new(il);

        int inSubworldIndex = -1; // loc
        int pathIndex = -1; // loc

        if (
            !c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdloc(out inSubworldIndex),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdsfld<SubworldSystem>(nameof(SubworldSystem.main)),
                i => i.MatchCallvirt<FileData>($"get_{nameof(FileData.Path)}"),
                i => i.MatchBr(out _),
                i => i.MatchCall<SubworldSystem>($"get_{nameof(SubworldSystem.CurrentPath)}"),
                i => i.MatchStloc(out pathIndex)
            )
        )
        {
            ModContent.GetInstance<Godseeker>().Logger.Debug("GodSeeker: Could not find Subworld Library's subworld file path variable!");
            return;
        }

        c.EmitLdloca(pathIndex);
        c.EmitLdloc(inSubworldIndex);

        c.EmitDelegate(
            static (ref string path, bool flag) =>
            {
                if (SubworldSystem.current is AerieSubworld && flag)
                {
                    path = Path.Combine(ModContent.GetInstance<Godseeker>().SourceFolder, "Subworld_Save", SubworldSystem.current.FileName + ".wld");
                }
            }
        );
    }
}
#endif
