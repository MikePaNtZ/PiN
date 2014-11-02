
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
        /// Game character constructor without a base texture
        /// TODO Tom is right about the base class taking a texture to its constructor.
        ///      We need to fix this up. Providing multiple constructors is a hack work around
        ///      that should be temporary.
        /// </summary>
        public GameCharacter(Level level, Vector2 initialPosition) : base()
        {
            Position = initialPosition;
            this.level = level;
            // construct the physics engine.
            this.physEngine = new PhysicsEngine(this);
            Reset(initialPosition);
        }

        /// <summary>
        /// Game character constructor
        /// </summary>
        public GameCharacter(Level level, Vector2 initialPosition, Texture2D loadedTexture) : base(loadedTexture)
        {
            Position = initialPosition;
            this.level = level;
            // construct the physics engine.
            this.physEngine = new PhysicsEngine(this);
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

        public override Vector2 Center
        {
            get { return new Vector2(Position.X + sprite.Animation.Texture.Width/2, Position.Y - sprite.Animation.Texture.Height); }
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
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);


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
        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
            physEngine.ApplyPhysics(gameTime);

            // determine the animation to use based on player action.
            determineAnimation(gameTime);

            // Clear inputs.
            movement = 0.0f;
            IsJumping = false;

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

        }//end Update method


        /// <summary>
        /// Called when the game character has been hit.
        /// </summary>
        public virtual void OnHit(GameCharacter hitBy)
        {
            IsHit = true;
            if (hitBy != null)
            {
                UpdateHealth(-25);
                hurtSound.Play();
            }
            else
            {
                UpdateHealth(-5);
                hurtSound.Play();
            }

            if (Health <= 0)
                OnKilled(hitBy);

        }
        

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
        public virtual void OnKilled(GameCharacter killedBy)
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

        // tint color drawn on top of game character.
        protected Color color;

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
        protected const float MaxPowerUpTime = 10.0f; //maximum power up time is 10 seconds
        // Jumping state
        private bool isJumping;
        //private bool isSwapping;
        private bool isAttacking;
        private bool isBlocking; //is player using his force field shield
        // Power up colors
        protected readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };
        protected readonly Color[] isHitColors = {
                               Color.Transparent,
                               Color.Beige,
                               Color.Transparent,
                               Color.Beige,
                        
                                               };

    }
}