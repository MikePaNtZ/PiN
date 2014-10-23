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

namespace Platformer
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class MonsterA : Enemy
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterA(Level level, Vector2 position) : base(level, position)
        {
            MaxHealth = 8.0F;
            health = MaxHealth;
            enemyType = "MonsterA";
            LoadContent();
        }

    }
}

