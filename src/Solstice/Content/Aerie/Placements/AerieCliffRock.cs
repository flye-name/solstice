using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public sealed class AerieCliffRock : ModItem
{
    public override string Texture => "Solstice/Assets/Images/Aerie/Placements/AerieCliffRock";

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieCliffRockTile>());
    }
}

public class AerieCliffRockTile : ModTile
{
    public override string Texture => "Solstice/Assets/Images/Aerie/Placements/AerieCliffRockTile";

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;

        AddMapEntry(new Color(103, 94, 78));

        DustType = ModContent.DustType<AerieStoneDust>();
        HitSound = SoundID.Tink;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        //offset every other X pos
        Tile tile = Main.tile[i, j];
        int frameX = tile.TileFrameX / 18;
        int frameY = tile.TileFrameY / 18;
        int numX = 0;
        int numY = 0;
        bool isLeft = i % 2 == 0;
        if ((frameX == 0 || frameX == 4 || frameX == 5 || frameX == 9 || frameX == 10 || frameX == 11 || frameX == 12) && frameY <= 2)
            numY = isLeft ? 0 : 1;
        if (frameX <= 3 && frameX >= 1 && frameY <= 3)
            numX = isLeft ? 1 : 2;
        if (frameX <= 8 && frameX >= 6 && frameY <= 4)
            numX = isLeft ? 6 : 7;
        if (frameX <= 11 && frameX >= 9 && frameY == 4)
            numX = isLeft ? 9 : 10;

        if ((frameX == 0 || frameX == 2 || frameX == 4) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 0 : 2;
        if ((frameX == 1 || frameX == 3 || frameX == 5) && frameY >= 3 && frameY <= 4)
            numX = isLeft ? 1 : 3;

        if (numX != 0)
            tileFrameX = (short)(numX * 18);
        if (numY != 0)
            tileFrameY = (short)(numY * 18);

        //offset every other Y pos
        if (j % 2 == 1)
            tileFrameY += 90;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 4;
    }
}