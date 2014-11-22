
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
 
namespace PiN
{

    public enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// Base class for all game characters. Both Heroes and Enemies derive from this base class.
    /// </summary>
    class GameCharacter : GameObject
    {
        //**********************PROPERTIES*****************//
        //*************************************************//
        public Level Level
        {
            get { return level; }
        }

        public PhysicsEngine PhysicsEngine
        {
            get { return physEngine; }
        }

        public AnimationLoader Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        public FaceDirection FaceDirection = FaceDirection.Right;

        public override Vector2 Center
        {
            get 
            {
                return new Vector2(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);
            }
        }

        public Platform CurrentPlatform
        {
            get { return currentPlatform; }
        }
        protected Platform currentPlatform;

        /// <summary>
        /// Gets whether or not the character's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
            set { this.isOnGround = value; }
        }

        public int Health
        {
            get { return health; }
            set { health = (int)MathHelper.Clamp(value, 0, MaxHealth); }
        }

        public virtual int MaxHealth
        {
            get { return 100; }
        }

        public virtual bool IsJumping { get { return false; } }
        public virtual bool IsAttacking { get { return false; } }

        public override bool IsAlive
        {
            get { return stateMachine.MainState.GetType() != typeof(DeadState); }
            set { ; }
        }

        public float Movement
        {
            get { return movement; }
            set { movement = value; }
        }

        public float MoveSpeed
        {
            get { return moveSpeed; }
        }

        /// <summary>
        /// Gets a rectangle which bounds this character in world space.
        /// </summary>
        public virtual Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public override Rectangle rectangle
        {
            get
            {
                int left = (int)position.X - sprite.Animation.FrameWidth/2;
                int width = sprite.Animation.FrameWidth;
                int top = (int)position.Y - sprite.Animation.FrameHeight;
                int height = sprite.Animation.FrameHeight;
                return new Rectangle(left, top, width, height);
            }
        }

        public virtual Vector2 ArmPosition
        {
            get
            {
                return new Vector2(rectangle.X + rectangle.Width / 2, 
                                   rectangle.Y + rectangle.Height / 2);
            }
        }

        public Weapon Weapon
        {
            get { return weapon; }
            set { weapon = value; }
        }

        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }

        public float PowerUpTime
        {
            get { return powerUpTime; }
            set { powerUpTime = value; }
        }
        

        public SoundEffect KilledSound { get { return killedSound; } }
        public SoundEffect JumpSound { get { return jumpSound; } }
        public SoundEffect FallSound { get { return fallSound; } }
        public SoundEffect HurtSound { get { return hurtSound; } }
        public SoundEffect PowerUpSound { get { return powerUpSound; } }

        public Animation IdleAnimation { get { return idleAnimation; } }
        public Animation RunAnimation { get { return runAnimation; } }
        public Animation JumpAnimation { get { return jumpAnimation; } }
        public Animation CelebrateAnimation { get { return celebrateAnimation; } }
        public Animation DieAnimation { get { return dieAnimation; } }
        public Animation ShieldAnimation { get { return shieldAnimation; } }

        public SpriteEffects Flip
        {
            get 
            {
                if (FaceDirection == FaceDirection.Right)
                    return SpriteEffects.FlipHorizontally;
                else
                    return SpriteEffects.None;
            }
        }


        //**********************METHODS********************//
        //*************************************************//

        /// <summary>
        /// Program character constructor
        /// </summary>
        public GameCharacter(Level level, Vector2 initialPosition)
            : base()
        {
            Position = initialPosition;

            this.level = level;
            // construct the physics engine.
            this.physEngine = new PhysicsEngine(this);

            Reset(initialPosition);
        }

        /// <summary>
        /// Resets the character to life.
        /// </summary>
        public override void Reset(Vector2 position)
        {
            base.Reset(position);
            FindCurrentPlatform();
            if (stateMachine != null)
                stateMachine.Reset();
            Velocity = Vector2.Zero;
            health = MaxHealth;
            powerUpTime = 0.0f;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player resetAfterHit.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
            physEngine.ApplyPhysics(gameTime);

            FindCurrentPlatform();

            stateMachine.Update(gameTime, gameInputs);

        }//end Update method


        /// <summary>
        /// Called when the game character has been hit.
        /// </summary>
        public virtual void OnHit(GameObject hitBy)
        {
            stateMachine.OnHit(hitBy);
        }

        /// <summary>
        /// Moves the character left or right
        /// </summary>
        public void Move(FaceDirection direction)
        {
            FaceDirection = direction;
            movement = 1 * (int)direction;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public virtual void OnKilled(GameObject killedBy)
        {
            stateMachine.OnKilled(killedBy);
        }

        protected virtual void FindCurrentPlatform()
        {
            if (IsOnGround)
            {
                Platform previous = currentPlatform;
                currentPlatform = Level.Platforms.FirstOrDefault(platform => platform.Y == position.Y && platform.LeftEdgeX <= position.X && platform.RightEdgeX >= position.X);
                if (currentPlatform == null) //default values
                    currentPlatform = previous;
            }
        }

        public virtual void determineColor(GameTime gameTime)
        {
            if (IsPoweredUp)
            {
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
                int colorIndex = (int)t % poweredUpColors.Length;
                color = poweredUpColors[colorIndex];
            }
            else
            {
                color = Color.White;
            }
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            stateMachine.Draw(gameTime, spriteBatch);
            if (currentPlatform != null)
            {
                Vector2 left = new Vector2(currentPlatform.LeftEdgeX, currentPlatform.Y);
                Vector2 right = new Vector2(currentPlatform.RightEdgeX, currentPlatform.Y);
                XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, left, right, Color.Green, 5);
                XnaDebugDrawer.DebugDrawer.DrawCircle(spriteBatch, left, 5, Color.Green, 5);
                XnaDebugDrawer.DebugDrawer.DrawCircle(spriteBatch, right, 5, Color.Green, 5);
            }
        }

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }

        //**********************VARIABLES********************//
        //*************************************************//

        // The physics engine that updates the position and velocity of this game character.
        protected PhysicsEngine physEngine;
        // All game characters belong to, at any given time, a single level. This object keeps track of which level this player is in,
        // and can be used to access other game objects that are also part of this level.
        private Level level;

        protected CharacterStateMachine stateMachine;

        // The game character sprite is an instance of an animation loader. The OnDraw routine will render this animation using the Animation Loader.
        public AnimationLoader sprite;
        // Animations
        protected Animation idleAnimation;
        protected Animation runAnimation;
        protected Animation jumpAnimation;
        protected Animation celebrateAnimation;
        protected Animation dieAnimation;
        protected Animation flinchAnimation;
        protected Animation shieldAnimation;
        protected Animation shieldPart1Animation;
        // Sound Effects
        protected SoundEffect killedSound;
        protected SoundEffect jumpSound;
        protected SoundEffect fallSound;
        protected SoundEffect hurtSound;
        protected SoundEffect powerUpSound;
        protected float powerUpTime;
        // character movement term. applied to velocity in physics
        protected float movement = 0.0f;
        protected float moveSpeed;
        // Character health
        protected int health;
        
        // Character's weapon
        protected Weapon weapon;
        /// <summary>
        /// Flag indicating the game character is on solid ground.
        /// </summary>
        private bool isOnGround;
        // Local rectangle boundary.
        protected Rectangle localBounds;
        // Powerup state
        protected float MaxPowerUpTime = 10.0f; //maximum power up time is 10 seconds
        // Power up colors
        protected readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };
        
    }
}