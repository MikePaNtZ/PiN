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
    /// The level owns the the activeHero and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        //possible health bar
        //private Texture2D healthBar;
        //private Vector2 healthBarLoc;
        //private Texture2D healthTexture;


        // Physical structure of the level.
        private Map map;

        // reordered list of tilesets. ordered by first tile id
        private Dictionary<string, Tileset> tilesets;

        // Entities in the level.
        //public Hero ActiveHero
        //{
        //    get { return activeHero; }
        //    //set { ActiveHero = activeHero; }
        //}
        private Hero activeHero;
        public Hero ActiveHero
        {
            get { return activeHero; }
            set { this.activeHero = value; }
        }


        // list to store the three different types of heroes
        private Hero[] Hero = new Hero[3];

        //list to store all health and invincibility consumables
        private List<Consumable> consumables = new List<Consumable>();
        //enemies list to store information about enemies
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
            timeRemaining = TimeSpan.FromMinutes(10.0); //changed the time limit to 6 minutes for longer level testing
            
            map = currentMap;
            
            LoadMap();
           
            cam = camera;
            cam.Limits = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);//defining world limits

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
            //possible health bar
            //healthBar = Content.Load<Texture2D>("Sprites/Player/healthbar");
            //healthTexture = Content.Load<Texture2D>("Sprites/Player/health");
            //healthBarLoc = new Vector2(Width * TileWidth - 205, 5);//location for the health bar
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
        /// Instantiates an activeHero, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private void LoadHero()
        {            
            if (ActiveHero != null)
                throw new NotSupportedException("A level may only have one starting point.");

            int x = map.ObjectGroups["events"].Objects["player"].X;
            int y = map.ObjectGroups["events"].Objects["player"].Y;

            //where the player starts in the Tiled map editor map
            start = RectangleExtensions.GetBottomCenter(GetTileAtPoint(x,y));

            activeHero = new Hero(this, start, this.Content.Load<Texture2D>("Sprites/Player/Idle"));

        }


        /****************************************************BAD SWAP HERO CODE*****************************************************/

        //public void SwapHeroes(Hero activeHero)
        //{
        //    KeyboardState keyboard = new KeyboardState();

        //    if (keyboard.IsKeyDown(Keys.R))
        //    {
        //        //activeHero = Hero[0];
        //        //activeHero = new HeroStrength(this, activeHero.Position, this.Content.Load<Texture2D>("Sprites/HeroStrength/Idle"));
        //        HeroStrength Kaeden = new HeroStrength(this, ActiveHero.Position, this.Content.Load<Texture2D>("Sprites/HeroStrength/Idle"));
        //        Hero[0] = (HeroStrength)Kaeden;
        //        activeHero = Hero[0];
        //    }
        //    else if (keyboard.IsKeyDown(Keys.F))
        //    {
        //        //activeHero = Hero[1];
        //        //activeHero = new HeroSpeed(this, activeHero.Position, this.Content.Load<Texture2D>("Sprites/HeroSpeed/Idle"));
        //        HeroSpeed Sammie = new HeroSpeed(this, ActiveHero.Position, this.Content.Load<Texture2D>("Sprites/HeroSpeed/Idle"));
        //        Hero[1] = (HeroSpeed)Sammie;
        //        activeHero = Hero[1];
        //    }
        //    else if (keyboard.IsKeyDown(Keys.C))
        //    {
        //        //activeHero = Hero[2];
        //        //activeHero = new HeroFlight(this, activeHero.Position, this.Content.Load<Texture2D>("Sprites/HeroFlight/Idle"));
        //        HeroFlight Aidan = new HeroFlight(this, ActiveHero.Position, this.Content.Load<Texture2D>("Sprites/HeroFlight/Idle"));
        //        Hero[2] = (HeroFlight)Aidan;
        //        activeHero = Hero[2];
        //    }
        //    else
        //        activeHero = new Hero(this, start, this.Content.Load<Texture2D>("Sprites/Player/Idle"));
        //}//SwapHeroes method

        /****************************************************************DOESN'T WORK****************************************************/

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

            /******************************************SWAPHEROES CALL*********************************/
            //SwapHeroes(activeHero);
            /********************************************END SWAPHEROES*********************************/


            // Pause while the activeHero is dead or time is expired.
            if (!ActiveHero.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the activeHero.
                ActiveHero.PhysicsEngine.ApplyPhysics(gameTime);

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
                //SwapHeroes(activeHero);
                ActiveHero.Update(gameTime, gameInputs);
                UpdateConsumables(gameTime);

                //follow the activeHero
                Camera.LookAt(ActiveHero.Position);

                // Falling off the bottom of the level kills the activeHero.
                if (ActiveHero.BoundingRectangle.Top >= Height * map.TileHeight)
                {
                    OnHeroKilled(null);
                }

                UpdateEnemies(gameTime);
                   

                // The activeHero has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.

                if (ActiveHero.IsAlive &&
                    ActiveHero.IsOnGround &&
                    ActiveHero.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        /// <summary>
        /// Animates each consumable and checks to allows the activeHero to collect them.
        /// </summary>
        private void UpdateConsumables(GameTime gameTime)
        {
            for (int i = 0; i < consumables.Count; ++i)
            {
                Consumable consumable = consumables[i];

                consumable.Update(gameTime);

                //if active hero intersects with the consumable, then the consumable is removes from consumables array
                if (consumable.BoundingCircle.Intersects(ActiveHero.BoundingRectangle))
                {
                    consumables.RemoveAt(i--);
                    OnConsumableCollected(consumable, ActiveHero);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the activeHero.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);
                // Touching an enemy decreases health of the activeHero

                if (enemy.IsAlive & enemy.BoundingRectangle.Intersects(ActiveHero.BoundingRectangle))
                {
                    if (ActiveHero.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, ActiveHero); //enemy dies instantly when you are in invincibility mode
                    }
                    else if (ActiveHero.IsBlocking)
                    {
                        //OnEnemyKilled(enemy, ActiveHero);
                        if (ActiveHero.Position.X == enemy.Position.X)
                        {
                            enemy.Position = new Vector2(enemy.Position.X + 100, enemy.Position.Y);
                            ActiveHero.UpdateHealth(8);
                            //really just to offset the tight bounding box of player
                            //player shouldn't be losing health when using shield
                            //now player just loses some health when using shield against enemy
                            //this implies the shield helped reduce some of the damage from the enemy
                        }
                        else if (ActiveHero.Position.X < enemy.Position.X)
                        {
                            enemy.Position = new Vector2(enemy.Position.X + 100, enemy.Position.Y);
                            ActiveHero.UpdateHealth(8);
                        }
                        else if (ActiveHero.Position.X > enemy.Position.X)
                        {
                            enemy.Position = new Vector2(enemy.Position.X - 100, enemy.Position.Y);
                            ActiveHero.UpdateHealth(8);
                        }
                        else
                        {
                            enemy.Position = new Vector2(enemy.Position.X + 100, enemy.Position.Y);
                            ActiveHero.UpdateHealth(8);
                        }
                    }
                    else if (!ActiveHero.IsHit)
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
        /// <param name="collectedBy">The activeHero who collected this gem.</param>
        private void OnConsumableCollected(Consumable consumable, Hero collectedBy)
        {
            consumable.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the activeHero is hit by an enemy.
        /// </summary>
        /// <param name="hitBy">
        /// The enemy who hit the activeHero. This is null if the activeHero was not hit by an
        /// enemy, such as when a activeHero hits or is hit by a hazard.
        /// </param>
        private void OnHeroHit(Enemy hitBy)
        {
            ActiveHero.OnHit(hitBy);
        }

        /// <summary>
        /// Called when the activeHero is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the activeHero. This is null if the activeHero was not killed by an
        /// enemy, such as when a activeHero falls into a hole.
        /// </param>
        private void OnHeroKilled(Enemy killedBy)
        {
            ActiveHero.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the activeHero reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            ActiveHero.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the activeHero to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            ActiveHero.Reset(start);
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
            
            //draw each of the enemies in the enemies list
            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            //draw each consumable in the consumables array
            foreach (Consumable consumable in consumables)
                consumable.Draw(gameTime, spriteBatch);
            //draw the active hero
            ActiveHero.Draw(gameTime, spriteBatch);


            /*********************************************WHERE HEALTH BAR SHOULD BE DRAWN**************************************/

            //draw health bar
            //spriteBatch.Draw(healthBar, healthBarLoc, Color.White);
            //spriteBatch.Draw(healthTexture, new Rectangle((int)healthBarLoc.X + 1, (int)healthBarLoc.Y + 1, ActiveHero.Health * 2 - 2, 30), Color.White);
            /*************************************************END HEALTH BAR CODE**********************************************/
            spriteBatch.End();

        }//end Draw method

        #endregion
    }
}
