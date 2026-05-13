using Godseeker.Content.Aerie.Placements;
using Godseeker.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Godseeker.Content.Aerie;
public static class AerieLeafCoatingDrawing
{
    private static (Texture2D texture, Color color) getPaintData(int i, int j, bool wall, Texture2D texture, Color color)
    {
        Tile tile = Main.tile[i, j];
        if (wall ? tile.IsWallFullbright : tile.IsTileFullbright)
            color = Color.White;
        
        int paint = wall ? tile.WallColor : tile.TileColor;
        if (paint > PaintID.None)
        {
            Texture2D newTexture = Main.instance.TilePaintSystem.TryGetWallAndRequestIfNotReady(ModContent.WallType<AerieGrassWallLeavesFake>(), paint);
            if (newTexture == null)
                color = color.MultiplyRGBA(WorldGen.paintColor(paint));
            else
                texture = newTexture;
        }

        return (texture, color);
    }
    
    public static void DrawLeafOverlay(int i, int j, bool wall)
    {
        Tile tile = Main.tile[i, j];
        
        Texture2D texture = Assets.Images.Aerie.Placements.AerieGrassWallTile_Leaves.Asset.Value;
        Color color = Lighting.GetColor(i, j);

        SolsticeTileData data = tile.Get<SolsticeTileData>();
        bool affectedByPaint = wall ? !data.LeafCoatingUnaffectedByPaint_Wall : !data.LeafCoatingUnaffectedByPaint_Tile;
        if (affectedByPaint)
            (texture, color) = getPaintData(i, j, wall, texture, color);

        if (!affectedByPaint || wall ? !tile.IsWallInvisible : !tile.IsTileInvisible)
            internalDrawLeafOverlay(i, j, texture, color, wall);
    }

    private static void internalDrawLeafOverlay(int i, int j, Texture2D texture, Color color, bool wall)
    {   
        var offset = i * j;
        
        float sway = MathF.Sin(Main.GameUpdateCount / 20f + offset) * Main.windSpeedCurrent * 2f;

        int frameCount = 4;
        int frameWidth = 24;
        int frameHeight = texture.Height / frameCount;
        Rectangle rect = new(wall ? 0 : frameWidth, new UnifiedRandom(offset).Next(frameCount) * frameHeight, frameWidth, frameHeight);

        Main.spriteBatch.Draw(texture, new Vector2(i, j).ToWorldCoordinates() - Main.screenPosition + new Vector2(Main.offScreenRange + sway), rect, color, offset + sway * 0.05f, rect.Size() / 2f, 1, SpriteEffects.None, 0);
    }
}

public class AerieLeafCoatingTile : GlobalTile
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (tile.HasTile && tile.Get<SolsticeTileData>().LeafCoatingActive_Tile) 
            AerieLeafCoatingDrawing.DrawLeafOverlay(i, j, false);
    }

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            Tile tile = Main.tile[i, j];
            tile.Get<SolsticeTileData>().LeafCoatingActive_Tile = false;
            tile.Get<SolsticeTileData>().LeafCoatingUnaffectedByPaint_Tile = false;
            
            // TODO: Visuals
        }
    }
}

public class AerieLeafCoatingWall : GlobalWall
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (type > WallID.None && tile.Get<SolsticeTileData>().LeafCoatingActive_Wall) 
            AerieLeafCoatingDrawing.DrawLeafOverlay(i, j, true);
    }

    public override void KillWall(int i, int j, int type, ref bool fail)
    {
        if (!fail)
        {
            Tile tile = Main.tile[i, j];
            tile.Get<SolsticeTileData>().LeafCoatingActive_Wall = false;
            tile.Get<SolsticeTileData>().LeafCoatingUnaffectedByPaint_Wall = false;
            
            // TODO: Visuals
        }
    }
}


// test items
public class AerieLeafCoating : ModItem
{
    public override string Texture => Assets.Images.AerieFluteHeld.KEY;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Paintbrush);
    }

    public override bool? UseItem(Player player)
    {
        if (!Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].HasTile && Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].WallType == 0)
            return base.UseItem(player);
        
        ref var tileData = ref Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].Get<SolsticeTileData>();
        tileData.LeafCoatingActive_Tile = !tileData.LeafCoatingActive_Tile;
        tileData.LeafCoatingActive_Wall = !tileData.LeafCoatingActive_Wall;
        return true;
    }
}

public class AerieLeafCoatingPaintSeparator : ModItem
{
    public override string Texture => Assets.Images.AerieFlute.KEY;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.Paintbrush);
    }

    public override bool? UseItem(Player player)
    {
        if (!Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].HasTile && Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].WallType == 0)
            return base.UseItem(player);
        
        ref var tileData = ref Main.tile[Main.MouseWorld.ToTileCoordinates().X, Main.MouseWorld.ToTileCoordinates().Y].Get<SolsticeTileData>();
        tileData.LeafCoatingUnaffectedByPaint_Tile = !tileData.LeafCoatingUnaffectedByPaint_Tile;
        tileData.LeafCoatingUnaffectedByPaint_Wall = !tileData.LeafCoatingUnaffectedByPaint_Wall;
        return true;
    }
}