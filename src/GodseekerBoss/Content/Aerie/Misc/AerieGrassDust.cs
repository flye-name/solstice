using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace GodseekerBoss.Content.Aerie.Misc;

public sealed class AerieGrassDust : ModDust
{
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
