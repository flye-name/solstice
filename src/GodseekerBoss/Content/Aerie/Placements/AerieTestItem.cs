using GodseekerBoss.Content.Aerie.Environment;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GodseekerBoss.Content.Aerie.Placements;

public sealed class AerieTestItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            if (!SubworldSystem.IsActive<AerieSubworld>())
                SubworldSystem.Enter<AerieSubworld>();
            else
                SubworldSystem.Exit();
        }

        return true;
    }
}
