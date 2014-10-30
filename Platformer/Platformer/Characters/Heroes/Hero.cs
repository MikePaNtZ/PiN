#region File Description
//-----------------------------------------------------------------------------
// ActiveHero.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer
{
    /// <summary>
    /// ActiveHero base class. This is a GameCharacter that is controllable by the player.
    /// </summary>
    class Hero : GameCharacter
    {

        private const float MaxIFrames = 1.5F;
        private float IFrames;

        /// <summary>
        /// Constructs a new player.
        /// </summary>
        public Hero(Level level, Vector2 initialPosition, Texture2D defaultTexture): base(level, initialPosition, defaultTexture)
        {
            LoadContent();
            Reset(initialPosition);
        }

        // This is where you load any content that is specific to this player class only.
        protected override void LoadContent()
        {
            // Load the activeHero's default weapon
            base.LoadContent();
            weapon = new Gun(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"), this);
        }

        /// <summary>
        /// Resets the player's invincibility frames to 0
        /// </summary>
        public override void Reset(Vector2 position)
        {
            IFrames = 0.0F;
            Position = position;
            Velocity = Vector2.Zero;
            IsAlive = true;
            IsHit = false;
            health = 100;
            sprite.LoadAnimation(idleAnimation);
            powerUpTime = 0.0f;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(InputHandler gameInputs)
        {
            // If any digital horizontal movement input is found, override the analog movement.
            if (gameInputs.KeyboardState.IsKeyDown(Keys.Left) || gameInputs.KeyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gameInputs.KeyboardState.IsKeyDown(Keys.Right) || gameInputs.KeyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if player is firing weapon
            IsAttacking = ((gameInputs.MouseState.LeftButton == ButtonState.Pressed) && (oldMouseState.LeftButton != ButtonState.Pressed));
            //the player is blocking by holding down the right mouse button
            IsBlocking = (gameInputs.MouseState.RightButton == ButtonState.Pressed) & (oldMouseState.RightButton == ButtonState.Pressed);

            // Check if the player wants to jump.
            IsJumping = gameInputs.KeyboardState.IsKeyDown(Keys.Space) || gameInputs.KeyboardState.IsKeyDown(Keys.Up) || gameInputs.KeyboardState.IsKeyDown(Keys.W);

            weapon.UpdateWeaponState(gameInputs);

            oldMouseState = gameInputs.MouseState;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player resetAfterHit.
        /// </summary>
        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
            // Handle inputs
            GetInput(gameInputs);

            // apply physics to the character.
            physEngine.ApplyPhysics(gameTime);

            // determine the animation to use based on player action.
            determineAnimation(gameTime);

            // Clear inputs.
            movement = 0.0f;
            IsJumping = false;

            // Check for power up.
            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
            
            // Update the player's weapon.
            weapon.UpdateWeaponState(gameInputs);
            if (IsAttacking)
                weapon.PerformNormalAttack();

        }//end Update method

        protected override void determineAnimation(GameTime gameTime)
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
            if (IsAlive && IsHit)
            {
                UpdateInvincibilityFrames(gameTime);
                spriteFlickerAnimation();
            }
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
            IsHit = true;
            if (hitBy != null)
            {
                UpdateHealth(-10);
                hurtSound.Play();
            }
            else
            {
                UpdateHealth(-5);
                hurtSound.Play();
            }
            sprite.LoadAnimation(flinchAnimation);
            if (Health <= 0)
                OnKilled(hitBy);
            else
                StartInvincibilityFrames();
        }

        //public void OnInjured()
        //{
        //    IsHit = true;

        //    health -= 10;

        //    if (health <= 0)
        //    {
        //        killedSound.Play();
        //        sprite.LoadAnimation(dieAnimation);
        //        IsAlive = false;
        //    }
        //}


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
                IsHit = false;
        }

        /// <summary>
        /// When player has been hit his animation will flicker, showing that he is invincible for a short time
        /// </summary>
        private void spriteFlickerAnimation()
        {
            //make him flicker...somehow
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.LoadAnimation(celebrateAnimation);
        }

        private MouseState oldMouseState;

    }//end class ActiveHero
}//end namespace Platformer
