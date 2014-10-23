#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
                            /******************************* CLASS VARIABLES ***********************************/
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation flinchAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;
        private Animation shieldAnimation;
        private Animation shieldPart1Animation;
        
        private Camera _cam;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        private SoundEffect hurtSound;

        // Shooting objects
        private GameObject arm;
        private GameObject crosshair;
        private GameObject[] bullets;
        private int MAX_BULLETS = 12;
        private MouseState oldMouseState;



        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        public int Health
        {
            get { return health; }
        }
        int health;

        public bool IsHit
        {
            get { return isHit; }
        }
        bool isHit;

        private const float MaxIFrames = 1.5F;
        private float IFrames;
        

        // Powerup state
        private const float MaxPowerUpTime = 10.0f; //maximum power up time is 10 seconds
        private float powerUpTime;
        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }
        private readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };
        private SoundEffect powerUpSound;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        Vector2 position;
        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;
        private bool isShooting;
        public bool isBlocking; //is player using his force field shield


        private Rectangle localBounds;
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

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;
            LoadContent();
            Reset(position);
        }

        /// <summary>
        /// Loads the player resetAfterHit sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            /*shieldPart1Animation is an image of just the shield alone; I couldn't get the shield to be overlayed onto the resetAfterHit
              I may have to do this later for the overdrive meter of the player*/
            shieldPart1Animation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/ShieldPart1"), 0.1f, true);
            shieldAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Shield"), 0.1f, true); //load image for the shield

            
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            flinchAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false); //placeholder

            // Calculate bounds within texture size.            
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

            // load shooting related object
            arm = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));
            crosshair = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Crosshair"));
            // load all bullets
            bullets = new GameObject[MAX_BULLETS];
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
            }
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            isHit = false;
            health = 100;
            IFrames = 0.0F;
            sprite.PlayAnimation(idleAnimation);
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
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            MouseState mouseState,
            Camera cam,
            GamePadState gamePadState,
            TouchCollection touchState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, mouseState, gamePadState, touchState, accelState, orientation);
            ApplyPhysics(gameTime);
            _cam = cam;

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    //resetAfterHit.PlayAnimation(shieldPart1Animation);
                    sprite.PlayAnimation(runAnimation);
                }

                else if (isBlocking) //if right-clicking mouse is true
                {
                    //movement = 0.0f; //if user clicks on right mouse button, then he cannot move
                    velocity.X = 0;
                    isJumping = false;
                    sprite.PlayAnimation(shieldAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }

                
            }

            if (IsAlive && isHit)
            {
                UpdateInvincibilityFrames(gameTime);
                spriteFlickerAnimation();
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

            // Shooting related updates
            MouseInput mouseInput = new MouseInput(_cam, mouseState);
            crosshair.position = mouseInput.Position;

            if (flip == SpriteEffects.FlipHorizontally)
                arm.position = new Vector2(position.X + 5, position.Y - 60);
            else
                arm.position = new Vector2(position.X - 5, position.Y - 60);

            // Updates the state of all bullets
            UpdateBullets();
        }//end Update method


        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState,
            MouseState mouseState,
            GamePadState gamePadState,
            TouchCollection touchState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }


            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if player is firing weapon
            isShooting = ((mouseState.LeftButton == ButtonState.Pressed) && (oldMouseState.LeftButton != ButtonState.Pressed));
            //the player is blocking by holding down the right mouse button
            isBlocking = (mouseState.RightButton == ButtonState.Pressed) & (oldMouseState.RightButton == ButtonState.Pressed);

            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W) ||
                touchState.AnyTouch();


            updateShooting(mouseState);
            oldMouseState = mouseState;
        }



        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
            {
                velocity.Y = 0;
                jumpTime = 0.0f;
            }
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;
            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / level.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / level.TileWidth)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / level.TileHeight);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / level.TileHeight)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform || velocity.Y > 500 && velocity.X == 0)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        private void FireBullet()
        {
            foreach (GameObject bullet in bullets)
            {
                //Find a bullet that isn't alive
                if (!bullet.alive)
                {
                    //And set it to alive.
                    bullet.alive = true;

                    if (flip == SpriteEffects.FlipHorizontally) //Facing right
                    {
                        float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                        float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                        //Set the initial position of our bullet at the end of our gun arm
                        //42 is obtained be taking the width of the Arm_Gun texture / 2
                        //and subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
                        bullet.position = new Vector2(
                            arm.position.X + 42 * armCos,
                            arm.position.Y + 42 * armSin);

                        //And give it a velocity of the direction we're aiming.
                        //Increase/decrease speed by changing 15.0f
                        bullet.velocity = new Vector2(
                            (float)Math.Cos(arm.rotation - MathHelper.PiOver2),
                            (float)Math.Sin(arm.rotation - MathHelper.PiOver2)) * 15.0f;
                    }
                    else //Facing left
                    {
                        float armCos = (float)Math.Cos(arm.rotation + MathHelper.PiOver2);
                        float armSin = (float)Math.Sin(arm.rotation + MathHelper.PiOver2);

                        //Set the initial position of our bullet at the end of our gun arm
                        //42 is obtained be taking the width of the Arm_Gun texture / 2
                        //and subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
                        bullet.position = new Vector2(
                            arm.position.X - 42 * armCos,
                            arm.position.Y - 42 * armSin);

                        //And give it a velocity of the direction we're aiming.
                        //Increase/decrease speed by changing 15.0f
                        bullet.velocity = new Vector2(
                           -armCos,
                           -armSin) * 15.0f;
                    }
                    return;
                }
            }
        }

        private void UpdateBullets()
        {
            //Check all of our bullets
            foreach (GameObject bullet in bullets)
            {
                //Only update them if they're alive
                if (bullet.alive)
                {
                    //Move our bullet based on it's velocity
                    bullet.position += bullet.velocity;

                    //Rectangle the size of the screen so bullets that
                    //fly off screen are deleted.
                    Rectangle screenRect = new Rectangle(0,0,(int)_cam.Position.X+_cam.ViewPort.Width, (int)_cam.Position.Y+_cam.ViewPort.Height*2);
                    if (!screenRect.Contains(new Point(
                        (int)bullet.position.X,
                        (int)bullet.position.Y)))
                    {
                        bullet.alive = false;
                        continue;
                    }

                    //Collision rectangle for each bullet -Will also be
                    //used for collisions with enemies.
                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.position.X - bullet.sprite.Width * 2,
                        (int)bullet.position.Y - bullet.sprite.Height * 2,
                        bullet.sprite.Width * 4,
                        bullet.sprite.Height * 4);


                    //Check for collisions with the enemies
                    foreach (Enemy enemy in level.enemies)
                    {
                        if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                        {
                            //We're going to want to put some enemy health reduction code here
                            //Enemy class needs a health member variable too
                            enemy.OnHit();
                            bullet.alive = false;
                        }
                    }

                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / level.TileWidth);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / level.TileWidth)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / level.TileHeight);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / level.TileHeight)) - 1;

                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = Level.GetCollision(x, y);

                            //If we collide with an Impassable or Platform tile
                            //then delete our bullet.
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    bullet.alive = false;
                            }
                        }
                    }
                }
            }
        }

        

        private void updateShooting(MouseState mouseState)
        {

            Vector2 aimDirection = arm.position - crosshair.position;
            arm.rotation = (float)Math.Atan2(aimDirection.Y, aimDirection.X) - (float)Math.PI / 2; //this will return the mouse angle(in radians).

            if (flip == SpriteEffects.FlipHorizontally) //Facing right
            {
                //If we try to aim behind our head then flip the
                //character around so he doesn't break his arm!
                if (arm.rotation < 0)
                    flip = SpriteEffects.None;

                //If we aren't rotating our arm then set it to the
                //default position. Aiming in front of us.
                if (arm.rotation == 0)
                    arm.rotation = MathHelper.PiOver2;
            }
            else //Facing left
            {
                //Once again, if we try to aim behind us then
                //flip our character.
                if (arm.rotation > 0)
                    flip = SpriteEffects.FlipHorizontally;

                //If we're not rotating our arm, default it to
                //aim the same direction we're facing.
                if (arm.rotation == 0)
                    arm.rotation = -MathHelper.PiOver2;
            }

            if (isShooting)
                FireBullet();

        }

        

        /// <summary>
        /// Called when the player has been hit.
        /// </summary>
        /// <param name="hitBy">
        /// The enemy who hit the player. This parameter is null if the player was
        /// not hit by an enemy (a hazard).
        /// </param>
        public void OnHit(Enemy hitBy)
        {
            isHit = true;
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
            sprite.PlayAnimation(flinchAnimation);

            if (health <= 0)
                OnKilled(hitBy);
            else
                StartInvincibilityFrames();
        }

        /*Your health is updated */
        public void UpdateHealth(int changeInHealth)
        {
            health += changeInHealth;
            if (health > 100)
                health = 100;
        }

        /// <summary>
        /// After being hit let the player get some breathing room
        /// </summary>
        private void StartInvincibilityFrames()
        {
            IFrames = MaxIFrames;
        }

        /// <summary>
        /// update Invincibility Frames
        /// </summary>
        private void UpdateInvincibilityFrames(GameTime gameTime)
        {
            IFrames -= gameTime.ElapsedGameTime.Milliseconds / 1000.0F;
            if (IFrames <= 0.0F)
                isHit = false;
        }

        /// <summary>
        /// When player has been hit his animation will flicker, showing that he is invincible for a short time
        /// </summary>
        private void spriteFlickerAnimation()
        {
            //make him flicker...somehow
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
            isAlive = false;

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
            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the resetAfterHit to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

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
            sprite.Draw(gameTime, spriteBatch, Position, flip, color);

            // Shooting related drawing.
            if (IsAlive)
            {

                //System.Diagnostics.Debug.WriteLine("Crosshair x = " + crosshair.position.X + " Crosshair y = " + crosshair.position.Y);
                spriteBatch.Draw(
                    crosshair.sprite,
                    crosshair.position,
                    null,
                    Color.White,
                    crosshair.rotation,
                    crosshair.center,
                    1.0f,
                    flip,
                    0);

                spriteBatch.Draw(
                    arm.sprite,
                    arm.position,
                    null,
                    Color.White,
                    arm.rotation,
                    arm.center,
                    1.0f,
                    flip,
                    0);

                //Draw the bullets
                foreach (GameObject bullet in bullets)
                {
                    if (bullet.alive)
                    {
                        spriteBatch.Draw(bullet.sprite,
                            bullet.position, Color.White);
                    }
                }

            }
        }

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }

    }//end class Player
}//end namespace Platformer
