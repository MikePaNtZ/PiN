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

namespace PiN
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy: GameCharacter
    {

        /// <summary>
        /// returns line of sight to player
        /// </summary>
        public Vector2 LineOfSightToHero
        {
            get { return Level.ActiveHero.Position - Position; }
        }

        public Vector2 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// if player is within this distance than transition to tracking state from searching state
        /// </summary>
        public virtual float MinTrackDistance
        {
            get { return 500.0F; }
        }

        /// <summary>
        /// if player is within this distance than you can attack
        /// </summary>
        public virtual float MaxAttackDistance
        {
            get { return 200.0F; }
        }

        /// <summary>
        /// if health is less than this percent of max health than kamikaze
        /// </summary>
        public virtual float KamikazeThresholdPercent
        {
            get { return 0.4F; }
        }

        /// <summary>
        /// How long to shoot
        /// </summary>
        public float AttackTime
        {
            get { return attackTime; }
            set { attackTime = MathHelper.Clamp(value, 0, MaxAttackTime); }
        }

        /// <summary>
        /// How long to wait between shots
        /// </summary>
        public virtual float MaxAttackTime
        {
            get { return 0.2F; }
        }

        /// <summary>
        /// How long this enemy has been waiting to shoot again.
        /// </summary>
        public float AttackWaitTime
        {
            get { return attackWaitTime; }
            set { attackWaitTime = MathHelper.Clamp(value, 0, MaxAttackWaitTime); }
        }

        /// <summary>
        /// How long to wait between shots
        /// </summary>
        public virtual float MaxAttackWaitTime
        {
            get { return 1.0f; }
        }

        public float WaitTime
        {
            get { return waitTime; }
            set { waitTime = MathHelper.Clamp(value, 0, maxWaitTime); }
        }

        public float MaxWaitTime
        {
            get { return maxWaitTime; }
        }

        public override bool IsJumping
        {
            get
            {
                return stateMachine.MainState.GetType() == typeof(EnemyJumpingState);
            }
        }

        public override bool IsAttacking
        {
            get
            {
                return stateMachine.ShooterState.GetType() == typeof(EnemyFiringState);
            }
        }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 initialPosition): base(level, initialPosition)
        {
            LoadContent();
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        protected virtual void LoadContent()
        {
            explosionAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/explosion"), 0.1f, false); //false means the animation is not going to loop
            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/Dying");
            // Temporary hurt sound. We probably want to use something different in the future.
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");

            // Calculate bounds within texture size.            
            // TODO It needs to be more clear what this is doing, and why it is done here. It is for collision detection.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load enemy default weapon
            weapon = new EnemyGun(this);

            stateMachine = new EnemyStateMachine(this);
        }


        /// <summary>
        /// Updates the ai and position of enemy.
        /// TODO We should refactor this to use the state pattern 
        /// design pattern: http://sourcemaking.com/design_patterns/state
        /// </summary>
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (!IsAlive)
                return;

            base.Update(gameTime, gameInputs);
        }


        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            // Shooting related drawing.
            if (IsAlive)
            {
                weapon.Draw(gameTime, spriteBatch);
            }
        }

        /// <summary>
        /// enemy dies
        /// </summary>
        public override void OnKilled(GameObject killedBy)
        {
            base.OnKilled(killedBy);
            SpawnRandomConsumable();
        }

        /// <summary>
        /// spawns a random consumable at the place the enemy dies
        /// </summary>
        protected void SpawnRandomConsumable()
        {
            Point point;
            point.Y = BoundingRectangle.Top + BoundingRectangle.Height / 3;
            point.X = BoundingRectangle.Center.X;

            Random random = new Random();
            int rand = random.Next(100);
            if (rand < 30)
                Level.SpawnConsumable(point.X, point.Y, "HealthConsumable");
            else if (rand > 90)
                Level.SpawnConsumable(point.X, point.Y, "PowerUp");
        }

        /// <summary>
        /// Enemy explosion animation.
        /// </summary>
        protected Animation explosionAnimation;

        protected Vector2 target;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        protected float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        protected float maxWaitTime = 0.5f;

        private float attackTime;
        private float attackWaitTime;
    }
}
