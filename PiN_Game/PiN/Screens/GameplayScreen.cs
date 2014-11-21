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
        SpriteFont gameFont;

        float pauseAlpha;

        // Resources for drawing.
        private SpriteBatch spriteBatch;

        // Global content.
        private Hud hud;

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
        private KeyboardState previousKeyboardState;
        private MouseState mouseState;
        private MouseState previousMouseState;

        // Program camera
        private Camera cam;

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 1;

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

            hud = new Hud(content);

            cam = new Camera(spriteBatch.GraphicsDevice.Viewport);

            try //This is where the maps are added
            {
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\TomLevel.tmx"), content));
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\MikeMLevel.tmx"), content));
                maps.Add(new Map(Path.Combine(content.RootDirectory, "Levels\\MikeBLevel.tmx"), content));
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

            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(content.Load<Song>("Sounds/TheDescent"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            LoadNextLevel();


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
                        else
                            ReloadCurrentLevel();
                    }
                }

                wasContinuePressed = continuePressed;
            }
        }


        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % maps.Count;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            levelIndex = 0; //index level 2 is MikeBLevel
            level = new Level(ScreenManager.Game.Services, maps[levelIndex], cam);

            // Load the level.
            //string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            //using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            //    level = new Level(ScreenManager.Program.Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.RoyalBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            spriteBatch = ScreenManager.SpriteBatch;

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
