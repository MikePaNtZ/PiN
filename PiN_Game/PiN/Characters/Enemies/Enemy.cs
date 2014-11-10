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
        public Vector2 LineOfSight
        {
            get { return Level.ActiveHero.Position - Position; }
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

        public override Rectangle BoundingRectangle
        {
            get
            {
                return rectangle;
            }
        }

        public override bool IsJumping
        {
            get
            {
                return stateMachine.MainState.GetType() == typeof(EnemyJumpingState);
            }
        }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 initialPosition): base(level, initialPosition)
       { 
            health = maxHealth;
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

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        protected float MoveSpeed = 40.0F;

        /// <summary>
        /// if player is within this distance than transition to tracking state from searching state
        /// </summary>
        protected float minTrackDistance = 500.0F;
        public float MinTrackDistance
        {
            get { return minTrackDistance; }
        }

        /// <summary>
        /// if player is within this distance than you can attack
        /// </summary>
        protected float maxAttackDistance = 200.0F;
        public float MaxAttackDistance
        {
            get { return maxAttackDistance; }
        }

        /// <summary>
        /// if health is less than this percent of max health than kamikaze
        /// </summary>
        protected float kamikazeThresholdPercent = 0.4F;
        public float KamikazeThresholdPercent
        {
            get { return kamikazeThresholdPercent; }
        }

        /// <summary>
        /// How long to wait between shots
        /// </summary>
        protected float maxAttackWaitTime = 1.0f;
        public float MaxAttackWaitTime
        {
            get { return maxAttackWaitTime; }
        }

        /// <summary>
        /// How long this enemy has been waiting to shoot again.
        /// </summary>
        protected float attackWaitTime;
        public float AttackWaitTime
        {
            get { return attackWaitTime; }
            set { attackWaitTime = MathHelper.Clamp(value, 0, maxAttackWaitTime); }
        }

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        protected float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        protected float maxWaitTime = 0.5f;
        /// <summary>
        /// The enemy is currently attacking.
        /// </summary>
        public bool alreadyAttacking = false;
    }
}
