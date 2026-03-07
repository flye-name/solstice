using GodseekerBoss.Core.Subworlds;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace GodseekerBoss.Core.GlobalClasses
{
	public class NPCSpawnsEditor : GlobalNPC
	{
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (SubworldSystem.IsActive<DragonAerieSubworld>())
			{
				foreach (int index in pool.Keys)
				{
					pool[index] = 0f;
				}
			}
		}
	}
}
