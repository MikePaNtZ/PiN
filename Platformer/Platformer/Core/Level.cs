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
using System.Diagnostics;
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
    /// The level owns the the hero and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Map map;

        // reordered list of tilesets. ordered by first tile id
        private Dictionary<string, Tileset> tilesets;

        //private List<Layer> layers;
        private Layer[] layers;

        // Entities in the level.
        public Hero Hero
        {
            get { return hero; }
        }
        private Hero hero;

        // array to store the three different types of heroes
        public Hero[] heroType = new Hero[3];

        private List<Consumable> consumables = new List<Consumable>();
        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        private Camera cam;

        public Camera Camera
        {
            get { return cam; }
        }

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
        public Level(IServiceProvider serviceProvider, Map currentMap, Camera camera)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            timeRemaining = TimeSpan.FromMinutes(5.0); //changed the time limit to 5 minutes for longer level testing

            map = currentMap;

            LoadMap();
            cam = camera;
            cam.Limits = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);//defining world limits


            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Uses the Tiled library to load .tmx tile map
        /// </summary>
        private void LoadMap()
        {
            //order tilesets by the first tile id. needed for tile collision
            tilesets = map.Tilesets.OrderBy((item) => item.Value.FirstTileID).ToDictionary(i => i.Key, i => i.Value);

            LoadHero();

            LoadEnemies();

            LoadExit();
        }

        /// <summary>
        /// Instantiates a hero, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private void LoadHero()
        {
            if (Hero != null)
                throw new NotSupportedException("A level may only have one starting point.");

            int x = map.ObjectGroups["events"].Objects["player"].X;
            int y = map.ObjectGroups["events"].Objects["player"].Y;

            start = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x,y));

            hero = new Hero(this, start, this.Content.Load<Texture2D>("Sprites/Player/Idle"));

        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private void LoadExit()
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            // get the position of the exit object in Tiled map editor
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
        public void SpawnEnemy(int x, int y, string enemyType)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x, y));
            enemies.Add(EnemyFactory.NewEnemy(this, position, enemyType));
        }

        /// <summary>
        /// Instantiates a consumable and puts it in the level.
        /// </summary>
        public void SpawnConsumable(int x, int y, string type)
        {
            Point position = GetTileAtPoint(x, y).Center;
            consumables.Add(ConsumableFactory.NewConsumable(this, new Vector2(position.X, position.Y), type));
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
            //if (y >= Height)
                return TileCollision.Passable;

            //get the id of tile
            int tileId = map.Layers["Foreground"].GetTile(x, y);

            //if tileId is 0 that means there is no tile, so it's passable
            if (tileId == 0)
                return TileCollision.Passable;

            Tileset.TilePropertyList currentTileProperties;
            try
            {
                //get list of properties for tile
                currentTileProperties = GetTileProperties(tileId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Something wrong with getting tileset name " + e);
                return TileCollision.Passable;
            }

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

            // changed this to Impassable so that the character won't fall through a tile if its TilCollision property was not set
            return TileCollision.Impassable; //ideally shouldn't actually get to here
        }

        /// <summary>
        /// Returns the property list of the current tile id. First it has to find which tileset the tile belongs to.
        /// The tilesets are ordered by the FirstTileID. This is done by looping through the tilesets and comparing the first tile id to the current tile id. 
        /// If it is greater then return the previous tileset properties.
        /// </summary>
        public Tileset.TilePropertyList GetTileProperties(int tileId)
        {
            //If there is only one tileset return its properties
            if (tilesets.Count == 1)
                return tilesets.First().Value.GetTileProperties(tileId);
            
            //loops through all the tilesets
            for (int i = 1; i < tilesets.Count; i++)//start at the second one
            {
                //checks if first tile id of the tileset is greater than current tile id
                //If it is then return the previous tileset properties
                if (tilesets.ElementAt(i).Value.FirstTileID > tileId)
                    return tilesets.ElementAt(i-1).Value.GetTileProperties(tileId);
            }

            //if tileId is greater than all of the FirstTileID of the tilesets then it has to be the last one
            return tilesets.Last().Value.GetTileProperties(tileId);
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
        /// and handles the time limit.
        /// </summary>
        public void Update(GameTime gameTime, InputHandler gameInputs)
        {
           
            // Pause while the hero is dead or time is expired.
            if (!Hero.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the hero.
                Hero.PhysicsEngine.ApplyPhysics(gameTime);

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

                Hero.Update(gameTime, gameInputs);
                UpdateConsumables(gameTime);

                //follow the hero
                Camera.LookAt(Hero.Position);

                // Falling off the bottom of the level kills the hero.
                if (Hero.BoundingRectangle.Top >= Height * map.TileHeight)
                {
                    OnHeroKilled(null);
                }

                UpdateEnemies(gameTime);

                

                // The hero has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.

                if (Hero.IsAlive &&
                    Hero.IsOnGround &&
                    Hero.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        /// <summary>
        /// Animates each consumable and checks to allows the hero to collect them.
        /// </summary>
        private void UpdateConsumables(GameTime gameTime)
        {
            for (int i = 0; i < consumables.Count; ++i)
            {
                Consumable consumable = consumables[i];

                consumable.Update(gameTime);

                if (consumable.BoundingCircle.Intersects(Hero.BoundingRectangle))
                {
                    consumables.RemoveAt(i--);
                    OnConsumableCollected(consumable, Hero);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the hero.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);
                // Touching an enemy decreases health of the hero

                if (enemy.IsAlive & enemy.BoundingRectangle.Intersects(Hero.BoundingRectangle))
                {
                    if (Hero.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Hero); //enemy dies instantly when you are in invincibility mode
                    }
                    else if (Hero.IsBlocking)
                    {
                        OnEnemyKilled(enemy, Hero);
                    }
                    else if (!Hero.IsHit)
                    {
                        OnHeroHit(enemy);
                    }
                }
            }
        }

        private void OnEnemyKilled(Enemy enemy, Hero killedBy)
        {
            enemy.OnKilled();
        }

        /// <summary>
        /// Called when a consumable is collected.
        /// </summary>
        /// <param name="consumable">The consumable that was collected.</param>
        /// <param name="collectedBy">The hero who collected this gem.</param>
        private void OnConsumableCollected(Consumable consumable, Hero collectedBy)
        {
            consumable.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the hero is hit by an enemy.
        /// </summary>
        /// <param name="hitBy">
        /// The enemy who hit the hero. This is null if the hero was not hit by an
        /// enemy, such as when a hero hits or is hit by a hazard.
        /// </param>
        private void OnHeroHit(Enemy hitBy)
        {
            Hero.OnHit(hitBy);
        }

        /// <summary>
        /// Called when the hero is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the hero. This is null if the hero was not killed by an
        /// enemy, such as when a hero falls into a hole.
        /// </param>
        private void OnHeroKilled(Enemy killedBy)
        {
            Hero.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the hero reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Hero.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the hero to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Hero.Reset(start);
        }

        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        Camera.GetViewMatrix(Vector2.One));
            
            map.Draw(spriteBatch, new Rectangle((int)Camera.Position.X, (int)Camera.Position.Y, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), Camera.Position);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (Consumable consumable in consumables)
                consumable.Draw(gameTime, spriteBatch);
            
            Hero.Draw(gameTime, spriteBatch);

            spriteBatch.End();

        }//end Draw method

        #endregion
    }
}
