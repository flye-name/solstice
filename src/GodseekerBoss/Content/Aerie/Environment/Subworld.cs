using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace GodseekerBoss.Content.Aerie.Environment;

public class AerieSubworld : Subworld
{
    public static bool Active => SubworldSystem.IsActive<AerieSubworld>();

    public override int Width => 1000;

    public override int Height => 400;

    public override List<GenPass> Tasks => new List<GenPass>() 
    {
        new PassLegacy("Aerie Settings", new WorldGenLegacyMethod(AerieSubworldSettings))
    };

    private static void AerieSubworldSettings(GenerationProgress progres, GameConfiguration configurations)
    {
        Main.worldSurface = Main.maxTilesY;
        Main.rockLayer = Main.maxTilesY + 42;
    }

    public override void SetStaticDefaults()
    {
            
    }

    public override void OnLoad()
    {
        SubworldSystem.hideUnderworld = true;
    }

    public override void Update()
    {
        Main.cloudAlpha = 0f;
        Main.raining = false;
        Main.cloudBGActive = 0f;
        for (int i = 0; i < Main.cloud.Length; i++)
        {
            Main.cloud[i].active = false;
        }
        Main.time = 27000;
        Main.dayTime = true;
        Main.eclipse = false;
        Main.windSpeedTarget = -1f;
        Main.windSpeedCurrent = -1f;
    }

    // public override bool ShouldSave => true;
}
