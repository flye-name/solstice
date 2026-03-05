using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.WorldBuilding;

namespace GodseekerBoss.Core.Subworlds
{
	public class DragonAerieSubworld : Subworld
	{
		public override int Width => 1000;

		public override int Height => 400;

		public override List<GenPass> Tasks => new List<GenPass>();

		public override void SetStaticDefaults()
		{
			
		}
	}
}
