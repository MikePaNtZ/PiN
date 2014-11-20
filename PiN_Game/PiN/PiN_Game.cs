using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;


namespace PiN
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PiN_Game : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
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

        public PiN_Game()

        {
            
            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();

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

            XnaDebugDrawer.DebugDrawer.LoadContent(GraphicsDevice);

            hud = new Hud(Content);

            cam = new Camera(spriteBatch.GraphicsDevice.Viewport);

            try //This is where the maps are added
            {
                maps.Add(new Map(Path.Combine(Content.RootDirectory, "Levels\\TomLevel.tmx"), Content));
                maps.Add(new Map(Path.Combine(Content.RootDirectory, "Levels\\MikeMLevel.tmx"), Content));
                maps.Add(new Map(Path.Combine(Content.RootDirectory, "Levels\\MikeBLevel.tmx"), Content));
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

            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/TheDescent"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            gameInputHandler = new InputHandler(cam, mouseState, previousMouseState, keyboardState, previousKeyboardState);
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
            levelIndex = 0; //index level 2 is MikeBLevel
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

            hud.Draw(spriteBatch, level, WarningTime);

            base.Draw(gameTime);
        }
    }
}
