using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Utilities;

namespace Solstice.Content.Aerie.Weather;

public struct RedSprite
{
    public const int MaxBranches = 14;
    
    public readonly int Seed;
    public UnifiedRandom Random => new(Seed);
    
    public readonly Vector2 Position;
    public readonly List<Vector2>[] Points = new List<Vector2>[MaxBranches];
    public readonly float MaxLifetime;
    public float Lifetime;

    public bool Active;

    public RedSprite(Vector2 position, float lifetime)
    {
        Seed = Main.rand.Next(int.MaxValue - 1000);

        Position = position;
        
        for (int i = 0; i < MaxBranches; i++)
            Points[i] = new();
        
        MaxLifetime = lifetime;
        Lifetime = lifetime;

        Active = true;
    }
}

