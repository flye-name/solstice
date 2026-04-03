using Godseeker.Content.Aerie.Environment;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie.Placements;

public sealed class AerieFlute : ModItem
{
    public override string Texture => GeneratedAssets.Assets.Images.Textures.AerieFlute.Key;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 36;
        Item.useAnimation = 60;
        Item.useTime = 60;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<HeldFlute>();
        Item.shootSpeed = 0f;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return true;
        }

        SubworldLoading.EnterAerie();

        return true;
    }
}

public class HeldFlute : ModProjectile 
{
    public override string Texture => GeneratedAssets.Assets.Images.Textures.AerieFluteHeld.Key;
    
    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (Projectile.localAI[0] == 0)
        {
            SoundEngine.PlaySound(new SoundStyle("Godseeker/Assets/Flute"), Projectile.Center);
            Projectile.localAI[0] = 1;
        }
        
        Projectile.timeLeft = 2;

        if (player.itemAnimation <= 0)
        {
            Projectile.Kill();
        }
        
        Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
        
        Projectile.Center = playerCenter + new Vector2(player.direction * 10, -3);
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2);
        
        Projectile.direction = player.direction;
        Projectile.spriteDirection = player.direction;
    }
}