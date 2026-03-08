using Daybreak.Common.Features.Hooks;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ZenSkies.Core.Assets;

#if DEBUG

[Autoload(Side = ModSide.Client)]
public static class ShaderHotCompiler
{
    private const NotifyFilters all_filters =
        NotifyFilters.FileName |
        NotifyFilters.DirectoryName |
        NotifyFilters.Attributes |
        NotifyFilters.Size |
        NotifyFilters.LastWrite |
        NotifyFilters.LastAccess |
        NotifyFilters.CreationTime |
        NotifyFilters.Security;

    private static readonly string[] effect_extensions = [".fx", ".hlsl"];

    private static string fxcPath = "";

    private static FileSystemWatcher? effectWatcher;

    private static string modSource = "";

    private readonly static Mod mod = ModContent.GetInstance<ModImpl>();

    [OnLoad]
    private static void Load()
    {
        try
        {
            modSource = mod.SourceFolder.Replace('\\', '/');

            string[] paths = Directory.GetFiles(modSource, "*fxc.exe", SearchOption.AllDirectories);

            if (paths.Length <= 0)
            {
                mod.Logger.Info("'fxc.exe' not found! Effects will not be compiled!");
                return;
            }

            fxcPath = paths[0].Replace('\\', '/');

            effectWatcher = new(modSource);

            foreach (string e in effect_extensions)
                effectWatcher.Filters.Add($"*{e}");

            effectWatcher.Changed += EffectChanged;

            effectWatcher.NotifyFilter = all_filters;

            effectWatcher.IncludeSubdirectories = true;
            effectWatcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            mod.Logger.Warn($"Unable to load Shader Hot-Compiler! - {e}");
        }
    }

    [OnUnload]
    private static void Unload()
    {
        effectWatcher?.Dispose();
    }

    private static void EffectChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType.HasFlag(WatcherChangeTypes.Created))
            return;

        string effectPath = e.FullPath.Replace('\\', '/');

        string shortPath = Path.GetRelativePath(modSource, effectPath);

        shortPath = Path.ChangeExtension(shortPath, null).Replace('\\', '/');

        if (e.ChangeType.HasFlag(WatcherChangeTypes.Deleted) ||
            e.ChangeType.HasFlag(WatcherChangeTypes.Renamed))
        {
            mod.Logger.Warn($"Effect at {shortPath} was removed or renamed!");
            return;
        }

        Task.Run(
            async () =>
            {
                await CompileShaderTask(fxcPath, effectPath, shortPath);
            }
        );
    }

    private static async Task CompileShaderTask(string executable, string effectPath, string shortPath)
    {
        // Prevent alledged issues with temp files.
        await Task.Delay(10);

        string wineArgument = "";

        string outputEffect = Path.ChangeExtension(effectPath, ".fxc");

        if (OperatingSystem.IsLinux())
        {
            HandleWineCompilation(ref executable, ref wineArgument, ref effectPath, ref outputEffect);
        }

        ProcessStartInfo pInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = executable,
            Arguments = $"{wineArgument} \"{effectPath}\" /T fx_2_0 /nologo /O2 /Fo \"{outputEffect}\"",
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = new();

        process.StartInfo = pInfo;

        process.ErrorDataReceived +=
            (_, e) =>
            {
                LogShaderCompilationError(e.Data ?? string.Empty, effectPath, shortPath);
            };

        process.Start();

        process.BeginErrorReadLine();

        process.WaitForExit();

        if (process.ExitCode == 0)
            return;

        mod.Logger.Warn($"Effect at {shortPath} could not be compiled! Exit code: {process.ExitCode}");
    }

    private static void HandleWineCompilation(ref string executable, ref string wineArgument, ref string effectPath, ref string outputEffect)
    {
        ProcessStartInfo pInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "/bin/bash",
            Arguments = "-c \"command -v wine\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = new();

        process.StartInfo = pInfo;
        process.Start();

        process.WaitForExit();

        string error = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();

        Debug.Assert(!string.IsNullOrEmpty(error));
        Debug.Assert(!string.IsNullOrEmpty(output));

        wineArgument = executable;
        executable = output.Trim();

        WinePathConversion(ref effectPath);
        WinePathConversion(ref outputEffect);
    }

    private static void WinePathConversion(ref string path)
    {
        ProcessStartInfo pInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "/bin/bash",
            Arguments = $"-c \"winepath --windows '{path}'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = new();

        process.StartInfo = pInfo;
        process.Start();

        process.WaitForExit();

        string error = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();

        if (string.IsNullOrEmpty(output))
        {
            mod.Logger.Warn($"Error converting path \"{path}\" using WINE; {error}");
        }

        path = output.Trim();
    }

    private static void LogShaderCompilationError(string error, string effectPath, string shortPath)
    {
        if (error.Length <= 0)
        {
            return;
        }

        error = error.Replace(effectPath, string.Empty);

        if (!error.Contains("error"))
        {
            return;
        }

        mod.Logger.Warn($"{shortPath}: {error}");
    }
}

#endif
