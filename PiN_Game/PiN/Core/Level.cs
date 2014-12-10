#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Program Platform
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

namespace PiN
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the the activeHero and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Map map;
        
        //The active hero
        // This is the hero that gets drawn and is updated
        public Hero ActiveHero
        {
            get { return activeHero; }
            set { this.activeHero = value; }
        }
        private Hero activeHero;

        // list to store the three different types of heroes
        public Hero[] Heroes = new Hero[3];

        //list to store all health and invincibility consumables
        private List<Consumable> consumables = new List<Consumable>();
        //enemies list to store information about enemies
        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        private AnimationLoader exitSprite;
        private Rectangle exitRectangle;

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

        public bool GameOver
        {
            get { return !Heroes[0].IsAlive && !Heroes[1].IsAlive && !Heroes[2].IsAlive; }
        }

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

        public int LevelIndex
        {
            get { return levelIndex; }
            set { LevelIndex = value; }
        }

        private int levelIndex = -1;

        //for debugging
        bool drawNavMesh;
        //Vector2 playersTarget;
        //float? rayIntersectDistance;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        public Level(IServiceProvider serviceProvider, Map currentMap, Camera camera, int lvlIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            timeRemaining = TimeSpan.FromMinutes(15.0); //changed the time limit to 15 minutes for longer level testing

            levelIndex = lvlIndex;

            map = currentMap;

            GlobalSolver.LoadMesh(map.NavMesh);
            Collision.LoadLevel(this);

            LoadHero();

            LoadEnemies();

            LoadExit();
           
            cam = camera;
            cam.Limits = new Rectangle(0, 0, map.Width * map.TileWidth, map.Height * map.TileHeight);//defining world limits

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Instantiates an activeHero, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private void LoadHero()
        {            
            if (ActiveHero != null)
                throw new NotSupportedException("A level may only have one starting point.");

            //where the player starts in the map
            start = RectangleExtensions.GetBottomCenter(GetBounds((int)map.StartTile.X,(int)map.StartTile.Y));

            Heroes[0] = new HeroStrength(this, new Vector2(-1,-1));
            Heroes[1] = new HeroSpeed(this, new Vector2(-1, -1));
            Heroes[2] = new HeroFlight(this, new Vector2(-1, -1));

            activeHero = (Hero)Heroes[1];
            activeHero.Position = start;
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private void LoadExit()
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(map.ExitTile.X,map.ExitTile.Y).Center;
            exitSprite.LoadAnimation(new Animation(Content.Load<Texture2D>("Sprites/Player/ExitPortal"), 0.1f, true));
            exitRectangle = new Rectangle(exit.X - exitSprite.Animation.FrameWidth / 2 + 20, exit.Y - exitSprite.Animation.FrameHeight + 20, exitSprite.Animation.FrameWidth - 40, exitSprite.Animation.FrameHeight - 40);
        }

        /// <summary>
        /// gets all the enemies from the level.
        /// </summary>
        private void LoadEnemies()
        {
            foreach (var enemy in map.Enemies)
                SpawnEnemy(enemy.x, enemy.y, enemy.type);
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
        /// This method handles tiles outside of the map boundaries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= map.Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= map.Height)
                return TileCollision.Passable;

            return map.Tiles[y,x].Collision;
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

        /// <summary>
        /// list of platforms in this level
        /// </summary>
        public List<Platform> Platforms
        {
            get { return map.Platforms; }
        }

        /// <summary>
        /// list of Tiles in this level
        /// </summary>
        public Tile[,] Tiles
        {
            get { return map.Tiles; }
        }
        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit.
        /// </summary>
        public void Update(GameTime gameTime, InputHandler gameInputs)
        {

            //switching characters in the air screws up the physics sometimes
            //This is a work around, but I would like to fix the bug
            if (activeHero.IsOnGround && activeHero.IsAlive)
                SwapHeroes(gameInputs);


            // Pause while the activeHero is dead or time is expired.
            if (!ActiveHero.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the activeHero.
                ActiveHero.PhysicsEngine.ApplyPhysics(gameTime);

                ((Gun)Heroes[0].Weapon).UpdateBullets();
                ((Gun)Heroes[1].Weapon).UpdateBullets();
                ((Gun)Heroes[2].Weapon).UpdateBullets();
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
                ActiveHero.Update(gameTime, gameInputs);

                if (ActiveHero is HeroStrength)
                {
                    ((Gun)Heroes[1].Weapon).UpdateBullets();
                    ((Gun)Heroes[2].Weapon).UpdateBullets();
                }
                else if (ActiveHero is HeroSpeed)
                {
                    ((Gun)Heroes[0].Weapon).UpdateBullets();
                    ((Gun)Heroes[2].Weapon).UpdateBullets();
                }
                else
                {
                    ((Gun)Heroes[0].Weapon).UpdateBullets();
                    ((Gun)Heroes[1].Weapon).UpdateBullets();
                }
                    
                UpdateConsumables(gameTime);

                //playersTarget = gameInputs.MouseInput.Position;
                //rayIntersectDistance = Collision.RayCastCollidesWithLevel(ActiveHero.Center, playersTarget);


                if (gameInputs.MouseState.ScrollWheelValue > gameInputs.PreviousMouseState.ScrollWheelValue)
                    Camera.Zoom += 0.1f;
                else if (gameInputs.MouseState.ScrollWheelValue < gameInputs.PreviousMouseState.ScrollWheelValue)
                    Camera.Zoom -= 0.1f;


                //follow the activeHero
                Camera.LookAt(ActiveHero.Position);

                

                // Falling off the bottom of the level kills the activeHero.
                if (ActiveHero.BoundingRectangle.Top >= Height * map.TileHeight && ActiveHero.IsAlive)
                {
                    OnHeroKilled(null);
                }

                UpdateEnemies(gameTime, gameInputs);
                   

                // The activeHero has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.

                if (ActiveHero.IsAlive &&
                    ActiveHero.IsOnGround &&
                    ActiveHero.BoundingRectangle.Intersects(exitRectangle))
                {
                    OnExitReached();
                }
            }
            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;

            if (gameInputs.KeyboardState.IsKeyDown(Keys.N) && !gameInputs.PreviousKeyboardState.IsKeyDown(Keys.N))
            {
                if (drawNavMesh)
                    drawNavMesh = false;
                else
                    drawNavMesh = true;
            }

            //GOD MODE!!!
            //ActiveHero.Health = ActiveHero.MaxHealth;
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
        private void UpdateEnemies(GameTime gameTime, InputHandler gameInputs)
        {
            foreach (Enemy enemy in enemies)
            {
                if ((!enemy.IsAlive || TimeRemaining == TimeSpan.Zero) && enemy.BoundingRectangle.Top <= Height * TileHeight)
                {
                    // Still want to perform physics on the enemy.
                    enemy.PhysicsEngine.ApplyPhysics(gameTime);
                    
                }

                if (!enemy.IsAlive)
                    ((Gun)enemy.Weapon).UpdateBullets();


                enemy.Update(gameTime, gameInputs);
                // Touching an enemy decreases health of the activeHero

                if (enemy.IsAlive & enemy.BoundingRectangle.Intersects(ActiveHero.BoundingRectangle))
                {
                    if (ActiveHero.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, ActiveHero); //enemy dies instantly when you are in invincibility mode
                    }
                    else if (!ActiveHero.IsHit)
                    {
                        OnHeroHit(enemy);
                    }
                }
                else if (enemy.BoundingRectangle.Top >= Height * map.TileHeight && enemy.IsAlive)
                {
                    OnEnemyKilled(enemy,null);
                }
                
            }
        }

        /// <summary>
        /// Handles swapping between the three main characters.
        /// </summary>
        /// <param name="gameInputs">The current inputs.</param>
        public void SwapHeroes(InputHandler gameInputs)
        {
            if (gameInputs.KeyboardState.IsKeyDown(Keys.D1))
            {
                if (!activeHero.Equals(Heroes[0]) && Heroes[0].IsAlive)
                {
                    Heroes[0].SwapIn();
                    activeHero = (Hero)Heroes[0];
                }
                
            }
            else if (gameInputs.KeyboardState.IsKeyDown(Keys.D2))
            {
                if (!activeHero.Equals(Heroes[1]) && Heroes[1].IsAlive)
                {
                    Heroes[1].SwapIn();
                    activeHero = (Hero)Heroes[1];
                }
            }
            else if (gameInputs.KeyboardState.IsKeyDown(Keys.D3))
            {
                if (!activeHero.Equals(Heroes[2]) && Heroes[2].IsAlive)
                {
                    Heroes[2].SwapIn();
                    activeHero = (Hero)Heroes[2];
                }
            }
        }


        private void OnEnemyKilled(Enemy enemy, Hero killedBy)
        {
            enemy.OnKilled(killedBy);
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
        private void OnHeroHit(GameCharacter hitBy)
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
            if (!activeHero.Equals(Heroes[0]) && Heroes[0].IsAlive)
            {
                Heroes[0].SwapIn();
                activeHero = (Hero)Heroes[0];
            }
            else if (!activeHero.Equals(Heroes[1]) && Heroes[1].IsAlive)
            {
                Heroes[1].SwapIn();
                activeHero = (Hero)Heroes[1];
            }
            else if (!activeHero.Equals(Heroes[2]) && Heroes[2].IsAlive)
            {
                Heroes[2].SwapIn();
                activeHero = (Hero)Heroes[2];
            }
            else
                return;
            
            ActiveHero.Reset(start);
        }

        /// <summary>
        /// Restores a hero to full health
        /// </summary>
        public void HealOneHero()
        {
            if (!Heroes[0].IsAlive)
                Heroes[0].Reset(Heroes[0].Position);
            else if (!Heroes[1].IsAlive)
                Heroes[1].Reset(Heroes[1].Position);
            else if (!Heroes[2].IsAlive)
                Heroes[2].Reset(Heroes[2].Position);
            else
            {
                int rand = random.Next(0,Heroes.Count());
                rand = (int)MathHelper.Clamp(rand, 0, Heroes.Count()-1);
                Heroes[rand].Health = Heroes[rand].MaxHealth;
            }
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
            
            map.Draw(spriteBatch, Camera, drawNavMesh);

            exitSprite.Draw(gameTime, spriteBatch, new Vector2(exit.X,exit.Y), SpriteEffects.None, Color.White);

            //draw each of the enemies in the enemies list
            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(gameTime, spriteBatch);
                ((Gun)enemy.Weapon).DrawBullets(gameTime, spriteBatch);
                if (enemy.IsAlive)
                {
                    //XnaDebugDrawer.DebugDrawer.DrawRectangle(spriteBatch, enemy.BoundingRectangle, Color.Red, 1);
                    spriteBatch.DrawString(Hud.hudFont, enemy.Health.ToString(), new Vector2(enemy.BoundingRectangle.X, enemy.BoundingRectangle.Y - 20), Color.Black);

                    //if (enemy.lineIntersectDistance != null)
                    //{
                    //    Vector2 direction = enemy.Target - enemy.Center;
                    //    direction.Normalize();
                    //    XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, enemy.Center,
                    //        new Vector2(enemy.Center.X + direction.X * (float)enemy.lineIntersectDistance, enemy.Center.Y + direction.Y * (float)enemy.lineIntersectDistance), Color.Red, 3);
                    //}

                    //if (enemy.Path != null)
                    //{
                    //    foreach (GraphNode<Platform> gNode in enemy.Path)
                    //    {
                    //        Vector2 center = new Vector2((gNode.Value.RightEdgeX + gNode.Value.LeftEdgeX) / 2, gNode.Value.Y);
                    //        XnaDebugDrawer.DebugDrawer.DrawCircle(spriteBatch, center, 8, Color.Red, 5);

                    //        foreach (GraphNode<Platform> neighbor in gNode.Neighbors)
                    //        {
                    //            Vector2 neighborCenter = new Vector2((neighbor.Value.RightEdgeX + neighbor.Value.LeftEdgeX) / 2, neighbor.Value.Y);
                    //            XnaDebugDrawer.DebugDrawer.DrawCircle(spriteBatch, neighborCenter, 8, Color.Red, 5);
                    //            XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, center, neighborCenter, Color.Red, 5);
                    //        }

                    //    }
                    //}
                }
                
            }
                

            //draw each consumable in the consumables array
            foreach (Consumable consumable in consumables)
                consumable.Draw(gameTime, spriteBatch);

            //draw the active hero
            ActiveHero.Draw(gameTime, spriteBatch);

            if (ActiveHero is HeroStrength)
            {
                ((Gun)Heroes[1].Weapon).DrawBullets(gameTime,spriteBatch);
                ((Gun)Heroes[2].Weapon).DrawBullets(gameTime, spriteBatch);
            }
            else if (ActiveHero is HeroSpeed)
            {
                ((Gun)Heroes[0].Weapon).DrawBullets(gameTime, spriteBatch);
                ((Gun)Heroes[2].Weapon).DrawBullets(gameTime, spriteBatch);
            }
            else
            {
                ((Gun)Heroes[0].Weapon).DrawBullets(gameTime, spriteBatch);
                ((Gun)Heroes[1].Weapon).DrawBullets(gameTime, spriteBatch);
            }
            //XnaDebugDrawer.DebugDrawer.DrawRectangle(spriteBatch, ActiveHero.BoundingRectangle, Color.Red, 1);

            //Collision.Draw(spriteBatch);
            //if (rayIntersectDistance != null)
            //{
            //    Vector2 direction = playersTarget - ActiveHero.Center;
            //    direction.Normalize();
            //    XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, ActiveHero.Center,
            //        new Vector2(ActiveHero.Center.X + direction.X * (float)rayIntersectDistance, ActiveHero.Center.Y + direction.Y * (float)rayIntersectDistance), Color.Red, 3);
            //}
                

            spriteBatch.End();

        }//end Draw method

        #endregion
    }
}
