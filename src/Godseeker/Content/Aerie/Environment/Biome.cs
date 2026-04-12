using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using BackgroundTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;

namespace Godseeker.Content.Aerie;

public class AerieBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<AerieBackground>();

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/HeavenAmbience"); 

    public override string MapBackground => BackgroundTextures.Map.Key;

    public override bool IsBiomeActive(Player player)
    {
        return SubworldSystem.IsActive<AerieSubworld>();
    }
}
