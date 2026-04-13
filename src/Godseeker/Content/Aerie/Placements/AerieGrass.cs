using Daybreak.Common.Features.Hooks;
using Godseeker.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using PlacementTextures = Godseeker.GeneratedAssets.Assets.Images.Aerie.Placements.Textures;
// ReSharper disable InconsistentNaming

namespace Godseeker.Content.Aerie;

public sealed class AerieGrassDust : ModDust
{
    public override string Texture => PlacementTextures.AerieGrassDust.Key;

    public override void OnSpawn(Dust dust)
    {
        dust.frame = new Rectangle(0, Main.rand.Next(3) * 12, 12, 12);
    }

    public override bool Update(Dust dust)
    {
        dust.velocity.X += Main.WindForVisuals * 0.043f;
        dust.velocity.Y += 0.065f;

        dust.position += dust.velocity;

        dust.rotation += dust.velocity.X * 0.1f;

        dust.scale *= 0.98f;

        if (dust.scale < 0.05f)
        {
            dust.active = false;
        }

        return false;
    }
}

public class AerieGrassSeeds : ModItem
{
    public override string Texture => PlacementTextures.AerieGrassSeeds.Key;

    private static readonly Dictionary<int, int> grass_replacements = new()
    {
        {ModContent.TileType<AerieBrickTile>(), ModContent.TileType<AerieBrickGrassTile>()},
    };

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;

        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.DirtBlock, 1);

        GodseekerItemSets.TileReplacements[Type] = new GodseekerItemSets.TileReplacementInfo(true, grass_replacements);
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerieBrickGrassTile>());
        Item.useTime = Item.useAnimation;
    }
}

#region Tall Grass
public sealed class TallAerieGrassSeedsDummy : ModItem
{
    public override string Texture => PlacementTextures.TallAerieGrassSeedsDummy.Key;

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(0);
        Item.value = Item.sellPrice(silver: 5);
        // Vanilla flower seeds don't have auto-reuse.
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation;
    }

    public override void OnCreated(ItemCreationContext context)
    {
        if (context is not
            (JourneyDuplicationItemCreationContext or
            BuyItemCreationContext or
            RecipeItemCreationContext))
        {
            return;
        }

        int stack = Item.stack;
        Item.SetDefaults(TallAerieGrassHelper.AlternateSeedItems[0].Type);
        Item.stack = Math.Min(stack, Item.maxStack);
    }
}

[Autoload(false)]
public sealed class TallAerieGrassSeeds<T> : ModItem where T : ModTile
{
    public override string Texture => PlacementTextures.TallAerieGrassSeeds.Key;

    public override string Name => "TallAerieGrassSeeds" + typeof(T).Name + PlaceStyle;

    protected override bool CloneNewInstances => true;

    private int PlaceStyle { get; init; }

    private Rectangle ItemFrame { get; init; }

    private Rectangle BarFrame { get; init; }

    public TallAerieGrassSeeds(int placeStyle, int frameX, int frameY)
    {
        PlaceStyle = placeStyle;

        if (Main.dedServ)
        {
            return;
        }

        // Ensure textures are loaded.
        PlacementTextures.TallAerieGrassSeeds.Wait();
        PlacementTextures.TallAerieGrassSeedsBar.Wait();

        ItemFrame = PlacementTextures.TallAerieGrassSeeds.Value.Frame(2, 2, frameX, frameY);
        BarFrame = PlacementTextures.TallAerieGrassSeedsBar.Value.Frame(2, 2, frameX, frameY);
    }

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;

        int parent = ModContent.ItemType<TallAerieGrassSeedsDummy>();

        ContentSamples.CreativeHelper.ShouldRemoveFromList(Item);
        ContentSamples.CreativeResearchItemPersistentIdOverride[Type] = parent;

        int next = Array.FindIndex(TallAerieGrassHelper.AlternateSeedItems, i => i.Type == Type) + 1;
        next %= TallAerieGrassHelper.AlternateSeedItems.Length;

        next = TallAerieGrassHelper.AlternateSeedItems[next].Type;

        GodseekerItemSets.SwapsTo[Type] = new GodseekerItemSets.ItemSwapInfo(next, true, SoundID.Grass);

        int[] similar = TallAerieGrassHelper.AlternateSeedItems.Select(i => i.Type).Append(parent).ToArray();

        GodseekerItemSets.CountsAs[Type] = similar;

