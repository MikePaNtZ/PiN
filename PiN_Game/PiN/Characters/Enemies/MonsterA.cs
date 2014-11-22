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
    /// Monster A derives from base enemy type
    /// </summary>
    class MonsterA : Enemy
    {
        public override int MaxHealth
        {
            get { return 12; }
        }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterA(Level level, Vector2 position) : base(level, position)
        {
            moveSpeed = 0.3F;
        }

        public override Vector2 ArmPosition
        {
            get
            {
                return new Vector2(rectangle.X + rectangle.Width / 2 + ((int)FaceDirection * rectangle.Width / 8),
                                   rectangle.Y + rectangle.Height / 2);
            }
        }


        protected override void LoadContent()
        {
            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterA/Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterA/Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterA/Die"), 0.07f, false);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/MonsterA/Idle"), 0.15f, true); //placeholder

            base.LoadContent();
        }

    }
}

