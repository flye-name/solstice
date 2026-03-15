using Daybreak.Common.Features.Hooks;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace GodseekerBoss.Core.Assets;

#if DEBUG
// TODO: Prevent image premultiplication.
[Autoload(Side = ModSide.Client)]
internal static class AssetReloader
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

    private static FileSystemWatcher? assetWatcher;

    private static string modSource = string.Empty;

    private static LocalAssetSource? assetSource;

    private static readonly Mod mod = ModContent.GetInstance<GodseekerBoss>();

    [OnLoad]
    private static void Load()
    {
        try
        {
            modSource = mod.SourceFolder.Replace('\\', '/');

            if (!Directory.Exists(modSource))
            {
                throw new DirectoryNotFoundException("Mod source directory does not exist!");
            }

            assetSource = new LocalAssetSource(modSource);

            ChangeContentSource();

            AssetReaderCollection assetReaderCollection = Main.instance.Services.Get<AssetReaderCollection>();

            string[] extensions = assetReaderCollection.GetSupportedExtensions();

            assetWatcher = new FileSystemWatcher(modSource);

            foreach (string e in extensions)
            {
                assetWatcher.Filters.Add($"*{e}");
            }

            assetWatcher.Changed += AssetChanged;

            assetWatcher.NotifyFilter = all_filters;

            assetWatcher.IncludeSubdirectories = true;
            assetWatcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            mod.Logger.Warn(
                $"Unable to load Asset Reloader:\n{e.GetType().Name!}\n{e.Message}" +
                $"\nThis warning should not be present for mod consumers!");
        }
    }

    [OnUnload]
    private static void Unload()
    {
        assetWatcher?.EnableRaisingEvents = false;
        assetWatcher?.Dispose();
    }

    private static void AssetChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType.HasFlag(WatcherChangeTypes.Created))
        {
            return;
        }

        string assetPath = Path.GetRelativePath(modSource, e.FullPath).Replace('/', '\\');

        assetSource?.AddAssetPath(assetPath);

        assetPath = Path.ChangeExtension(assetPath, null);

        if (e.ChangeType.HasFlag(WatcherChangeTypes.Deleted) ||
            e.ChangeType.HasFlag(WatcherChangeTypes.Renamed))
        {
            string message = $"Unable to reload asset at \"{assetPath},\" asset was deleted or renamed!";

            mod.Logger.Warn(message);

            if (!Main.gameMenu)
            {
                Main.NewText(message);
            }

            return;
        }

        var repositoryAssets = mod.Assets._assets;

        Debug.Assert(repositoryAssets is not null);

        if (!repositoryAssets.TryGetValue(assetPath, out IAsset? asset) ||
            asset is null)
        {
            return;
        }

        Main.QueueMainThreadAction(() => ReloadAsset(asset));
    }

    private static void ReloadAsset(IAsset asset)
    {
        try
        {
            lock (mod.Assets._requestLock)
            {
                mod.Assets.ForceReloadAsset(asset, AssetRequestMode.ImmediateLoad);
            }

            InvokeAssetWait(asset);
        }
        catch (Exception e)
        {
            string message = $"Unable to reload asset \"{asset.Name}\":\n{e.GetType().Name!}\n{e.Message}";

            mod.Logger.Warn(message);
        }
    }

    private static void InvokeAssetWait(IAsset asset)
    {
        Type type = asset.GetType();

        if (!type.IsGenericType ||
            type.GetGenericTypeDefinition() != typeof(Asset<>))
        {
            throw new ArgumentException($"Asset was of incorrect type!");
        }

        MethodInfo? getAssetWait = type.GetProperty(nameof(Asset<>.Wait), BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();

        var wait = (Action?)getAssetWait?.Invoke(asset, []);

        wait?.Invoke();
    }

    private static void ChangeContentSource()
    {
        Main.QueueMainThreadAction(
            () =>
            {
                mod.Assets.SetSources([assetSource, mod.RootContentSource]);
            }
        );
    }

    internal sealed class LocalAssetSource : ContentSource
    {
        private readonly string modSource;

        public string[] AssetPaths
        {
            get => assetPaths;
            set => SetAssetNames(value);
        }

        public LocalAssetSource(string modSource)
        {
            this.modSource = modSource;

            assetPaths = [];
        }

        // Should not breakpoint the debugger.
        [DebuggerNonUserCode]
        public override Stream OpenStream(string fullAssetName)
        {
            return File.OpenRead(Path.Combine(modSource, fullAssetName));
        }

        public void AddAssetPath(string path)
        {
            if (AssetPaths.Contains(path))
            {
                return;
            }

            AssetPaths = AssetPaths.Append(path).ToArray();
        }
    }
}
#endif