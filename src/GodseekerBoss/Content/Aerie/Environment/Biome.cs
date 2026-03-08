using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using BackgroundTextures = GodseekerBoss.GeneratedAssets.Assets.Images.Aerie.Backgrounds.Textures;

namespace GodseekerBoss.Content.Aerie.Environment;

public class AerieBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<AerieBackground>();

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/YharonAuricSoulMusic"); // Again, credit is all moonburns, purely a placeholder for now

    public override string MapBackground => BackgroundTextures.Map.Key;

    public override bool IsBiomeActive(Player player)
    {
        return SubworldSystem.IsActive<AerieSubworld>();
    }
}
