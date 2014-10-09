#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;

namespace Platformer
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {

        private Map map;
        // Physical structure of the level.
        //private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        //private Vector2 cameraPosition;
        private Camera cam;

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        public Level(IServiceProvider serviceProvider, int levelIndex, Viewport viewport)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            //cameraPosition = new Vector2(0, this.TileHeight * Height - spriteBatch.GraphicsDevice.Viewport.Height);
            timeRemaining = TimeSpan.FromMinutes(2.0);

            string levelPath = string.Format("Levels/{0}.tmx", levelIndex);

            LoadMap(levelPath);
            cam = new Camera(viewport);
            cam.Limits = new Rectangle(0, 0, map.Width*map.TileWidth, map.Height*map.TileHeight);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Uses the Tiled library to load .tmx tile map
        /// </summary>
        private void LoadMap(String levelPathName) 
        {
            

            map = Map.Load(Path.Combine(Content.RootDirectory, levelPathName), content);

            //get player start point. Have to convert to tilebased so divide by tile dimensions
            LoadPlayer((int)Math.Floor((float)map.ObjectGroups["events"].Objects["player"].X / map.TileWidth),
                       (int)Math.Floor((float)map.ObjectGroups["events"].Objects["player"].Y / map.TileHeight));

            //exit point
            LoadExit((int)Math.Floor((float)map.ObjectGroups["events"].Objects["exit"].X / map.TileWidth),
                     (int)Math.Floor((float)map.ObjectGroups["events"].Objects["exit"].Y / map.TileHeight));

            #region load map commented Code
            //// Load the level and ensure all of the lines are the same length.
            //int width;
            //List<string> lines = new List<string>();
            //using (StreamReader reader = new StreamReader(fileStream)) {
            //    string line = reader.ReadLine();
            //    width = line.Length;
            //    while (line != null) {
            //        lines.Add(line);
            //        if (line.Length != width)
            //            throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
            //        line = reader.ReadLine();
            //    }
            //}

            //// Allocate the tile grid.
            //tiles = new Tile[width, lines.Count];

            //// Loop over every tile position,
            //for (int y = 0; y < Height; ++y) {
            //    for (int x = 0; x < Width; ++x) {
            //        // to load each tile.
            //        char tileType = lines[y][x];
            //        tiles[x, y] = LoadTile(tileType, x, y);
            //    }
            //}

            //// Verify that the level has a beginning and an end.
            //if (Player == null)
            //    throw new NotSupportedException("A level must have a starting point.");
            //if (exit == InvalidPosition)
            //    throw new NotSupportedException("A level must have an exit.");
            #endregion

        }
        #region load tiles commented code
        /*
        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y, false);

                // Power-up gem
                case 'P':
                    return LoadGemTile(x, y, true);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA");
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }
        */
        #endregion

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private void LoadPlayer(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private void LoadExit(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private void LoadEnemy(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private void LoadGem(int x, int y, bool isPowerUp)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y), isPowerUp));
        }
        
        
        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            //get the id of tile
            int tileId = map.Layers["Foreground"].GetTile(x, y);

            //get list of properties for tile
            Tileset.TilePropertyList currentTileProperties = map.Tilesets["platformertiles"].GetTileProperties(tileId);

            if (currentTileProperties != null) //check if current tile has properties
            {
                switch (Convert.ToInt32(currentTileProperties["TileCollision"]))//should be a number 0-2
                {
                    case 0:
                        return TileCollision.Passable;
                    case 1:
                        return TileCollision.Impassable;
                    case 2:
                        return TileCollision.Platform;
                }
            }
            

            return TileCollision.Passable; //ideally shouldn't actually get to here but if it does tile is passable
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * map.TileWidth, y * map.TileHeight, map.TileWidth, map.TileHeight);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return map.Width; }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return map.Height; }
        }

        /// <summary>
        /// Width of tiles in this level.
        /// </summary>
        public int TileWidth
        {
            get { return map.TileWidth; }
        }

        /// <summary>
        /// Height of the tiles in this level.
        /// </summary>
        public int TileHeight
        {
            get { return map.TileHeight; }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
                
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;

                //manually move player for debugging purposes
                if (keyboardState.IsKeyDown(Keys.K))
                {
                    Player.Position = new Vector2(Player.Position.X + 20, Player.Position.Y);
                    //cam.Move(new Vector2(10, 0));
                }

                else if (keyboardState.IsKeyDown(Keys.H))
                {
                    Player.Position = new Vector2(Player.Position.X - 20, Player.Position.Y);
                    //cam.Move(new Vector2(-10, 0));
                }
                if (keyboardState.IsKeyDown(Keys.J))
                {
                    Player.Position = new Vector2(Player.Position.X, Player.Position.Y + 20);
                    //cam.Move(new Vector2(0, 10));
                }
                else if (keyboardState.IsKeyDown(Keys.U))
                {
                    Player.Position = new Vector2(Player.Position.X, Player.Position.Y - 20);
                    //cam.Move(new Vector2(0, -10));
                }

                Player.Update(gameTime, keyboardState, gamePadState, touchState, accelState, orientation);

                //follow player
                cam.LookAt(Player.Position);

                //UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                //if (Player.BoundingRectangle.Top >= Height * map.TileHeight)
                    //OnPlayerKilled(null);

                    //UpdateEnemies(gameTime);

                //just making sure collision is working
                switch (GetCollision((int)Math.Floor((float)Player.Position.X / map.TileWidth),
                                     (int)Math.Floor((float)Player.Position.Y / map.TileHeight)))
                {
                    case TileCollision.Impassable:
                        score++;
                        break;
                }

                    // The player has reached the exit if they are standing on the ground and
                    // his bounding rectangle contains the center of the exit tile. They can only
                    // exit when they have collected all of the gems.
                
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            
                

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (Player.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Player);
                    }
                    else
                    {
                        OnPlayerKilled(enemy);
                    }
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            /*spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition.X);
            spriteBatch.End();*/


            
            //ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
           // Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0.0f);

            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        cam.GetViewMatrix(Vector2.One));
            

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                              //RasterizerState.CullCounterClockwise, null, cam.GetViewMatrix(Vector2.One)); 

            //cameraPosition.Y = 544;
            //DrawTiles(spriteBatch);
            //cam.Position = new Vector2(0, 544);
            
            
            //spriteBatch.Begin();
            /*foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);*/
            map.Draw(spriteBatch, new Rectangle((int)cam.Position.X, (int)cam.Position.Y, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), cam.Position);

            Player.Draw(gameTime, spriteBatch);
            
            

            /*foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);*/

            spriteBatch.End();

            /*spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition.X);
            spriteBatch.End();*/
        }
    

        /*private void ScrollCamera(Viewport viewport)
        {
#if ZUNE
const float ViewMargin = 0.45f;
#else
            const float ViewMargin = 0.35f;
#endif

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition.X + marginWidth;
            float marginRight = cameraPosition.X + viewport.Width - marginWidth;

            float marginHeight = viewport.Height * ViewMargin;
            float marginTop = cameraPosition.Y + marginHeight;
            float marginBottom = cameraPosition.Y + viewport.Height - marginHeight;

            // Calculate how far to scroll when the player is near the edges of the screen.
            Vector2 cameraMovement = new Vector2(0.0f, 0.0f);
            if (Player.Position.X < marginLeft)
                cameraMovement.X = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement.X = Player.Position.X - marginRight;

            if (Player.Position.Y < marginTop)
                cameraMovement.Y = Player.Position.Y - marginTop;
            else if (Player.Position.Y > marginBottom)
                cameraMovement.Y = Player.Position.Y - marginBottom;

            // Update the camera position, but prevent scrolling off the ends of the level.
            Vector2 maxCameraPosition = new Vector2(this.TileWidth * Width - viewport.Width, this.TileHeight * Height - viewport.Height);
            cameraPosition.X = MathHelper.Clamp(cameraPosition.X + cameraMovement.X, 0.0f, maxCameraPosition.X);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y + cameraMovement.Y, 0.0f, maxCameraPosition.Y);
        }*/

        #region drawTiles commented code
        /*
        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
        */
        #endregion

        #endregion
    }
}
