using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Solstice.Content.Aerie;

public class RodOfMoses : ModItem
{
    public override string Texture => Assets.Images.RodOfMoses.KEY;

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.SuspiciousLookingEye);
        
        Item.useStyle = ItemUseStyleID.RaiseLamp;
        Item.maxStack = 1;
        Item.consumable = false;
    }

    public override bool? UseItem(Player player)
    {
        if (!AerieSubworld.Active)
            return false;
        
        if (RedThunderstormSky.Active)
        {
            SoundEngine.PlaySound(SoundID.Item165);
        }
        else
        {
            var slot = SoundEngine.PlaySound(Assets.Sounds.Thunder.CloseThunder.Asset with { PitchVariance = 0.2f, Volume = 2 });
            if (SoundEngine.TryGetActiveSound(slot, out var sound))
            {
                sound.Sound?.INTERNAL_applyReverb(0.5f);
            }
        }
        
        RedThunderstormSky.Active = !RedThunderstormSky.Active;
        return true;
    }
}