using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie;

public sealed class AerieBrickEroded : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickEroded.KEY;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickErodedTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe()
           .AddIngredient<AerieBrick>()
           .AddTile(TileID.WorkBenches)
           .Register();

        CreateRecipe()
           .AddIngredient<AerieStone>(2)
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieBrickErodedTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.AerieBrickErodedTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(138, 158, 168));

        DustType = -1;
        HitSound = SoundID.Tink;
    }
}