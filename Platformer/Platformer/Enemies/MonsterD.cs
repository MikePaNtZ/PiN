﻿#region File Description
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
    /// Monster D derives from base enemy type
    /// </summary>
    class MonsterD : Enemy
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public MonsterD(Level level, Vector2 position) : base(level, position)
        {
            // Base enemies just default to look like monster a with 5 health. 
            health = 2.0f;
            enemyType = "MonsterD";
            LoadContent();
        }

    }
}

