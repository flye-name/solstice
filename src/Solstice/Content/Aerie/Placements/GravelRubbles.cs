using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Solstice.Content.Aerie;

public abstract class GravelPile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.GravelPile.KEY;
    
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        DustType = DustID.Asphalt;
        Main.tileMerge[ModContent.TileType<AerieGravelTile>()][Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.StyleHorizontal = true;  
        TileObjectData.addTile(Type);
        
        AddMapEntry(new Color(88, 83, 65));
    }
}

public abstract class GravelDebris : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.GravelDebris.KEY;
    
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        DustType = ModContent.DustType<AerieBrickDust>();
        Main.tileMerge[ModContent.TileType<AerieGravelTile>()][Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.StyleHorizontal = true;  
        TileObjectData.addTile(Type);
        
        AddMapEntry(new Color(88, 83, 65));
    }
}

public class GravelPileEcho : GravelPile
{
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<AerieGravel>(), Type, 0, 1);
        
        RegisterItemDrop(ModContent.ItemType<AerieGravel>());
    }
}

public class GravelDebrisEcho : GravelDebris
{
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<AerieGravel>(), Type, 0, 1);
        
        RegisterItemDrop(ModContent.ItemType<AerieGravel>());
    }
}