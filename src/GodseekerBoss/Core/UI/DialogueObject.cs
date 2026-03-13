using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace GodseekerBoss.Core.UI;

public struct DialogueData
{
    public string Text;
    public Vector2 Position;
    
    public Color BorderColor = Color.Black;
    public Color Color;
    public Color[] Colors;

    public float Scale = .6f;
    public float[] Scales;
    
    public int LifetimeAfterCompletion = 150;
    
    public SoundStyle? CharacterSound = null;
    public int CharacterInterval = 4, SoundInterval = 5, EndTriggerTime = 1; 

    public DialogueData(string text, Vector2 position, Color color, float scale = 0.6f)
    {
        Text = text;
        Position = position;

        Color = color;
        Scale = scale;
        Colors = new Color[text.Length];
        Scales = new float[text.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            Colors[i] = Color.Transparent;
            Scales[i] = scale * 1.4f;
        }
    }
}

[Autoload(Side = ModSide.Client)]
public class DialogueObject
{
    public static readonly DialogueObject Empty = new DialogueObject(new DialogueData("", Vector2.Zero, Color.White) with { LifetimeAfterCompletion =  0 });
    
    public delegate void End(DialogueObject dialogue);
    public event End OnEnd;
    
    public DialogueData BaseData, Data;
    public int HighlightedCharacterIndex = -1;
    public int Delay;
    public bool Screenspace;
    public DialogueObject(DialogueData data)
    {
        BaseData = data;
        Data = data;
        Data.Text = "";
    }

    public void Update()
    {
        if (Data.LifetimeAfterCompletion <= 0) return;

        Delay--;
        if (Main.ambientCounter % Data.CharacterInterval == 0 && HighlightedCharacterIndex <= BaseData.Text.Length && Delay <= 0)
        {
            HighlightedCharacterIndex++;
            Data.Colors[HighlightedCharacterIndex] = Color.White;
            Data.Text = BaseData.Text.Substring(0, HighlightedCharacterIndex + 1);

            SetDelays();
        }

        if (Data.CharacterSound.HasValue && Main.ambientCounter % Data.SoundInterval == 0 && BaseData.Text[HighlightedCharacterIndex] != ' ' && Delay <= 0)
            SoundEngine.PlaySound(Data.CharacterSound.Value, Data.Position);

        for (int i = 0; i < BaseData.Text.Length; i++)
        {
            if (i >= HighlightedCharacterIndex) continue;
                
            Data.Colors[i] = Color.Lerp(Data.Colors[i], BaseData.Color, 0.1f);
            Data.Scales[i] = MathHelper.Lerp(Data.Scales[i], BaseData.Scale, 0.2f);
        }

        if (HighlightedCharacterIndex > BaseData.Text.Length)
            FadeOut();
    }

    void SetDelays()
    {
        char character = BaseData.Text[HighlightedCharacterIndex];
        if (character == ' ')
            Delay = 3;
            
        if (character == ',')
            Delay = 8;
            
        if (character == '.')
            Delay = 30;
            
        // TODO: chat tag for specific delays
    }
    
    void FadeOut()
    {
        Data.LifetimeAfterCompletion--;
            
        if (Data.LifetimeAfterCompletion == Data.EndTriggerTime)
            OnEnd?.Invoke(this);

        if (Data.LifetimeAfterCompletion < BaseData.LifetimeAfterCompletion / 2f)
        {
            float progress = Utils.GetLerpValue(0, BaseData.LifetimeAfterCompletion / 2f, Data.LifetimeAfterCompletion);

            for (int i = 0; i < BaseData.Text.Length; i++)
                Data.Colors[i] = BaseData.Colors[i] * progress;
        }
    }

    public void Draw() // TODO: multiline
    {
        if (Data.Text.Length <= 0 || Data.LifetimeAfterCompletion <= 0) return;

        DynamicSpriteFont font = FontAssets.DeathText.Value;
        Vector2 position = Data.Position - font.MeasureString(Data.Text) with { Y = 0 } * 0.5f * Data.Scale - (Screenspace ? Vector2.Zero : Main.screenPosition);
        Vector2 origin = font.MeasureString(" ") * 0.5f;
        for (int i = 0; i < Data.Text.Length; i++)
        {   
            float alpha = Data.Colors[i].A / 255f;
            
            var charData = font.GetCharacterData(Data.Text[i]);
            position -= new Vector2((charData.Kerning.X) * Data.Scale * 0.9f, 0);
            
            if (Data.Text[i] == 'j') // TODO: check other characters that might look off
                position += new Vector2((charData.Kerning.X) * Data.Scale * 1.2f, 0);
            
            ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, Data.Text[i].ToString(), position, Data.BorderColor * alpha, 0, origin, Data.Scales[i] * Vector2.One);
            Vector2 newPosition = ChatManager.DrawColorCodedString(Main.spriteBatch, font, Data.Text[i].ToString(), position, Data.Colors[i], 0, origin, Data.Scales[i] * Vector2.One);
            
            position.X = newPosition.X;
        }
    }
}