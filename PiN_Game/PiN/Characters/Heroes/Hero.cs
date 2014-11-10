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

namespace PiN
{
    /// <summary>
    /// ActiveHero base class. This is a GameCharacter that is controllable by the player.
    /// </summary>
    class Hero : GameCharacter
    {

        public float InvincibilityFrames 
        { 
            get { return iFrames; }
            set { iFrames = value; }
        }

        public bool IsHit
        {
            get { return iFrames > 0.0F; }
        }

        public override bool IsJumping 
        {
            get
            {
                return stateMachine.MainState.GetType() == typeof(HeroJumpingState);
            }
        }

        public override bool IsAttacking
        {
            get
            {
                return stateMachine.ShooterState.GetType() == typeof(HeroFiringState);
            }
        }

        protected float iFrames;

        public readonly Color[] isHitColors = 
        {
            Color.Transparent,
            Color.White,
        };

        /// <summary>
        /// Constructs a new player.
        /// </summary>
        public Hero(Level level, Vector2 initialPosition): base(level, initialPosition)
        {
            iFrames = 0.0F;
            LoadContent();
        }

        protected virtual void LoadContent()
        {
            stateMachine = new HeroStateMachine(this);
        }

        /// <summary>
        /// Resets the hero
        /// </summary>
        public override void Reset(Vector2 position)
        {
            base.Reset(position);
            iFrames = 0.0F;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(InputHandler gameInputs)
        {
            //the player is blocking by holding down the right mouse button
            IsBlocking = (gameInputs.MouseState.RightButton == ButtonState.Pressed) & (gameInputs.PreviousMouseState.RightButton == ButtonState.Pressed);

        }

        /// <summary>
        /// Handles input, performs physics, and animates the player resetAfterHit.
        /// </summary>
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            // Handle inputs
            GetInput(gameInputs);

        }//end Update method

        

        public void SwapIn()
        {
            iFrames = Level.ActiveHero.InvincibilityFrames;
            Position = Level.ActiveHero.Position;
            Velocity = Level.ActiveHero.Velocity;
            IsOnGround = Level.ActiveHero.IsOnGround;
            IsBlocking = Level.ActiveHero.IsBlocking;
            powerUpTime = Level.ActiveHero.powerUpTime;
            stateMachine.MainState = Level.ActiveHero.stateMachine.MainState;
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            stateMachine.OnReachedExit();
        }

        public override void determineColor(GameTime gameTime)
        {
            if (IsHit)
            {
                float s = (float)gameTime.TotalGameTime.TotalSeconds * 100.0f;
                int colorIdx = (int)s % isHitColors.Length;
                color = isHitColors[colorIdx];
            }
            else
                base.determineColor(gameTime);
        }

    }//end class ActiveHero
}//end namespace PiN
