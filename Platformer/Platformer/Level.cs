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
        // Physical structure of the level.
        private Map map;
        //private List<Layer> layers;
        private Layer[] layers;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Consumable> consumables = new List<Consumable>();
        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
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

            string levelPath = string.Format("Levels/{0}.tmx", levelIndex); //levelPath for the current level

            LoadMap(levelPath);
            cam = new Camera(viewport); //instantiating the camera view
            cam.Limits = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);//defining world limits


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
        /// gets all the enemies from the level.
        /// </summary>
        private void LoadEnemies()
        {
            //first is named enemy without a number after it
            int x = map.ObjectGroups["enemies"].Objects["enemy"].X;
            int y = map.ObjectGroups["enemies"].Objects["enemy"].Y;
            string enemyType = map.ObjectGroups["enemies"].Objects["enemy"].Properties["enemyType"];

            SpawnEnemy(x, y, enemyType);

            //the rest are called enemy1, enemy2, etc.
            for (int i = 1; i < map.ObjectGroups["enemies"].Objects.Values.Count((item) => item.Name.Equals("enemy")); i++)
            {
                x = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].X;
                y = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].Y;
                enemyType = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}",i)].Properties["enemyType"];

                SpawnEnemy(x, y, enemyType);
            }
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private void SpawnEnemy(int x, int y, string enemyType)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x, y));
            enemies.Add(EnemyFactory.NewEnemy(this, position, enemyType));
        }

        /// <summary>
        /// Instantiates a consumable and puts it in the level.
        /// </summary>
        private void SpawnConsumable(int x, int y, ConsumableType type)
        {
            Point position = GetTileAtPoint(x, y).Center;
            consumables.Add(new Consumable(this, new Vector2(position.X, position.Y), type));
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

            //get the tileset name of the current tile
            string tilesetName = GetTilesetName(tileId);

            //get list of properties for tile
            Tileset.TilePropertyList currentTileProperties = map.Tilesets[tilesetName].GetTileProperties(tileId);

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
        /// Gets the tileset name of the current tile. This is done by looping through all the tilesets, if there are more than one,
        /// and comparing the tile id with the first id of every tileset.
        /// </summary>
        public string GetTilesetName(int tileId)
        {
            //loops through all the tilesets
            for (int i = 0; i < map.Tilesets.Keys.Count; i++)
            {
                if (i != map.Tilesets.Keys.Count - 1)//if this isn't the last tileset
                {
                    //checks if current tile id is greater than first tile id of current tileset and less than first id of next tileset
                    //then it returns the tileset namekind of clucky but it works
                    if (map.Tilesets[map.Tilesets.Keys.ElementAt(i)].FirstTileID <= tileId && tileId < map.Tilesets[map.Tilesets.Keys.ElementAt(i + 1)].FirstTileID)
                        return map.Tilesets.Keys.ElementAt(i);
                }
                else
                    return map.Tilesets.Keys.ElementAt(i); //if this is the last tileset then return it

            }
            return map.Tilesets.Keys.ElementAt(0); //hopefully there is at least one tileset

        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// the x and y parameters are tile based not pixel based
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * map.TileWidth, y * map.TileHeight, map.TileWidth, map.TileHeight);
        }

        /// <summary>
        /// Gets the bounding rectangle of the tile the point is in.
        /// the x and y parameters are pixel based not tile based
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

                #region Debugging for Tiled Map Editor
                //manually move player for debugging purposes
                if (keyboardState.IsKeyDown(Keys.K))
                {
                    Player.Position = new Vector2(Player.Position.X + 20, Player.Position.Y);
                }

                else if (keyboardState.IsKeyDown(Keys.H))
                {
                    Player.Position = new Vector2(Player.Position.X - 20, Player.Position.Y);
                }
                if (keyboardState.IsKeyDown(Keys.J))
                {
                    Player.Position = new Vector2(Player.Position.X, Player.Position.Y + 20);
                }
                else if (keyboardState.IsKeyDown(Keys.U))
                {
                    Player.Position = new Vector2(Player.Position.X, Player.Position.Y - 20);
                }

                #endregion Debugging for Tiled Map Editor

                Player.Update(gameTime, keyboardState, mouseState, cam, gamePadState, touchState, accelState, orientation);
                UpdateConsumables(gameTime);

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
        /// Animates each consumable and checks to allows the player to collect them.
        /// </summary>
        private void UpdateConsumables(GameTime gameTime)
        {
            for (int i = 0; i < consumables.Count; ++i)
            {
                Consumable consumable = consumables[i];

                consumable.Update(gameTime);

                if (consumable.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    consumables.RemoveAt(i--);
                    OnConsumableCollected(consumable, Player);
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
                // Touching an enemy decreases health of player

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
                    else if (!Player.IsHit)
                    {
                        OnPlayerHit(enemy);
                    }
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled();

            int rand = random.Next(100);

            if (rand >= 90)
                SpawnConsumable((int)enemy.Position.X, (int)enemy.Position.Y, ConsumableType.PowerUp);
            else if (rand <= 30)
                SpawnConsumable((int)enemy.Position.X, (int)enemy.Position.Y, ConsumableType.Health);
        }

        /// <summary>
        /// Called when a consumable is collected.
        /// </summary>
        /// <param name="consumable">The consumable that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnConsumableCollected(Consumable consumable, Player collectedBy)
        {
            consumable.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is hit by an enemy.
        /// </summary>
        /// <param name="hitBy">
        /// The enemy who hit the player. This is null if the player was not hit by an
        /// enemy, such as when a player hits or is hit by a hazard.
        /// </param>
        private void OnPlayerHit(Enemy hitBy)
        {
            Player.OnHit(hitBy);
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

            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        cam.GetViewMatrix(Vector2.One));

            foreach (Consumable consumable in consumables)
                consumable.Draw(gameTime, spriteBatch);

            map.Draw(spriteBatch, new Rectangle((int)cam.Position.X, (int)cam.Position.Y, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), cam.Position);
            
            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

        }//end Draw method



        

       

        #endregion
    }
}
