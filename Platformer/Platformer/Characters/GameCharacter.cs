
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
 
namespace Platformer
{

    /// <summary>
    /// Base class for all game characters. Both Heroes and Enemies derive from this base class.
    /// </summary>
    class GameCharacter : GameObject
    {

        /// <summary>
        /// Game character constructor
        /// </summary>
        public GameCharacter(Level level, Vector2 initialPosition, Texture2D loadedTexture) : base(loadedTexture)
        {
            Position = initialPosition;
            this.level = level;
            // construct the physics engine.
            this.physEngine = new PhysicsEngine(this);
            LoadContent();
            Reset(initialPosition);
        }

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
        }

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
        }

        public float Movement
        {
            get { return movement; }
        }

        public bool IsHit
        {
            get { return isHit; }
            set { this.isHit = value; }
        }

        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
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

        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }

        public bool IsAttacking
        {
            get { return isAttacking; }
            set { this.isAttacking = value; }
        }

        public bool IsBlocking
        {
            get { return isBlocking; }
            set { this.isBlocking = value; }
        }

      

        //public bool IsSwapping
        //{
        //    get { return isSwapping; }
        //    set { this.isSwapping = value; }
        //}

        public bool IsJumping
        {
            get { return isJumping; }
            set { this.isJumping = value; }
        }

        public SoundEffect JumpSound
        {
            get { return jumpSound; }
        }

        public Animation JumpAnimation
        {
            get { return jumpAnimation; }
        }

        public SpriteEffects Flip
        {
            get { return flip; }
            set { this.flip = value; }
        }

        /// <summary>
        /// Loads the game character.
        /// </summary>
        protected virtual void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            shieldPart1Animation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/ShieldPart1"), 0.1f, true);
            shieldAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Shield"), 0.1f, true); //load image for the shield
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            flinchAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false); //placeholder

            // Calculate bounds within texture size.            
            // TODO It needs to be more clear what this is doing, and why it is done here. It is for collision detection.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");//placeholder
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/Powerup");

            // Load character's default weapon
            weapon = new Gun(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"), this);
        }


        /// <summary>
        /// Resets the player to life.
        /// </summary>
        public override void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            IsAlive = true;
            isHit = false;
            health = 100;
            sprite.LoadAnimation(idleAnimation);
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
        protected override void Update(GameTime gameTime)
        {
            physEngine.ApplyPhysics(gameTime);

            determineAnimation(gameTime);

            // Clear input.
            movement = 0.0f;
            isJumping = false;

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
        }//end Update method

        /*Your health is updated */
        public void UpdateHealth(int changeInHealth)
        {
            health += changeInHealth;
            if (health > 100)
                health = 100;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            IsAlive = false;

            if (killedBy != null)
            {
                health = 0;
                killedSound.Play();
            }
            else//
            {
                health = 0;
                fallSound.Play();
            }
            sprite.LoadAnimation(dieAnimation);
        }

        protected virtual void determineAnimation(GameTime gameTime)
        {
            Vector2 heroVelocity = Velocity;
            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.LoadAnimation(runAnimation);
                }
                else if (IsBlocking)
                {
                    heroVelocity.X = 0;
                    Velocity = heroVelocity;
                    IsJumping = false;
                    sprite.LoadAnimation(shieldAnimation);
                }
                else
                {
                    sprite.LoadAnimation(idleAnimation);
                }
                
            }
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the resetAfterHit to face the way we are moving.
            if (Velocity.X > 0)
                Flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                Flip = SpriteEffects.None;

            // Calculate a tint color based on power up state.
            Color color;
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

            // Draw that resetAfterHit.
            sprite.Draw(gameTime, spriteBatch, Position, Flip, color);

            // Shooting related drawing.
            if (IsAlive)
            {
                weapon.Draw(gameTime, spriteBatch);
            }
        }

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }

        // The physics engine that updates the position and velocity of this game character.
        protected PhysicsEngine physEngine;
        // All game characters belong to, at any given time, a single level. This object keeps track of which level this player is in,
        // and can be used to access other game objects that are also part of this level.
        private Level level;
        // The game character sprite is an instance of an animation loader. The OnDraw routine will render this animation using the Animation Loader.
        protected AnimationLoader sprite;
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
        // Character health
        protected int health;

        private SpriteEffects flip = SpriteEffects.None;
        // Character's weapon
        protected Weapon weapon;
        // Character has been hit flag
        private bool isHit;
        /// <summary>
        /// Flag indicating the game character is on solid ground.
        /// </summary>
        private bool isOnGround;
        // Local rectangle boundary.
        protected Rectangle localBounds;
        // Powerup state
        private const float MaxPowerUpTime = 10.0f; //maximum power up time is 10 seconds
        // Jumping state
        private bool isJumping;
        //private bool isSwapping;
        private bool isAttacking;
        private bool isBlocking; //is player using his force field shield
        // Power up colors
        private readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };

    }
}