using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class WorldSpawnSetter : ModItem
{
    public override string Texture => Assets.Images.TestItem.KEY;

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            Main.spawnTileX = Main.MouseWorld.ToTileCoordinates().X; 
            Main.spawnTileY = Main.MouseWorld.ToTileCoordinates().Y;
        }

        return true;
    }
}
