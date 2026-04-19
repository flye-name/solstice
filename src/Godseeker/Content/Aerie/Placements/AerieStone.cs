using Godseeker.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;

namespace Godseeker.Content.Aerie;

public sealed class AerieStone : ModItem
{
    public override string Texture => PlacementTextures.AerieStone.Key;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieStoneTile>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(2)
           .AddIngredient<AerieBrick>()
           .AddTile(TileID.WorkBenches)
           .Register();

        CreateRecipe(2)
           .AddIngredient<AerieBrickEroded>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}

public class AerieStoneTile : ModTile
{
    public override string Texture => PlacementTextures.AerieStoneTile.Key;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(103, 94, 78));

        DustType = -1;
        HitSound = SoundID.Tink;
    }
}

public sealed class AerieStoneGrassTile : AerieStoneTile
{
    public override string Texture => PlacementTextures.AerieStoneGrassTile.Key;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        RegisterItemDrop(ModContent.ItemType<AerieStone>(), 0);

        TileID.Sets.ChecksForMerge[Type] = true;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        Main.tileMerge[Type][ModContent.TileType<AerieStoneTile>()] = true;
        Main.tileMerge[ModContent.TileType<AerieStoneTile>()][Type] = true;

        GodseekerTileSets.TransformTo[Type] = ModContent.TileType<AerieStoneTile>();

        HitSound = SoundID.Dig;
        DustType = ModContent.DustType<AerieGrassDust>();
    }
}