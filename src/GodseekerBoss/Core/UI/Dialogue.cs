using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace GodseekerBoss.Core.UI;

[Autoload(Side = ModSide.Client)]
public class Dialogue : ModSystem
{
    public static DialogueObject[] DialogueObjects = new DialogueObject[15];
    public override void OnWorldLoad()
    {
        for (int i = 0; i < DialogueObjects.Length; i++) 
            DialogueObjects[i] = DialogueObject.Empty;
    }

    public static DialogueObject NewDialogue(DialogueData data, bool screenspace = false)
    {
        int index = 0;

        while (DialogueObjects[index].Data.LifetimeAfterCompletion > 0 && index < DialogueObjects.Length - 1)
            index++;
        
        DialogueObjects[index] = new DialogueObject(data);
        DialogueObjects[index].Screenspace = screenspace;
        
        return DialogueObjects[index];
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int textIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: MP Player Names"));
        layers.Insert(textIndex, new LegacyGameInterfaceLayer("Godseeker: Dialogue", () =>
        {
            for (int i = 0; i < DialogueObjects.Length; i++)
                if (!DialogueObjects[i].Screenspace)
                    DialogueObjects[i].Draw();

            return true;
        }));
        
        layers.Insert(textIndex, new LegacyGameInterfaceLayer("Godseeker: DialogueUI", () =>
        {
            for (int i = 0; i < DialogueObjects.Length; i++)
                if (DialogueObjects[i].Screenspace)
                    DialogueObjects[i].Draw();

            return true;
        }, InterfaceScaleType.UI));
    }

    public override void PostUpdateEverything()
    {
        for (int i = 0; i < DialogueObjects.Length; i++)
        {
            if (DialogueObjects[i] is null || (DialogueObjects[i].Data.LifetimeAfterCompletion <= 0 && DialogueObjects[i].Data.Text.Length > 0))  
                DialogueObjects[i] = DialogueObject.Empty;
            
            DialogueObjects[i].Update();
        }
    }
}