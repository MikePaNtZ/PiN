#region File Description
//-----------------------------------------------------------------------------
// ActiveHero.cs
//
// Microsoft XNA Community Program Platform
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

        public bool IsBlocking
        {
            get { return ((HeroStateMachine)stateMachine).ShieldState.GetType() == typeof(BlockingState) || 
                         ((HeroStateMachine)stateMachine).ShieldState.GetType() == typeof(ShieldHitState); }
        }

        public Texture2D ShieldSprite
        {
            get { return shieldSprite; }
        }

        public float ShieldCharge
        {
            get { return shieldCharge; }
            set { shieldCharge = MathHelper.Clamp(value, 0.0F, maxShieldCharge); }
        }

        public float MaxShieldCharge
        {
            get { return maxShieldCharge; }
        }

        public float ShieldChargeRate
        {
            get { return shieldChargeRate; }
        }

        public float ShieldHitTime
        {
            get { return shieldHitTime; }
            set { shieldHitTime = MathHelper.Clamp(value, 0.0F, maxShieldHitTime); }
        }

        public float MaxShieldHitTime
        {
            get { return maxShieldHitTime; }
        }

        public float DepletedShieldTime
        {
            get { return depletedShieldTime; }
            set { depletedShieldTime = MathHelper.Clamp(value, 0.0F, maxDepletedShieldTime); }
        }

        public float MaxDepletedShieldTime
        {
            get { return maxDepletedShieldTime; }
        }
        
        

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
            shieldCharge = maxShieldCharge;
            depletedShieldTime = 0.0F;
            shieldHitTime = 0.0F;
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
            shieldCharge = maxShieldCharge;
            shieldHitTime = 0.0F;
            depletedShieldTime = 0.0F;
            iFrames = 0.0F;
        }

        public void SwapIn()
        {
            iFrames = Level.ActiveHero.InvincibilityFrames;
            depletedShieldTime = Level.ActiveHero.DepletedShieldTime;
            shieldCharge = Level.ActiveHero.ShieldCharge;
            shieldHitTime = Level.ActiveHero.ShieldHitTime;
            Position = Level.ActiveHero.Position;
            Velocity = Level.ActiveHero.Velocity;
            IsOnGround = Level.ActiveHero.IsOnGround;
            powerUpTime = Level.ActiveHero.powerUpTime;

            ((HeroStateMachine)stateMachine).Swap(((HeroStateMachine)Level.ActiveHero.stateMachine)); //clunky but it works
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

        protected Texture2D shieldSprite;
        protected float iFrames;
        protected float shieldCharge;
        protected float maxShieldCharge = 50.0F;
        protected float shieldChargeRate = 2.0F;
        protected float shieldHitTime;
        protected float maxShieldHitTime = 0.5F;
        protected float depletedShieldTime;
        protected float maxDepletedShieldTime = 2.0F;

    }//end class ActiveHero
}//end namespace PiN
