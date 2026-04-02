using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SteelSeries.GameSense;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using Terraria.Utilities;
using MiscShaders = Godseeker.GeneratedAssets.Assets.Effects;

namespace Godseeker.Content.Aerie.Environment;

public partial class AerieSubworld : Subworld
{
    public override void DrawMenu(GameTime gameTime) => SubworldLoading.DrawCloudTransition();

    public override bool ChangeAudio()
    {
        if (Main.gameMenu)
            Main.newMusic = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Loading");
        return Main.gameMenu; 
    }
}

public class SubworldLoading : ModSystem
{
    public static bool ShouldBeLoading;
    public static float Progress;
    public static int PostLoadDelay;
    public static bool IntoSubworld;
    public static float Timer;
    public static Player DummyPlayer = new Player();
    
    public override void Load()
    {
        On_Main.DrawInterface_34_PlayerChat += (orig, self) =>
        {
            orig(self);

            if (ShouldBeLoading)
            {
                DrawCloudTransition();
            }
        };
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (!ShouldBeLoading)
        {
            Timer = 0;
            DummyPlayer = Main.player[Main.myPlayer];
            return;
        }

        bool finishedLoading = IntoSubworld == SubworldSystem.IsActive<AerieSubworld>() && !Main.gameMenu;
        if (finishedLoading)
        {
            if (PostLoadDelay-- < 0)
                Progress = MathF.Max(Progress - 0.01f, 0f);

            if (Progress <= 0f)
                ShouldBeLoading = false;
        }
        else
        {
            PostLoadDelay = 40;
            Progress = MathF.Min(Progress + 0.01f, 1f);

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
        Timer += 0.01f;
        
        int width = Main.screenWidth + 500;
        int height = Main.screenHeight + 500;
        
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-250, -250, width, height), Color.Black * Progress);
        
        Main.spriteBatch.End(out var snapshot);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        
        bool finishedLoading = IntoSubworld == SubworldSystem.IsActive<AerieSubworld>() && !Main.gameMenu;

        MiscShaders.AerieLoading.Progress = Progress;
        MiscShaders.AerieLoading.Loaded = finishedLoading;
        MiscShaders.AerieLoading.Time = Main.GlobalTimeWrappedHourly;
        MiscShaders.AerieLoading.Resolution = new Vector2(width, height);
        MiscShaders.AerieLoading.Color = new Vector4(0.98f, 0.95f, 1, 1);
        MiscShaders.AerieLoading.Apply();
        
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-250, -250, width, height), Color.White * Progress);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
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