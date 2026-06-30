using Microsoft.Xna.Framework;
using Solstice.Core;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Solstice.Content.Aerie;

public class GossipStone : ModItem
{
    public override string Texture => Assets.Images.Aerie.Placements.GossipStone.KEY;
    
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<GossipStoneTile>());
    }
    
    // TODO: recipe
}

public class GossipStoneTile : ModTile
{
    public override string Texture => Assets.Images.Aerie.Placements.GossipStoneTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;
        
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new(ModContent.GetInstance<GossipStoneTileEntity>().Hook_AfterPlacement, -1, 0, true);
        TileObjectData.addTile(Type);
        Main.tileMerge[ModContent.TileType<AerieStoneTile>()][Type] = true;
        Main.tileMerge[ModContent.TileType<AerieStoneGrassTile>()][Type] = true;

        DustType = ModContent.DustType<AerieStoneDust>();
        HitSound = SoundID.Tink;
        
        
        RegisterItemDrop(ModContent.ItemType<GossipStone>());

        AddMapEntry(new Color(103, 94, 78));
    }
    
    public override void KillMultiTile(int i, int j, int frameX, int frameY) 
    {
        ModContent.GetInstance<GossipStoneTileEntity>().Kill(i, j);
    }
}

public class GossipStoneTileEntity : ModTileEntity
{
    public override bool IsTileValidForEntity(int x, int y) 
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<GossipStoneTile>();
    }

    public string GossipMessage = "   ";
    public override void SaveData(TagCompound tag) => tag[nameof(GossipMessage)] = GossipMessage;

    public override void LoadData(TagCompound tag) => GossipMessage = tag.GetString(nameof(GossipMessage));

    public override void NetSend(BinaryWriter writer) => writer.Write(GossipMessage);

    public override void NetReceive(BinaryReader reader) => GossipMessage = reader.ReadString();

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        // TODO: sign UI stuff
        
        TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
        Point16 point16 = TileObjectData.TopLeft(i, j);
        if (Main.netMode != 1)
            return this.Place((int) point16.X, (int) point16.Y);
        NetMessage.SendTileSquare(Main.myPlayer, (int) point16.X, (int) point16.Y, tileData.Width, tileData.Height);
        NetMessage.SendData(87, number: (int) point16.X, number2: (float) point16.Y, number3: (float) this.Type);
        return -1;
    }

    public override void Update()
    {
        var worldPosition = Position.ToVector2() * 16;
        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        if ((Main.GameUpdateCount + Position.X + Position.Y) % 1300 == 0 && worldPosition.Between(-screenSize, screenSize * 2)) 
        {
            Dialogue.NewDialogue(new(GossipMessage, Position.ToVector2() * 16 + new Vector2(16, 22), Color.Gray with { A = 0 }, 0.3f, DialogueType.Wind)
                {
                    LifetimeAfterCompletion = 1400,
                }
            );
            
            SoundEngine.PlaySound(Assets.Sounds.Decorative.GossipStone.Asset with
            {
                MaxInstances = 3,
                PitchVariance = 0.2f,
                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
            }, worldPosition);
        }
    }
}