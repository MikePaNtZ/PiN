using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace PiN
{
    class GameplayScreen : GameScreen
    {
        ContentManager content;

        int screenWidth;
        int screenHeight;

        float pauseAlpha;

        //MikeM level backgrounds (levelIndex 1)
        //Texture2D lvl2bg0;
        //Texture2D lvl2bg1;
        //Texture2D lvl2bg2;
        //Texture2D lvl2Foreground;
        //Texture2D lvl2Gradient;
        //Texture2D lvl2Middleground;
        //Texture2D lvl2Middleplus;
        //Texture2D lvl2Sky1;
        //Texture2D lvl2Sky2;
        Texture2D lvl2TreeSunsetMain;

        //Tom level backgrounds (levelIndex 0)
        //Texture2D lvl1Gradient;
        //Texture2D lvl1Sky;
        //Texture2D lvl1bg2;
        //Texture2D lvl1bg1;
        //Texture2D lvl1Middleground;
        Texture2D lvl1Main;

        // Resources for drawing.
        private SpriteBatch spriteBatch;
        //Texture2D background1;
        //Texture2D background2;
        Texture2D lvl3Main;
        //Texture2D middleground;
        // Global content.
        private Hud hud;

        // Meta-level game state.

        //public int LevelIndex
        //{
        //    get { return levelIndex; }
        //    set { LevelIndex = value; }
        //}

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
        private KeyboardState previousKeyboardState;
        private MouseState mouseState;
        private MouseState previousMouseState;

        // Program camera
        private Camera cam;

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            maps = new List<Map>();
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            XnaDebugDrawer.DebugDrawer.LoadContent(ScreenManager.GraphicsDevice);



            cam = new Camera(spriteBatch.GraphicsDevice.Viewport);

            //background1 = content.Load<Texture2D>("Backgrounds/Layer0_0");
            //background2 = content.Load<Texture2D>("Backgrounds/Layer0_1");

            //MikeM level content (levelIndex 1)
            //lvl2bg0 = content.Load<Texture2D>("Backgrounds/MikeMLevel/bg0-z-1");
            //lvl2bg1 = content.Load<Texture2D>("Backgrounds/MikeMLevel/bg1z-2");
            //lvl2bg2 = content.Load<Texture2D>("Backgrounds/MikeMLevel/bg2z-3");
            //lvl2Foreground = content.Load<Texture2D>("Backgrounds/MikeMLevel/foreground-z2");
            //lvl2Gradient = content.Load<Texture2D>("Backgrounds/MikeMLevel/Gradient-z-6");
            //lvl2Middleground = content.Load<Texture2D>("Backgrounds/MikeMLevel/Middleground-z0");
            //lvl2Middleplus = content.Load<Texture2D>("Backgrounds/MikeMLevel/MiddlePlus-z1");
            //lvl2Sky1 = content.Load<Texture2D>("Backgrounds/MikeMLevel/Sky-z-4");
            //lvl2Sky2 = content.Load<Texture2D>("Backgrounds/MikeMLevel/Sky2-z-5");
            lvl2TreeSunsetMain = content.Load<Texture2D>("Backgrounds/MikeMLevel/treeSunsetMain");

            //Tom level backgrounds (levelIndex 0)
            //lvl1Gradient = content.Load<Texture2D>("Backgrounds/TomLevel/Gradient(z-6)");
            //lvl1Sky = content.Load<Texture2D>("Backgrounds/TomLevel/sky(z-4)");
            //lvl1bg2 = content.Load<Texture2D>("Backgrounds/TomLevel/bg2(z-3)");
            //lvl1bg1 = content.Load<Texture2D>("Backgrounds/TomLevel/bg1(z-2)");
            //lvl1Middleground = content.Load<Texture2D>("Backgrounds/TomLevel/assetmiddleground(z-1)");
            lvl1Main = content.Load<Texture2D>("Backgrounds/TomLevel/lvl1Main");
            lvl3Main = content.Load<Texture2D>("Backgrounds/MikeBLevel/Cool");

            try //This is where the maps are added
            {
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\TomLevel.tmx"), content));
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\MikeBLevel.tmx"), content));
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\MikeMLevel.tmx"), content));
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Map file not found " + e);
                ScreenManager.Game.Exit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ScreenManager.Game.Exit();
            }



            screenWidth = spriteBatch.GraphicsDevice.PresentationParameters.BackBufferWidth;
            screenHeight = spriteBatch.GraphicsDevice.PresentationParameters.BackBufferHeight;

            LoadNextLevel();
            hud = new Hud(content);
            if (levelIndex == 0)
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(content.Load<Song>("Sounds/Scattershot"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (levelIndex == 1)
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(content.Load<Song>("Sounds/TheDescent"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (levelIndex == 2)
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(content.Load<Song>("Sounds/TheComplex"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        public override void UnloadContent()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            HandlePlayerControls();
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // update our level, passing down the GameTime along with all of our input states
                level.Update(gameTime, gameInputHandler);
            }
        }

        protected void HandlePlayerControls()
        {
            // get all of our input states
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            gameInputHandler = new InputHandler(cam, mouseState, previousMouseState, keyboardState, previousKeyboardState);
        }

        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            keyboardState = input.CurrentKeyboardStates[playerIndex];

            if (input.IsPauseGame(ControllingPlayer))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
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
                    }

                    if (level.GameOver)
                    {
                        ExitScreen();
                        ScreenManager.AddScreen(new BackgroundScreen(), ControllingPlayer);
                        ScreenManager.AddScreen(new MainMenuScreen(), ControllingPlayer);
                    }

                }

                wasContinuePressed = continuePressed;
            }
        }


        private void LoadNextLevel()
        {
            // move to the next level
            //levelIndex = (levelIndex + 1) % maps.Count;
            levelIndex++;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            if (levelIndex == maps.Count)
            {
                ExitScreen();
                ScreenManager.AddScreen(new BackgroundScreen(), ControllingPlayer);
                ScreenManager.AddScreen(new MainMenuScreen(), ControllingPlayer);
                return;
            }

            // Load the level.
            //levelIndex = 2; //index level 2 is MikeBLevel
            level = new Level(ScreenManager.Game.Services, maps[levelIndex], cam, levelIndex);
        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            if (levelIndex == 2)
            {
                spriteBatch.Draw(lvl2TreeSunsetMain, screenRectangle, Color.White);
            }
            else if (levelIndex == 0)
            {
                spriteBatch.Draw(lvl1Main, screenRectangle, Color.White);
            }
            else if (levelIndex == 1)
            {
                //MikeB level content (levelIndex 1)
                spriteBatch.Draw(lvl3Main, screenRectangle, Color.White);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.RoyalBlue, 0, 0);


            // Our player and enemy are both actually just text strings.
            spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            DrawScenery();
            spriteBatch.End();

            level.Draw(gameTime, spriteBatch);
            hud.Draw(spriteBatch, level, WarningTime);

            base.Draw(gameTime);


            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

        }
    }
}
