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
            SpawnRedSprite(new RedSprite(Main.MouseScreen, 60));
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
            for (int i = 0; i < RedSprite.MaxBranches; i++)
                rs.Points[i].Clear();
            rs.Active = false;
        }

        float progress = Utils.GetLerpValue(rs.MaxLifetime, 0, rs.Lifetime);

        if (progress < 0.5f)
        {
            int branchAmount = rs.Random.Next(RedSprite.MaxBranches - 3, RedSprite.MaxBranches);
            for (int i = 0; i < branchAmount; i++)
            {
                UnifiedRandom branchRand = new(rs.Seed + i + 1);
                Vector2 direction = new Vector2(0, 1 * branchRand.Next([1, -1])).RotatedBy(branchRand.NextFloat(-0.4f, 0.4f));
                Vector2 newPosition = rs.Position + direction.RotatedByRandom(0.2f) * branchRand.NextFloat(100, 400);
                Vector2 position = Vector2.Lerp(rs.Position, newPosition, progress * branchRand.Next(1, 3));
                rs.Points[i].Add(position);
            }
        }
    }

    public static void DrawRedSprites()
    {
        for (int i = 0; i < MaxSprites; i++)
        {
            ref RedSprite rs = ref RedSprites[i];
            if (!rs.Active)
                continue;
            
            Vector2 transformScale = Main.BackgroundViewMatrix.Zoom * rs.Random.NextFloat(0.4f, 0.5f);
            Matrix transform = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector2 translation = new Vector2(Main.screenWidth, Main.screenHeight) / 2f * (Vector2.One - transformScale);
            transform *= Matrix.CreateScale(transformScale.X, transformScale.Y, 1) * Matrix.CreateTranslation(new Vector3(translation, 0));
                
            Main.spriteBatch.End(out var snapshot);
            Main.spriteBatch.Begin(snapshot);
            {
                DrawRedSprite(i);
            }
            Main.spriteBatch.Restart(in snapshot);
        }
    }
    
    public static void DrawRedSprite(int index)
    {
        ref RedSprite rs = ref RedSprites[index];

        for (int j = 0; j < RedSprite.MaxBranches; j++)
        for (int i = 1; i < rs.Points[j].Count; i++)
        {
            Utils.DrawLine(Main.spriteBatch, rs.Points[j][i-1] + Main.screenPosition, rs.Points[j][i] + Main.screenPosition, Color.Red, Color.Red, 4);
        }
    }
}