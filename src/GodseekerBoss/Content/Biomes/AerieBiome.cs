using GodseekerBoss.Content.Biomes.Backgrounds;
using GodseekerBoss.Core.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace GodseekerBoss.Content.Biomes;

public class AerieBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<AerieBackground>();

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/YharonAuricSoulMusic"); //Again, credit is all moonburns, purely a placeholder for now

    public override string MapBackground => "GodseekerBoss/Assets/Images/MapBG33";

    public override bool IsBiomeActive(Player player)
    {
        return SubworldSystem.IsActive<DragonAerieSubworld>();
    }
}
