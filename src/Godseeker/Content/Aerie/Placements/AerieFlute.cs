using Godseeker.Content.Aerie.Environment;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie.Placements;

public sealed class AerieFlute : ModItem
{
    public override string Texture => GeneratedAssets.Assets.Images.Textures.AerieFlute.Key;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 36;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noUseGraphic = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return true;
        }

        SubworldLoading.EnterAerie();

        return true;
    }
}