using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace Solstice.Core;

public struct SolsticeTileData : ITileData
{
    public byte PackedData;

    public bool LeafCoatingActive_Tile
    {
        get => TileDataPacking.GetBit(PackedData, 0);
        set => PackedData = (byte)TileDataPacking.SetBit(value, PackedData, 0);
    }
    
    public bool LeafCoatingActive_Wall
    {
        get => TileDataPacking.GetBit(PackedData, 1);
        set => PackedData = (byte)TileDataPacking.SetBit(value, PackedData, 1);
    }

    public bool LeafCoatingUnaffectedByPaint_Tile
    {
        get => TileDataPacking.GetBit(PackedData, 2);
        set => PackedData = (byte)TileDataPacking.SetBit(value, PackedData, 2);
    }
    
    public bool LeafCoatingUnaffectedByPaint_Wall
    {
        get => TileDataPacking.GetBit(PackedData, 3);
        set => PackedData = (byte)TileDataPacking.SetBit(value, PackedData, 3);
    }
}

public class TileDataSaving : ModSystem
{
    private const string KEY = "Solstice:TileData";
    
    public override void SaveWorldData(TagCompound tag)
    {
        ReadOnlySpan<SolsticeTileData> span = Main.tile.GetData<SolsticeTileData>();
        tag.Add(KEY, MemoryMarshal.AsBytes(span).ToArray());
    }

    public override void LoadWorldData(TagCompound tag)
    {
        SolsticeTileData[] tileData = Main.tile.GetData<SolsticeTileData>();
        byte[] bytes = tag.GetByteArray(KEY);

        if (bytes.Length != tileData.Length * Marshal.SizeOf<SolsticeTileData>())
            return;
        
        bytes.CopyTo(MemoryMarshal.AsBytes(tileData.AsSpan()));
    }
}