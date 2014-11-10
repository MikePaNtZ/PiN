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
    /// Monster B derives from base enemy type
    /// </summary>
    class MonsterB : Enemy
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterB(Level level, Vector2 position) : base(level, position)
        {
            maxHealth = 6;
        }

        protected override void LoadContent()
        {
            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Die"), 0.07f, false);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterB/Idle"), 0.15f, true); //placeholder
            
            base.LoadContent();
        }

    }
}

