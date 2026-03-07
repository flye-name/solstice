using GodseekerBoss.Core.Subworlds;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Skies;
using Terraria.ModLoader;

namespace GodseekerBoss
{
	public class GodseekerBoss : Mod
	{
		public override void Load()
		{
			On_AmbientSky.FadingSkyEntity.Update += DisableAllSkyObjectsInAerie;
			On_AmbientSky.FadingSkyEntity.UpdateOpacity += UpdateObjectOpacityInAerie;

			On_Main.DrawSunAndMoon += PreventSunRenderingInAerie;
		}

		private void PreventSunRenderingInAerie(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Microsoft.Xna.Framework.Color moonColor, Microsoft.Xna.Framework.Color sunColor, float tempMushroomInfluence)
		{
			if (!SubworldSystem.IsActive<DragonAerieSubworld>())
			{
				orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
			}
		}

		//Probs useless since objects should spawn with 0 opacity anyways, but still here to ensure that if objects spawn anyways, they are faded out and not instantly disabled
		private void UpdateObjectOpacityInAerie(On_AmbientSky.FadingSkyEntity.orig_UpdateOpacity orig, object self, int frameCount)
		{
			if (SubworldSystem.IsActive<DragonAerieSubworld>())
			{
				if (((AmbientSky.FadingSkyEntity)self).Opacity > 0)
				{
					((AmbientSky.FadingSkyEntity)self).Opacity--;
				}
				else
				{
					orig(self, frameCount);
				}
			}
			else
			{
				orig(self, frameCount);
			}
		}

		private void DisableAllSkyObjectsInAerie(On_AmbientSky.FadingSkyEntity.orig_Update orig, object self, int frameCount)
		{
			if (SubworldSystem.IsActive<DragonAerieSubworld>())
			{
				if (((AmbientSky.FadingSkyEntity)self).Opacity <= 0)
				{
					((AmbientSky.FadingSkyEntity)self).IsActive = false;
				}
			}
			else
			{
				orig(self, frameCount);
			}
		}
	}
}
