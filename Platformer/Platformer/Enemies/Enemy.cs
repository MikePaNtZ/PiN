#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        public float Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }


        public bool IsAlive { get; set; }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        public Animation explosionAnimation;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect enemyHurtSound;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;


        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 40.0f; //changed from 64 to 40f

        // Attributes intended to be overwritten in derived class.
        protected EnemyState state;
        protected float health;
        protected string enemyType;

        protected const float MaxTrackDistance = 500.0F; //player is within this distance than you can track
        protected const float MaxAttackDistance = 200.0F; //player is within this distance than you can attack
        protected const float KamikazeThresholdPercent = 20.0F; //health is less than this percent of health than kamikaze

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position)
       { 
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.state = EnemyState.Search;
        }

        /// <summary>
        /// Loads a particular enemy resetAfterHit sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animations.
            string spriteSet = "Sprites/" + enemyType + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            explosionAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/explosion"), 0.1f, false); //false means the animation is not going to loop
            sprite.PlayAnimation(idleAnimation);

            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");
            // Temporary hurt sound. We probably want to use something different in the future.
            enemyHurtSound = killedSound;

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.9); //gets enemy closer to the outer edge of the shield
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }
        

        /// <summary>
        /// Called when the enemy has been hit.
        /// </summary>
        public void OnHit()
        {

            health -= 1.0f;
            enemyHurtSound.Play();
            if (health <= 0)
                OnKilled();
        }
        


        /// <summary>
        /// Updates the ai and position of enemy
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!IsAlive)
                return;

            Vector2 lineOfSight = getLineOfSight(); //line of sight to player

            UpdateState(gameTime, lineOfSight); //changes the state if need be

            //These different methods define the enemy's actions for this frame
            switch (state)
            {
                case EnemyState.Search:
                    Search(gameTime);
                    break;
                case EnemyState.Track:
                    Track(gameTime, lineOfSight);
                    break;
                case EnemyState.Attack:
                    Attack(gameTime, lineOfSight);
                    break;
                case EnemyState.Kamikaze:
                    Kamikaze(gameTime, lineOfSight);
                    break;
            }
            
        }

        /// <summary>
        /// changes the state of the enemy depending on the distance to the player and health
        /// </summary>
        protected void UpdateState(GameTime gameTime, Vector2 lineOfSight)
        {
            switch (state)
            {
                case EnemyState.Search:
                    //if health is lower than threshold then kamikaze
                    if (health <= health / KamikazeThresholdPercent)
                        state = EnemyState.Kamikaze;
                    else if (lineOfSight.X * (int)direction > 0) //make sure enemy is facing the right direction
                    {
                        if (Math.Abs(lineOfSight.X) <= MaxAttackDistance)// player is in attacking distance then attack
                            state = EnemyState.Attack;
                        else if (Math.Abs(lineOfSight.X) <= MaxTrackDistance)//or at least in tracking distance then track
                            state = EnemyState.Track;
                    }
                    break;

                case EnemyState.Track:
                    //if health is lower than threshold than kamikaze
                    if (health <= health / KamikazeThresholdPercent)
                        state = EnemyState.Kamikaze;
                    else if (Math.Abs(lineOfSight.X) <= MaxAttackDistance)// player is in attacking distance then attack
                        state = EnemyState.Attack;

                    Track(gameTime, lineOfSight);
                    break;

                case EnemyState.Attack:
                    // if health is lower than threshold than kamikaze
                    if (health <= health / KamikazeThresholdPercent)
                        state = EnemyState.Kamikaze;
                    else if (Math.Abs(lineOfSight.X) > MaxAttackDistance)// player moved outside of attacking range then track
                        state = EnemyState.Track;
                    break;

                case EnemyState.Kamikaze:
                    //nothing to change to
                    break;

            }
        }

        /// <summary>
        /// Searches by pacing back and forth along a platform, waiting at either end.
        /// </summary>
        protected void Search(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / level.TileWidth) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / level.TileHeight);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }
        }

        /// <summary>
        /// Moves toward player, is still too far away to attack
        /// </summary>
        protected void Track(GameTime gameTime, Vector2 lineOfSight)
        {

        }

        /// <summary>
        /// Attacking player, by shooting
        /// </summary>
        protected void Attack(GameTime gameTime, Vector2 lineOfSight)
        {

        }

        /// <summary>
        /// Charges at player
        /// </summary>
        protected void Kamikaze(GameTime gameTime, Vector2 lineOfSight)
        {

        }

        /// <summary>
        /// returns line of sight to player
        /// </summary>
        private Vector2 getLineOfSight()
        {
            return level.Player.Position - position;
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!IsAlive)
            {
                //sprite.PlayAnimation(explosionAnimation); //doesn't work for some reason
                sprite.PlayAnimation(dieAnimation);//then play the enemy dying
            }
            //if player is not alive or if player hasn't reached the exit, or if the time
            //remaining is 0, or if waiting time is greater than 0
            //then the idle animation for the enemies is playing
            else if (!Level.Player.IsAlive ||
                      Level.ReachedExit ||
                      Level.TimeRemaining == TimeSpan.Zero ||
                      waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                //if none of the above, then enemies are running
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public void OnKilled()
        {
            IsAlive = false;
            killedSound.Play();

        }

    }
}
