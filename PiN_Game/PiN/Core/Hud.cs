using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace PiN
{
    class Hud
    {
        public static SpriteFont hudFont;

        private Texture2D introOverlay; //so far this is the just team logo. I don't know where to stick it.
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D gameEndOverlay;

        //health bar
        private Texture2D healthBar1;
        private Texture2D healthBar2;
        private Texture2D healthBar3;
        private Texture2D healthTexture;

        public Hud(ContentManager content)
        {
            // Load fonts
            hudFont = content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            introOverlay = content.Load<Texture2D>("Overlays/Intro");
            winOverlay = content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = content.Load<Texture2D>("Overlays/you_died");
            gameEndOverlay = content.Load<Texture2D>("Overlays/GameOver");

            //health bar
            healthBar1 = content.Load<Texture2D>("Sprites/Player/healthbarHero1");
            healthBar2 = content.Load<Texture2D>("Sprites/Player/healthbarHero2");
            healthBar3 = content.Load<Texture2D>("Sprites/Player/healthbarHero3");
            healthTexture = content.Load<Texture2D>("Sprites/Player/health");
        }

        public void Draw(SpriteBatch spriteBatch, Level level, TimeSpan warningTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Rectangle titleSafeArea = spriteBatch.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);
            float hudOpacity = 0.9F;

            // Draw time remaining. Uses modulo division to cause blinking when the
            // activeHero is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > warningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.White * hudOpacity;
            }
            else
            {
                timeColor = Color.Red * hudOpacity;
            }
            DrawShadowedString(spriteBatch, hudFont, timeString, hudLocation, timeColor);

            // Draw health
            if (level.Heroes != null)
            {
                float timeHeight = hudFont.MeasureString(timeString).Y;

                //define the health bar positions
                Vector2 Hero1HealthBarLocation = new Vector2(hudLocation.X + 3, hudLocation.Y + timeHeight * 1.2f);
                Vector2 Hero2HealthBarLocation = new Vector2(hudLocation.X + 3, hudLocation.Y + Hero1HealthBarLocation.Y + healthBar1.Height + 2); //the 2 is the distance between bars
                Vector2 Hero3HealthBarLocation = new Vector2(hudLocation.X + 3, hudLocation.Y + Hero2HealthBarLocation.Y + healthBar2.Height + 2);

                //hero names
                string Hero1Name = "Kaeden ";
                string Hero2Name = "Sammie ";
                string Hero3Name = "Aidan ";

                //drawing hero names
                DrawShadowedString(spriteBatch, hudFont, Hero1Name, Hero1HealthBarLocation, Color.Orange * hudOpacity);
                DrawShadowedString(spriteBatch, hudFont, Hero2Name, Hero2HealthBarLocation, Color.Orange * hudOpacity);
                DrawShadowedString(spriteBatch, hudFont, Hero3Name, Hero3HealthBarLocation, Color.Orange * hudOpacity);

                //updating healthbar locations x so they are next to the names and at an equal x
                Hero1HealthBarLocation.X = hudFont.MeasureString(Hero1Name).X + Hero1HealthBarLocation.X + 6;
                Hero2HealthBarLocation.X = Hero1HealthBarLocation.X;
                Hero3HealthBarLocation.X = Hero1HealthBarLocation.X;

                //draw health bars; The +1s are to compensate for the little offset of the red bar on the background
                spriteBatch.Draw(healthBar1, Hero1HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero1HealthBarLocation.X + 1, (int)Hero1HealthBarLocation.Y + 1, level.Heroes[0].Health - 2, 20), Color.White * hudOpacity);

                spriteBatch.Draw(healthBar2, Hero2HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero2HealthBarLocation.X + 1, (int)Hero2HealthBarLocation.Y + 1, level.Heroes[1].Health - 2, 20), Color.White * hudOpacity);

                spriteBatch.Draw(healthBar3, Hero3HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero3HealthBarLocation.X + 1, (int)Hero3HealthBarLocation.Y + 1, level.Heroes[2].Health - 2, 20), Color.White * hudOpacity);
            }

            if (level.ActiveHero != null)
            {
                DrawShadowedString(spriteBatch, hudFont, "SHIELD: " + (int)level.ActiveHero.ShieldCharge, new Vector2(hudLocation.X + hudFont.MeasureString(timeString).X * 1.2F, hudLocation.Y), Color.White * hudOpacity);
            }

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    if (level.LevelIndex == 2) //hardcoded == ugly but whatever we're done
                        status = introOverlay;
                    else
                        status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.ActiveHero.IsAlive)
            {
                if (level.GameOver)
                    status = gameEndOverlay;
                else
                    status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
