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
    /// Monster B derives from base enemy type
    /// </summary>
    class MonsterB : Enemy
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterB(Level level, Vector2 position) : base(level, position)
        {
            MaxHealth = 6.0F;
            health = MaxHealth;
            enemyType = "MonsterB";
            LoadContent();
        }

    }
}

