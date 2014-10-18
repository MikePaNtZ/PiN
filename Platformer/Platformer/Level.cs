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
using System.Linq;
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
        private AnimationPlayer resetAfterHit;
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

        // Powerup state
        private const float MaxHitResetTime = 3.0f;
        public float isHitResetTime;
        public bool isHit
        {
            get { return isHitResetTime > 0.0f; }
            //set { wasHit = value; }
        }
        //bool wasHit;

        private readonly Color[] isHitColors = {
                               Color.LightPink,
                               Color.Chartreuse,
                               Color.Cyan,
                               Color.Fuchsia,
                                               };

        private List<Gem> gems = new List<Gem>();
        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        //private Vector2 cameraPosition;
        private Camera cam;

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
            timeRemaining = TimeSpan.FromMinutes(5.0); //changed the time limit to 5 minutes for longer level testing
            //health = 100; //setting the player's health to 100 when the level starts

            string levelPath = string.Format("Levels/{0}.tmx", levelIndex); //levelPath for only the current level

            LoadMap(levelPath);
            cam = new Camera(viewport); //instantiating the camera view
            cam.Limits = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);


            /*This next section doesn't look like Tom's level code*/
            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            //layers = new Layer[3];
            //layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            //layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            //layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Uses the Tiled library to load .tmx tile map
        /// </summary>
        private void LoadMap(String levelPathName)
        {


            map = Map.Load(Path.Combine(Content.RootDirectory, levelPathName), content);

            LoadPlayer();

            LoadEnemies();


            //Loading a gem
            //LoadGem((int)Math.Floor((float)map.ObjectGroups["events"].Objects["Gem"].X / map.TileWidth),
            //           (int)Math.Floor((float)map.ObjectGroups["events"].Objects["Gem"].Y / map.TileHeight), true);

            //exit point
            LoadExit();
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private void LoadPlayer()
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            int x = map.ObjectGroups["events"].Objects["player"].X;
            int y = map.ObjectGroups["events"].Objects["player"].Y;

            start = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x,y));

            player = new Player(this, start);

        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private void LoadExit()
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            int x = map.ObjectGroups["events"].Objects["exit"].X;
            int y = map.ObjectGroups["events"].Objects["exit"].Y;

            exit = GetTileAtPoint(x, y).Center;
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private void LoadEnemies()
        {
            int x = map.ObjectGroups["enemies"].Objects["enemy"].X;
            int y = map.ObjectGroups["enemies"].Objects["enemy"].Y;
            string enemyType = map.ObjectGroups["enemies"].Objects["enemy"].Properties["enemyType"];

            SpawnEnemy(x, y, enemyType);

            for (int i = 1; i < map.ObjectGroups["enemies"].Objects.Values.Count((item) => item.Name.Equals("enemy")); i++)
            {
                x = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].X;
                y = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].Y;
                enemyType = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].Properties["enemyType"];

                SpawnEnemy(x, y, enemyType);
            }
        }

        private void SpawnEnemy(int x, int y, string enemyType)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x, y));
            enemies.Add(new Enemy(this, position, enemyType));
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private void SpawnConsumable(int x, int y, string type)
        {
            Point position = GetTileAtPoint(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y), true));
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
            //int tileId2 = map.Layers["Background"].GetTile(x, y); //the background layer of the scene in Tiled map editor
            //in case the Background layer works


            /*****************************************************************************************HOW DO I ADD ANOTHER TILESET***********************************/
            //get list of properties for tile
            Tileset.TilePropertyList currentTileProperties = map.Tilesets["platformertiles"].GetTileProperties(tileId);
            //Tileset.TilePropertyList nightmareIceTileProperties = map.Tilesets["Nightmare_Ice"].GetTileProperties(tileId);
            //Tileset.TilePropertyList ruinTileProperties = map.Tilesets["Classical_Ruin"].GetTileProperties(tileId);
            //Tileset.TilePropertyList multiPurposeTileProperties = map.Tilesets["MultiPurpose"].GetTileProperties(tileId);
            //Tileset.TilePropertyList oldPlatformerTileProperties = map.Tilesets["oldPlatformer"].GetTileProperties(tileId);
            //Tileset.TilePropertyList BackgroundTileProperties = map.Tilesets["Backgrounds"].GetTileProperties(tileId);

            /*collision properties for platformertiles tileset in Tiled -- Tom's original code*/
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

            
            ///*******************maybe this is how I add the other tileset?**********************************************/
            ///********************and it worked!***************************************************/
            
            ///*collision properties for Nightmare_Ice tileset in Tiled*/
            //if (nightmareIceTileProperties != null) //check if current tile has properties
            //{
            //    switch (Convert.ToInt32(nightmareIceTileProperties["TileCollision"]))//should be a number 0-2
            //    {
            //        case 0:
            //            return TileCollision.Passable;
            //        case 1:
            //            return TileCollision.Impassable;
            //        case 2:
            //            return TileCollision.Platform;
            //    }
            //}

            ///*collision properties for Classic_Ruins tileset in Tiled*/
            //if (ruinTileProperties != null) //check if current tile has properties
            //{
            //    switch (Convert.ToInt32(ruinTileProperties["TileCollision"]))//should be a number 0-2
            //    {
            //        case 0:
            //            return TileCollision.Passable;
            //        case 1:
            //            return TileCollision.Impassable;
            //        case 2:
            //            return TileCollision.Platform;
            //    }
            //}

            ///*collision properties for multiPurpose tileset in Tiled*/
            //if (multiPurposeTileProperties != null) //check if current tile has properties
            //{
            //    switch (Convert.ToInt32(multiPurposeTileProperties["TileCollision"]))//should be a number 0-2
            //    {
            //        case 0:
            //            return TileCollision.Passable;
            //        case 1:
            //            return TileCollision.Impassable;
            //        case 2:
            //            return TileCollision.Platform;
            //    }
            //}

            ///*collision properties for oldPlatformer tileset in Tiled*/
            //if (oldPlatformerTileProperties != null) //check if current tile has properties
            //{
            //    switch (Convert.ToInt32(oldPlatformerTileProperties["TileCollision"]))//should be a number 0-2
            //    {
            //        case 0:
            //            return TileCollision.Passable;
            //        case 1:
            //            return TileCollision.Impassable;
            //        case 2:
            //            return TileCollision.Platform;
            //    }
            //}

            ///*collision properties for Backgrounds tileset in Tiled*/
            //if (BackgroundTileProperties != null) //check if current tile has properties
            //{
            //    switch (Convert.ToInt32(BackgroundTileProperties["TileCollision"]))//should be a number 0-2
            //    {
            //        case 0:
            //            return TileCollision.Passable;
            //        case 1:
            //            return TileCollision.Impassable;
            //        case 2:
            //            return TileCollision.Platform;
            //    }
            //}
            
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
        /// Gets the bounding rectangle of the tile the point is in.
        /// </summary>        
        public Rectangle GetTileAtPoint(int x, int y)
        {
            return GetBounds((int)Math.Floor((float)x / map.TileWidth), (int)Math.Floor((float)y / map.TileHeight));
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

        public int Health
        {
            get { return health; }
            set { this.health = value; }
        }
        int health;//simply setting health to 100

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
            MouseState mouseState,
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

            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;

                //leave a time delay after the player is hit by the enemy
                if (isHit)
                    isHitResetTime = Math.Max(0.0f, isHitResetTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                isHitResetting();


                #region Debugging for Tiled Map Editor
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

                #endregion Debugging for Tiled Map Editor

                Player.Update(gameTime, keyboardState, mouseState, gamePadState, touchState, accelState, orientation);
                UpdateGems(gameTime);

                //follow player
                cam.LookAt(Player.Position);


                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * map.TileHeight)
                {
                    OnPlayerKilled(null);
                }

                UpdateEnemies(gameTime);

                

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

                /*Someone please figure out how to make a player's health

                /****************************************************************************************************************************************************************************/
                // Touching an enemy instantly kills the player
                if (enemy.IsAlive & enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (Player.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Player); //enemy dies instantly when you are in invincibility mode
                    }
                    else if (Player.isBlocking)
                    {
                        OnEnemyKilled(enemy, Player);
                    }
                    else if (isHit == true)//finally found the health will be affected
                    {
 
                        health -= 1;
                        //wasHit = true;
                        //OnPlayerKilled(enemy);
                    }
                }
            }
        }
        /*************************************************************************************************************************************************************/
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


            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        cam.GetViewMatrix(Vector2.One));


            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
            //RasterizerState.CullCounterClockwise, null, cam.GetViewMatrix(Vector2.One)); 

            /*foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);*/

            map.Draw(spriteBatch, new Rectangle((int)cam.Position.X, (int)cam.Position.Y, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), cam.Position);

            // Calculate a tint color based on power up state.
            Color color;
            if (isHit)
            {
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + isHitResetTime / MaxHitResetTime) * 20.0f;
                int colorIndex = (int)t % isHitColors.Length;
                color = isHitColors[colorIndex];
            }
            else
            {
                color = Color.White;
            }
            // Draw that resetAfterHit.


            /****I cannot draw the animation for after the player gets hit by an enemy******/
            //resetAfterHit.Draw(gameTime, spriteBatch, Player.Position, SpriteEffects.None, color);
            Player.Draw(gameTime, spriteBatch);
            


            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            /*spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition.X);
            spriteBatch.End();*/
        }//end Draw method

        public void isHitResetting()
        {
            isHitResetTime = MaxHitResetTime;
            //wasHit = false;
        }


        

       

        #endregion
    }
}
