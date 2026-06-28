using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Solstice.Core;

public enum DialogueType : byte
{
    Normal = 0,
    Wind = 1, 
    Storm = 2
}

public struct DialogueData
{
    public DialogueType Type;
    
    public string Text;
    public Vector2 Position;
    
    public Color BorderColor = Color.Black;
    public Color Color;
    public readonly Color[] Colors;

    public float Scale = .6f;
    public readonly float[] Scales;
    
    public SoundStyle? CharacterSound = null;
    
    public int LifetimeAfterCompletion = 150;
    public int CharacterInterval = 4;
    public int SoundInterval = 5;
    public int EndTriggerTime = 1; 

    public DialogueData(string text, Vector2 position, Color color, float scale = 0.6f, DialogueType type = DialogueType.Normal)
    {
        Type = type;
        
        Text = text;
        Position = position;

        Color = color;
        Scale = scale;
        Colors = new Color[text.Length];
        Scales = new float[text.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            Colors[i] = Color.Transparent;
            Scales[i] = scale * (Type == DialogueType.Normal ? 1.4f : 1);
        }
    }
}

[Autoload(Side = ModSide.Client)]
public class DialogueObject
{
    public static readonly DialogueObject Empty = new(new DialogueData("", Vector2.Zero, Color.White) { LifetimeAfterCompletion =  0 });
    
    public delegate void End(DialogueObject dialogue);
    public event End? OnEnd;
    
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
        if (Data.LifetimeAfterCompletion <= 0)
        {
            return;
        }

        Delay--;
        if (Main.timeForVisualEffects % Data.CharacterInterval == 0 && HighlightedCharacterIndex <= BaseData.Text.Length && Delay <= 0)
        {
            HighlightedCharacterIndex++;
            
            Data.Text = BaseData.Text[..(HighlightedCharacterIndex + 1)];

            OnCharacterAppearPerType();
        }

        if (Data.CharacterSound.HasValue && Main.timeForVisualEffects % Data.SoundInterval == 0 && BaseData.Text[HighlightedCharacterIndex] != ' ' && Delay <= 0)
        {
            SoundEngine.PlaySound(Data.CharacterSound.Value, Data.Position);
        }
        
        if (Data.LifetimeAfterCompletion >= BaseData.LifetimeAfterCompletion)
            for (int i = 0; i < BaseData.Text.Length; i++)
            {
                if (i >= HighlightedCharacterIndex)
                {
                    continue;
                }
                    
                Data.Colors[i] = Color.Lerp(Data.Colors[i], BaseData.Color, 0.1f);
                Data.Scales[i] = MathHelper.Lerp(Data.Scales[i], BaseData.Scale, 0.2f);
            }

