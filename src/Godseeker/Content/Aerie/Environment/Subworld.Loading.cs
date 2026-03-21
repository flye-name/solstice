using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Godseeker.Content.Aerie.Environment;

public partial class AerieSubworld
{
    public override void DrawMenu(GameTime gameTime) => SubworldLoading.DrawCloudTransition();

    public override bool ChangeAudio()
    {
        if (Main.gameMenu)
        {
            Main.newMusic = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Loading");
        }

        return Main.gameMenu; 
    }
}

public class SubworldLoading : ModSystem
{
    public static bool ShouldBeLoading;
    public static float Progress;
    public static int PostLoadDelay;
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
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

                DrawCloudTransition();
                
                Main.spriteBatch.End();
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
            if (PostLoadDelay-- < 0)
                Progress = MathF.Max(Progress - 0.025f, 0f);

            if (Progress <= 0f)
                ShouldBeLoading = false;
        }
        else
        {
            PostLoadDelay = 40;
            Progress = MathF.Min(Progress + 0.025f, 1f);

            if (!Main.gameMenu && Progress >= 1f)
            {
                if (!SubworldSystem.IsActive<AerieSubworld>())
                    SubworldSystem.Enter<AerieSubworld>();
                else
                    SubworldSystem.Exit();
            }
        }
    }

    public static void DrawCloudTransition()
    {
        bool finishedLoading = IntoSubworld == SubworldSystem.IsActive<AerieSubworld>() && !Main.gameMenu;

        int width = Main.screenWidth + 100;
        int height = Main.screenHeight + 100;
        Rectangle rectangle = new Rectangle(-50, -50, width, (int)(height * Progress));
        if (finishedLoading)
            rectangle = new Rectangle(-50, (int)(height * (1f - Progress)) - 50, width, height);
        
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.Black);
    }

    public static void EnterAerie()
    {
        IntoSubworld = !SubworldSystem.IsActive<AerieSubworld>();
        ShouldBeLoading = true;
    }
}

public class SubworldLoadingSceneEffect : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => SubworldLoading.ShouldBeLoading;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Loading");
}