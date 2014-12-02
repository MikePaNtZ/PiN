#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Program Platform
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
    /// Monster B derives from base enemy type
    /// </summary>
    class MonsterB : Enemy
    {
        public override int MaxHealth
        {
            get { return 17; }
        }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterB(Level level, Vector2 position) : base(level, position)
        {
            moveSpeed = 0.8F;
        }

        protected override void LoadContent()
        {
            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Die"), 0.07f, false);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Idle"), 0.15f, true); //placeholder
            killedSound = Level.Content.Load<SoundEffect>("Sounds/WomanFall");
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/WomanDying");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/WomanJump");

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

    }
}

