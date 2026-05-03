using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie;

public class AerieBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<AerieBackground>();

    public override int Music => Assets.Music.HeavenAmbience.Slot;

    public override string MapBackground => Assets.Images.Aerie.Backgrounds.Map.KEY;

    public override bool IsBiomeActive(Player player)
    {
        return SubworldSystem.IsActive<AerieSubworld>();
    }
}
