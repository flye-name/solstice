using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Utilities;

namespace Solstice.Content.Aerie.Weather;

public struct RedSprite
{
    public readonly int Seed;
    public UnifiedRandom Random => new(Seed);
    
    public readonly Vector2 Position;
    public readonly List<Vector2> Points;
    public readonly float MaxLifetime;
    public float Lifetime;

    public bool Active;

    public RedSprite(Vector2 position, float lifetime)
    {
        Seed = Main.rand.Next(int.MaxValue);

        Position = position;
        Points = new();
        
        MaxLifetime = lifetime;
        Lifetime = lifetime;

        Active = true;
    }
}

