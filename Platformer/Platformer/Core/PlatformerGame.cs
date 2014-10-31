#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Squared.Tiled;


namespace Platformer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D introOverlay; //so far this is the just team logo. I don't know where to stick it.
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        //health bar
        private Texture2D healthBar;
        private Texture2D healthTexture;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private List<Map> maps;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private InputHandler gameInputHandler;
        private KeyboardState keyboardState;
        private MouseState mouseState;

        // Game camera
        private Camera cam;
        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 1;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);

            //graphics.ToggleFullScreen();

            Content.RootDirectory = "Content";

            maps = new List<Map>();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            introOverlay = Content.Load<Texture2D>("Overlays/Intro");
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            //health bar
            healthBar = Content.Load<Texture2D>("Sprites/Player/healthbar");
            healthTexture = Content.Load<Texture2D>("Sprites/Player/health");

            cam = new Camera(spriteBatch.GraphicsDevice.Viewport);

            try //This is where the maps are added
            {
                maps.Add(Map.Load(Path.Combine(Content.RootDirectory, "Levels\\TomLevel.tmx"), Content));
                maps.Add(Map.Load(Path.Combine(Content.RootDirectory, "Levels\\MikeMLevel.tmx"), Content));
                maps.Add(Map.Load(Path.Combine(Content.RootDirectory, "Levels\\MikeBLevel.tmx"), Content));
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Map file not found " + e);
                this.Exit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Exit();
            }

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, gameInputHandler);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            gameInputHandler = new InputHandler(cam, mouseState, keyboardState);
            // Exit the game when back is pressed.
            if (gameInputHandler.KeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            bool continuePressed = gameInputHandler.KeyboardState.IsKeyDown(Keys.Space);

            // Perform the appropriate action to advance the game and
            // to get the activeHero back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.ActiveHero.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            
            // move to the next level
            levelIndex = (levelIndex + 1) % maps.Count;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            levelIndex = 2; //index level 2 is MikeBLevel
            level = new Level(Services,maps[levelIndex],cam);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);
            float hudOpacity = 0.7F;

            // Draw time remaining. Uses modulo division to cause blinking when the
            // activeHero is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.White * hudOpacity;
            }
            else
            {
                timeColor = Color.Red * hudOpacity;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw health
            if (level.Heroes != null)
            {
                float timeHeight = hudFont.MeasureString(timeString).Y;
                
                //define the health bar positions
                Vector2 Hero1HealthBarLocation = new Vector2(hudLocation.X, hudLocation.Y + timeHeight * 1.2f);
                Vector2 Hero2HealthBarLocation = new Vector2(hudLocation.X, hudLocation.Y + Hero1HealthBarLocation.Y + healthBar.Height + 2); //the 2 is the distance between bars
                Vector2 Hero3HealthBarLocation = new Vector2(hudLocation.X, hudLocation.Y + Hero2HealthBarLocation.Y + healthBar.Height + 2);

                //hero names
                string Hero1Name = "Kaeden ";
                string Hero2Name = "Sammie ";
                string Hero3Name = "Aidan ";

                //drawing hero names
                DrawShadowedString(hudFont, Hero1Name, Hero1HealthBarLocation, Color.White * hudOpacity);
                DrawShadowedString(hudFont, Hero2Name, Hero2HealthBarLocation, Color.White * hudOpacity);
                DrawShadowedString(hudFont, Hero3Name, Hero3HealthBarLocation, Color.White * hudOpacity);

                //updating healthbar locations x so they are next to the names and at an equal x
                Hero1HealthBarLocation.X = hudFont.MeasureString(Hero1Name).X + Hero1HealthBarLocation.X;
                Hero2HealthBarLocation.X = Hero1HealthBarLocation.X;
                Hero3HealthBarLocation.X = Hero1HealthBarLocation.X;

                //draw health bars; The +1s are to compensate for the little offset of the green bar on the background
                spriteBatch.Draw(healthBar, Hero1HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero1HealthBarLocation.X + 1, (int)Hero1HealthBarLocation.Y + 1, level.Heroes[0].Health * 2 - 2, 30), Color.White * hudOpacity);

                spriteBatch.Draw(healthBar, Hero2HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero2HealthBarLocation.X + 1, (int)Hero2HealthBarLocation.Y + 1, level.Heroes[1].Health * 2 - 2, 30), Color.White * hudOpacity);

                spriteBatch.Draw(healthBar, Hero3HealthBarLocation, Color.White * hudOpacity);
                spriteBatch.Draw(healthTexture, new Rectangle((int)Hero3HealthBarLocation.X + 1, (int)Hero3HealthBarLocation.Y + 1, level.Heroes[2].Health * 2 - 2, 30), Color.White * hudOpacity);
            }
           
            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    
                    status = loseOverlay;
                }
            }
            else if (!level.ActiveHero.IsAlive)
            {
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

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
