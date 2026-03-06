using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace GodseekerBoss.Core.Subworlds
{
	public class DragonAerieSubworld : Subworld
	{
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
		}

		//public override bool ShouldSave => true;
	}
}
