using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie.Environment;

public partial class AerieSubworld : Subworld
{
    public override void DrawMenu(GameTime gameTime)
    {
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Red);
    }
}

public class SubworldLoading : ModSystem
{
    public static bool ShouldBeLoading;
    public static float Progress;
    public static bool IntoSubworld;
    public static Player DummyPlayer = new Player();
    
    public override void Load()
    {
        On_Main.DrawInterface_34_PlayerChat += (orig, self) =>
        {
            orig(self);

            if (ShouldBeLoading)
            {
                Main.spriteBatch.End(out var snapshot);
                DrawCloudTransition(snapshot);
                Main.spriteBatch.Begin(in snapshot);
            }
        };
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (!ShouldBeLoading)
        {
            DummyPlayer = Main.player[Main.myPlayer];
            return;
        }

        bool finishedLoading = IntoSubworld == SubworldSystem.IsActive<AerieSubworld>() && !Main.gameMenu;
        if (finishedLoading)
        {
            Progress = MathF.Max(Progress - 0.05f, 0f);

            if (Progress <= 0f)
                ShouldBeLoading = false;
        }
        else
        {
            Progress = MathF.Min(Progress + 0.05f, 1f);

            if (!Main.gameMenu && Progress >= 1f)
            {
                if (!SubworldSystem.IsActive<AerieSubworld>())
                    SubworldSystem.Enter<AerieSubworld>();
                else
                    SubworldSystem.Exit();
            }
        }
        
        Main.NewText(Progress);
    }

    public static void DrawCloudTransition(SpriteBatchSnapshot snapshot)
    {
        Main.spriteBatch.Begin(in snapshot);
            
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, (int)(Progress * Main.screenWidth / 2), Main.screenHeight), Color.Red);
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(Main.screenWidth - (int)(Progress * Main.screenWidth / 2), 0, (int)(Progress * Main.screenWidth / 2), Main.screenHeight), Color.Red);
        
        Main.spriteBatch.End();
    }

    public static void EnterAerie()
    {
        IntoSubworld = !SubworldSystem.IsActive<AerieSubworld>();
        ShouldBeLoading = true;
    }
}