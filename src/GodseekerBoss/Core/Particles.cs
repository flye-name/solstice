using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace GodseekerBoss.Core;

public interface IParticle
{
    public bool IsActive { get; set; }

    public void Update();

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice device);
}

public class ParticleHandler<T> where T : struct, IParticle
{
    public T[] Particles { get; init; }

    public ParticleHandler(int maxParticles)
    {
        Particles = new T[maxParticles];

        Array.Clear(Particles);
    }

    public virtual void Update()
    {
        for (int i = 0; i < Particles.Length; i++)
        {
            if (Particles[i].IsActive)
            {
                Particles[i].Update();
            }
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice device)
    {
        ReadOnlySpan<T> activeParticles = [.. Particles.Where(p => p.IsActive)];

        for (int i = 0; i < activeParticles.Length; i++)
        {
            activeParticles[i].Draw(spriteBatch, device);
        }
    }

    public bool Spawn(T particle)
    {
        int index = Array.FindIndex(Particles, p => !p.IsActive);

        if (index == -1)
        {
            return false;
        }

        Particles[index] = particle;

        return true;
    }
}
