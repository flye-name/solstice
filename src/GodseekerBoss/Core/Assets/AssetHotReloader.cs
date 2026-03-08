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

namespace ZenSkies.Core.Assets;

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

    private readonly static Mod mod = ModContent.GetInstance<ModImpl>();

    [OnLoad]
    private static void Load()
    {
        try
        {
            modSource = mod.SourceFolder.Replace('\\', '/');

            if (!Directory.Exists(modSource))
            {
                throw new DirectoryNotFoundException("Mod source directory does not exsist!");
            }

            assetSource = new LocalAssetSource(modSource);

            ChangeContentSource();

            AssetReaderCollection assetReaderCollection = Main.instance.Services.Get<AssetReaderCollection>();

            string[] extensions = assetReaderCollection.GetSupportedExtensions();

            assetWatcher = new(modSource);

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
            mod.Logger.Warn($"Unable to load Asset Reloader! - {e}");
        }
    }

    [OnUnload]
    private static void Unload()
    {
        if (assetWatcher is not null)
        {
            assetWatcher.EnableRaisingEvents = false;
            assetWatcher.Dispose();
        }
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
            mod.Logger.Warn($"Asset at {assetPath} was removed or renamed!");

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
        lock (mod.Assets._requestLock)
        {
            mod.Assets.ForceReloadAsset(asset, AssetRequestMode.ImmediateLoad);
        }

        InvokeAssetWait(asset);
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

        public LocalAssetSource(string modSource) : base()
        {
            this.modSource = modSource;

            assetPaths = [];
        }

        public override Stream OpenStream(string fullAssetName)
        {
            return File.OpenRead(Path.Combine(modSource, fullAssetName));
        }

        public void AddAssetPath(string path)
        {
            AssetPaths = AssetPaths.Append(path).ToArray();
        }
    }
}

#endif