using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Utilities;

namespace Solstice.Content.Aerie.Weather;

public class RedThunderstorm
{
    public static bool Active;
    
    public const int MaxSprites = 10; 
    public static RedSprite[] RedSprites = new RedSprite[MaxSprites];

    [OnLoad]
    public static void Load()
    {
        for (int i = 0; i < MaxSprites; i++)
            RedSprites[i] = new RedSprite(Vector2.Zero, 0)
            {
                Active = false
            };
    }
    
    public static void SpawnRedSprite(RedSprite obj)
    {
        int index = -1;
        for (int i = 0; i < MaxSprites; i++)
        {
            if (RedSprites[i].Active)
                continue;

            index = i;
        }

        if (index == -1)
            return;

        RedSprites[index] = obj;
    }

    [ModSystemHooks.PostUpdateEverything]
    public static void Update()
    {
        // Red sprites are updated even if the event is inactive so clearing ones can fade out properly.
        for (int i = 0; i < MaxSprites; i++)
        {
            if (RedSprites[i].Active)
                UpdateRedSprite(i);
        }

        if (Main.mouseRightRelease && Main.mouseRight)
        {
            Main.NewText("gai");
            SpawnRedSprite(new RedSprite(Main.MouseWorld, 60));
        }

        if (!Active)
            return;
        
        
    }

    static void UpdateRedSprite(int index)
    {
        ref RedSprite rs = ref RedSprites[index];

        rs.Lifetime--;
        Main.NewText(rs.Lifetime);
        if (--rs.Lifetime < 0)
        {
            rs.Points.Clear();
            rs.Active = false;
        }

        float progress = Utils.GetLerpValue(rs.MaxLifetime, 0, rs.Lifetime);
        Vector2 position = Vector2.Lerp(rs.Position, rs.Position + rs.Random.NextVector2Circular(500, 500), progress);
        rs.Points.Add(position);
    }

    public static void DrawRedSprites()
    {
        for (int i = 0; i < MaxSprites; i++)
        {
            ref RedSprite rs = ref RedSprites[i];
            if (!rs.Active)
                continue;
            
            Vector2 transformScale = Main.BackgroundViewMatrix.Zoom * rs.Random.NextFloat(0.1f, 0.2f);
            Matrix transform = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector2 translation = new Vector2(Main.screenWidth, Main.screenHeight) / 2f * (Vector2.One - transformScale);
            transform *= Matrix.CreateScale(transformScale.X, transformScale.Y, 1) * Matrix.CreateTranslation(new Vector3(translation, 0));
                
            Main.spriteBatch.End(out var snapshot);
            Main.spriteBatch.Begin(snapshot with { TransformMatrix = transform });
            {
                DrawRedSprite(i);
            }
            Main.spriteBatch.Restart(in snapshot);
        }
    }
    
    public static void DrawRedSprite(int index)
    {
        ref RedSprite rs = ref RedSprites[index];

        foreach (var point in rs.Points)
        {
            Main.spriteBatch.Draw(TextureAssets.SunOrb.Value, point - Main.screenPosition, Color.White);
        }
    }
}