#region File Description
//-----------------------------------------------------------------------------
// PowerUp.cs
// Power up picked up by player
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
    class PowerUp : Consumable
    {
        public PowerUp(Level level, Vector2 position): base(level, position)
        {
            color = Color.Red;
            LoadContent();
        }

        /// <summary>
        /// Called when this item has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this item. He powers up
        /// </param>
        override public void OnCollected(Hero collectedBy)
        {
            collectedSound.Play();
            collectedBy.PowerUp();
        }
    }
}