        GodseekerItemSets.StaticFrame[Type] = ItemFrame;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<T>(), PlaceStyle);
        Item.value = Item.sellPrice(silver: 5);
        // Vanilla flower seeds don't have auto-reuse.
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation;
    }

    public override void PostDrawInInventory(
        SpriteBatch spriteBatch,
        Vector2 position,
        Rectangle frame,
        Color drawColor,
        Color itemColor,
        Vector2 origin,
        float scale
    )
    {
        var texture = PlacementTextures.TallAerieGrassSeedsBar.Value;

        position += new Vector2(15f, 10f) * Main.inventoryScale;// * scale;

        spriteBatch.Draw(texture, position, BarFrame, drawColor, 0f, BarFrame.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
    }
}

public sealed class TallAerieGrass1x1 : ModTile
{
    public override string Texture => PlacementTextures.TallAerieGrassTile.Key;

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(0, 0, 1);

        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 7;
        TileObjectData.newTile.StyleMultiplier = 7;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.RandomStyleRange = 3;
        TileObjectData.newAlternate.StyleMultiplier = 3;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.DrawFlipMode[Type] = 1;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        GodseekerTileSets.UseAlternateTileObjectDataRandomStyles[Type] = true;

        AddMapEntry(new Color(185, 168, 72));
        DustType = ModContent.DustType<AerieGrassDust>();
        HitSound = SoundID.Grass;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        if (!TallAerieGrassHelper.CanPlaceAerieGrass(i, j))
        {
            WorldGen.KillTile(i, j);
        }
        return false;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        // Vanilla flower seeds change random style after each placement.
        TileObjectPreviewData.randomCache.Reset();

        TileObject.CanPlace(i, j, Type, item.placeStyle, Main.LocalPlayer.direction, out _, onlyCheck: true);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override bool CanPlace(int i, int j)
    {
        return TallAerieGrassHelper.CanPlaceAerieGrass(i, j);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }
}

public sealed class TallAerieGrass1x2 : ModTile
{
    public override string Texture => PlacementTextures.TallerAerieGrassTile.Key;

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(0, 0, 1);

        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.Origin = new(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 4;
        TileObjectData.newTile.StyleMultiplier = 4;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new(0, 1);
        TileObjectData.newAlternate.RandomStyleRange = 4;
        TileObjectData.newAlternate.StyleMultiplier = 4;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.DrawFlipMode[Type] = 1;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        GodseekerTileSets.UseAlternateTileObjectDataRandomStyles[Type] = true;

        AddMapEntry(new Color(185, 168, 72));
        DustType = ModContent.DustType<AerieGrassDust>();
        HitSound = SoundID.Grass;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        // Vanilla flower seeds change random style after each placement.
        TileObjectPreviewData.randomCache.Reset();

        TileObject.CanPlace(i, j, Type, item.placeStyle, Main.LocalPlayer.direction, out _, onlyCheck: true);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override bool CanPlace(int i, int j)
    {
        return TallAerieGrassHelper.CanPlaceAerieGrass(i, j);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 2;
    }
}

public static class TallAerieGrassHelper
{
    public static readonly ModItem[] AlternateSeedItems =
    [
        // 1x1
        new TallAerieGrassSeeds<TallAerieGrass1x1>(placeStyle: 0, frameX: 0, frameY: 0),
        new TallAerieGrassSeeds<TallAerieGrass1x1>(placeStyle: 1, frameX: 0, frameY: 1),
        // 1x2
        new TallAerieGrassSeeds<TallAerieGrass1x2>(placeStyle: 0, frameX: 1, frameY: 0),
        new TallAerieGrassSeeds<TallAerieGrass1x2>(placeStyle: 1, frameX: 1, frameY: 1),
    ];

    [OnLoad]
    private static void Load(Mod mod)
    {
        foreach (var item in AlternateSeedItems)
        {
            mod.AddContent(item);
        }
    }

    public static readonly int[] ValidGrasses =
    [
        ModContent.TileType<AerieCloudTile>(),
        ModContent.TileType<AerieStoneTile>(),
        ModContent.TileType<AerieBrickTile>(),
        ModContent.TileType<AerieBrickGrassTile>(),
        ModContent.TileType<AerieCeramicTile>(),
        TileID.Grass,
        TileID.Dirt,
    ];

    // TODO: Edit and use WorldGen.PlantCheck.
    public static bool CanPlaceAerieGrass(int i, int j)
    {
        if (j < 1 || j > Main.maxTilesY - 1)
        {
            return false;
        }

        Tile tile = Framing.GetTileSafely(i, j + 1);

        if (!tile.HasTile || tile.Slope != SlopeType.Solid || tile.IsHalfBlock)
        {
            return false;
        }

        return ValidGrasses.Contains(tile.TileType);
    }
}
#endregion