        if (HighlightedCharacterIndex > BaseData.Text.Length)
        {
            FadeOut();
        }
    }

    void OnCharacterAppearPerType()
    {
        switch (Data.Type)
        {
            case DialogueType.Normal:
                Data.Colors[HighlightedCharacterIndex] = Color.White;
                SetDelays();
                break;
            
            case DialogueType.Wind:
                
                break;
            
            case DialogueType.Storm:
                
                break;
        }
    } 

    private void SetDelays()
    {
        char character = BaseData.Text[HighlightedCharacterIndex];

        // TODO: Chat tags for specific delays.
        Delay = character switch
        {
            ' ' => 3,
            ',' => 8,
            '.' => 30,
            _ => Delay
        };
    }

    private void FadeOut()
    {
        Data.LifetimeAfterCompletion--;
            
        if (Data.LifetimeAfterCompletion == Data.EndTriggerTime)
        {
            OnEnd?.Invoke(this);
        }
        
        float progress = Utils.GetLerpValue(0, BaseData.LifetimeAfterCompletion / 2f, Data.LifetimeAfterCompletion);
        switch (Data.Type)
        {
            case DialogueType.Normal:
                if (Data.LifetimeAfterCompletion < BaseData.LifetimeAfterCompletion / 2f)
                    for (int i = 0; i < BaseData.Text.Length; i++)
                    {
                        Data.Colors[i] = BaseData.Colors[i] * progress;
                    }
                break;
            
            case DialogueType.Wind:
                if (HighlightedCharacterIndex <= BaseData.Text.Length * 2)
                {
                    HighlightedCharacterIndex++;
                }

                for (int i = 0; i < HighlightedCharacterIndex - BaseData.Text.Length; i++)
                {
                    Data.Colors[i] *= 0.9f;
                }
                break;
        }
    }

    public void Draw() // TODO: Multiline dialogues.
    {
        if (Data.Text.Length <= 0 || Data.LifetimeAfterCompletion <= 0)
        {
            return;
        }

        DynamicSpriteFont font = FontAssets.DeathText.Value;

        Vector2 position = Data.Position;


        position -= Screenspace ? Vector2.Zero : Main.screenPosition;

        switch (Data.Type)
        {
            case DialogueType.Normal:
                position.X -= font.MeasureString(Data.Text).X * 0.5f * Data.Scale;
                
                DrawNormalDialogue(position);
                break;
            
            case DialogueType.Wind:
                var spiral = DragonAlphabet.GetPoints(position, BaseData.Text.Length, out var rotations, Data.Scale);
                DrawSpiralDialogue(spiral, rotations);
                break;
        }
    }

    private void DrawSpiralDialogue(Vector2[] positions, float[] rotations)
    {
        DynamicSpriteFont font = FontAssets.DeathText.Value;
        
        Vector2 origin = font.MeasureString(" ") * 0.5f;
        
        for (int i = 0; i < Data.Text.Length; i++)
        {   
            var charData = font.GetCharacterData(Data.Text[i]);
            positions[i] -= new Vector2(charData.Kerning.X * Data.Scale * 0.9f, 0).RotatedBy(rotations[i]);

            // TODO: Better impl for manual kerning.
            if (Data.Text[i] == 'j')
            {
                positions[i] += new Vector2(charData.Kerning.X * Data.Scale * 1.2f, 0).RotatedBy(rotations[i]);
            }
            
            if (!new Rectangle(-200, -200, Main.screenWidth + 200, Main.screenHeight + 200).Contains(positions[i].ToPoint()))
                continue;

            float progress = Utils.GetLerpValue(0, Data.Text.Length, i);
            
            float startFactor = MathHelper.Clamp(progress * 3, 0, 1);
            float additionalScale = MathHelper.Lerp(0.5f, 1, startFactor);
            float windFactor = (1f + MathF.Sin((float)Main.timeForVisualEffects * 0.01f + i * 0.3f)) * 0.5f;
            positions[i] += new Vector2(Main.windSpeedCurrent * progress * 40, windFactor * 30 * progress);

            var coord = new Point16((int)(positions[i].X + Main.screenPosition.X) / 16, (int)(positions[i].Y + Main.screenPosition.Y) / 16);
            if (coord.X > Main.offLimitBorderTiles && coord.X < Main.maxTilesX - Main.offLimitBorderTiles && coord.Y > Main.offLimitBorderTiles && coord.Y < Main.maxTilesY - Main.offLimitBorderTiles) 
            {
                Main.instance.TilesRenderer.Wind.GetWindTime(coord.X, coord.Y, 100, out var timeLeft, out var dirX, out var dirY);
                positions[i] += new Vector2(dirX, dirY) * MathF.Sin(MathF.PI * timeLeft / 100f) * 6;
            }

            ChatManager.DrawColorCodedString(
                Main.spriteBatch,
                font,
                Data.Text[i].ToString(),
                positions[i],
                Data.Colors[i],
                rotations[i],
                origin,
                Data.Scales[i] * Vector2.One * additionalScale
            );
        }
    }

    private void DrawNormalDialogue(Vector2 position)
    {
        DynamicSpriteFont font = FontAssets.DeathText.Value;
        
        Vector2 origin = font.MeasureString(" ") * 0.5f;

        for (int i = 0; i < Data.Text.Length; i++)
        {   
            var charData = font.GetCharacterData(Data.Text[i]);
            position -= new Vector2(charData.Kerning.X * Data.Scale * 0.9f, 0);

            // TODO: Better impl for manual kerning.
            if (Data.Text[i] == 'j')
            {
                position += new Vector2(charData.Kerning.X * Data.Scale * 1.2f, 0);
            }
            
            if (!new Rectangle(-200, -200, Main.screenWidth + 200, Main.screenHeight + 200).Contains(position.ToPoint()))
                continue;
            
            Vector2 newPosition = ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                font,
                Data.Text[i].ToString(),
                position,
                Data.Colors[i],
                Data.BorderColor,
                0,
                origin,
                Data.Scales[i] * Vector2.One
            );
            
            position.X = newPosition.X;
        }
    }
}