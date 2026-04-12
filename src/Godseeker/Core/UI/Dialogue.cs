using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Godseeker.Core;

[Autoload(Side = ModSide.Client)]
public class Dialogue : ModSystem
{
    private static readonly DialogueObject[] dialogue = new DialogueObject[5];

    public override void OnWorldLoad()
    {
        for (int i = 0; i < dialogue.Length; i++)
        {
            dialogue[i] = DialogueObject.Empty;
        }
    }

    public static DialogueObject NewDialogue(DialogueData data, bool screenspace = false)
    {
        int index = 0;

        while (dialogue[index].Data.LifetimeAfterCompletion > 0 && index < dialogue.Length - 1)
        {
            index++;
        }

        dialogue[index] = new DialogueObject(data) { Screenspace = screenspace };

        return dialogue[index];
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int textIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: MP Player Names"));

        layers.Insert(textIndex, new LegacyGameInterfaceLayer("Godseeker: Dialogue",
            () =>
            {
                foreach (DialogueObject t in dialogue)
                {
                    if (!t.Screenspace)
                    {
                        t.Draw();
                    }
                }

                return true;
            })
        );
        
        layers.Insert(textIndex, new LegacyGameInterfaceLayer("Godseeker: DialogueUI",
            () =>
            {
                foreach (DialogueObject dialog in dialogue)
                {
                    if (dialog.Screenspace)
                    {
                        dialog.Draw();
                    }
                }

                return true;
            },
            InterfaceScaleType.UI)
        );
    }

    public override void PostUpdateDusts()
    {
        if (Main.gameInactive)
        {
            return;
        }
        
        for (int i = 0; i < dialogue.Length; i++)
        {
            if (dialogue[i].Data.LifetimeAfterCompletion <= 0 && dialogue[i].Data.Text.Length > 0)
            {
                dialogue[i] = DialogueObject.Empty;
            }
            
            dialogue[i].Update();
        }
    }
}